using Azure.Storage.Blobs;
using Core.Modelos;
using Core.Servicios.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Core.Servicios.Impl
{
    public class Support: ISupport
    {
        private IDbConnection conexionMD;
        private IDbTransaction transaction;
        public ConexionConfig conf;
        public Support(ConexionConfig conf)
        {
            this.conf = conf;
        }
        public Support(IDbConnection conn, IDbTransaction transaction)
        {
            this.conexionMD = conn;
            this.transaction = transaction;
        }
        public async Task<ResponseSupport> NuevoTicket(PeticionSoporte req)
        {
            try
            {
                using (IDbConnection _conn = new SqlConnection(conf.SQLServerPool))
                {
                    _conn.Open();
                    using (var transaction = _conn.BeginTransaction())
                    {
                        try
                        {
                            ResponseSupport response = new ResponseSupport();
                            response.status = "Error";
                            Repositorios.Support repo = new Repositorios.Support(_conn, transaction);
                            int ID = await repo.AgregaTicket(req);
                            if (ID > 0) {
                                string connectionString = this.conf.AzureProdConexion;
                                BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                                string containerName = this.conf.ContainerProductosIMG;

                                foreach (Microsoft.AspNetCore.Http.IFormFile file in req.Archivos) {
                                    // Create the container and return a container client object
                                    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                                    string extension = System.IO.Path.GetExtension(file.FileName);
                                    string fileName = Guid.NewGuid().ToString() + extension;
                                    BlobClient blobClient = containerClient.GetBlobClient(fileName);
                                    using (var stream = file.OpenReadStream())
                                    {
                                        await blobClient.UploadAsync(stream, true);
                                    }
                                    await repo.AgregaImg(ID.ToString(), fileName);
                                }
                                response.status = "OK";
                                response.mensaje = "Ticket numero #" + ID + " creado con exito, pronto tendra respuesta";
                                transaction.Commit();
                            }

                            _conn.Close();
                            return response;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            _conn.Close();
                            throw new Exception(ex.Message);
                        }
                    }


                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<List<ResponseMensajes>> GetMensajes(string idUsuario)
        {
            try
            {
                using (IDbConnection _conn = new SqlConnection(conf.SQLServerPool))
                {
                    _conn.Open();
                    using (var transaction = _conn.BeginTransaction())
                    {
                        try
                        {
                            Repositorios.Support repo = new Repositorios.Support(_conn, transaction);
                            List<ResponseMensajes> mensajes = await repo.ObtenerMensajes(idUsuario);
                            transaction.Commit();
                            _conn.Close();
                            return mensajes;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            _conn.Close();
                            throw new Exception(ex.Message);
                        }
                    }


                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<ResponseSupport> SetMensajeLiedo(string idMensaje)
        {
            try
            {
                using (IDbConnection _conn = new SqlConnection(conf.SQLServerPool))
                {
                    _conn.Open();
                    using (var transaction = _conn.BeginTransaction())
                    {
                        try
                        {
                            ResponseSupport response = new ResponseSupport();
                            response.status = "Error";
                            Repositorios.Support repo = new Repositorios.Support(_conn, transaction);
                            response.status = "OK";
                            await repo.SetLeido(idMensaje);

                            transaction.Commit();
                            _conn.Close();
                            return response;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            _conn.Close();
                            throw new Exception(ex.Message);
                        }
                    }


                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<ResponseInfoMensaje> GetInfoMensaje(string idMensaje)
        {
            try
            {
                using (IDbConnection _conn = new SqlConnection(conf.SQLServerPool))
                {
                    _conn.Open();
                    using (var transaction = _conn.BeginTransaction())
                    {
                        try
                        {
                            Repositorios.Support repo = new Repositorios.Support(_conn, transaction);
                            ResponseInfoMensaje response = await repo.ObtenerInfoMensaje(idMensaje);
                            response.imagenes = new List<ObjetoImagenDatos>();
                            List<ImagenesRespuesta> imagenes = await repo.ObtenerImagenesResponse(idMensaje);
                            string connectionString = this.conf.AzureProdConexion;
                            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                            string containerName = this.conf.ContainerProductosIMG;

                            // Create the container and return a container client object
                            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                            foreach (ImagenesRespuesta img in imagenes) {
                                ObjetoImagenDatos objeto = new ObjetoImagenDatos();
                                objeto.guid = img.pathImg;
                                string fileName = img.pathImg;
                                byte[] array = { };
                                // Get a reference to a blob
                                BlobClient blobClient = containerClient.GetBlobClient(fileName);
                                using (var ms = new MemoryStream())
                                {
                                    blobClient.DownloadTo(ms);
                                    objeto.bytes = ms.ToArray();

                                    string[] valores = fileName.Split('.');
                                    if (valores.Length == 2)
                                    {
                                        objeto.tipo = "image/" + valores[1];
                                    }
                                }
                                response.imagenes.Add(objeto);
                            }

                            transaction.Commit();
                            _conn.Close();
                            return response;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            _conn.Close();
                            throw new Exception(ex.Message);
                        }
                    }


                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
