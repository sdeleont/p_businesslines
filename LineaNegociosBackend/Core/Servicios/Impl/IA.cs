using Core.Modelos;
using Core.Servicios.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Discovery;
using Google.Apis.Services;
//using Google.Apis.Discovery.v1;
//using Google.Apis.Discovery.v1.Data;
using Google.Cloud.DocumentAI.V1;
using Google.Apis.Auth.OAuth2;
using System.IO;
using Google.Protobuf;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Core.Servicios.Impl
{
    public class IA: IIA
    {
        private IDbConnection conexionMD;
        private IDbTransaction transaction;
        public ConexionConfig conf;
        public IA(ConexionConfig conf)
        {
            this.conf = conf;
        }
        public IA(IDbConnection conn, IDbTransaction transaction)
        {
            this.conexionMD = conn;
            this.transaction = transaction;
        }
        public static async Task<string> GetAccessTokenFromJSONKeyAsync(string jsonKeyFilePath, params string[] scopes)
        {
            using (var stream = new FileStream(jsonKeyFilePath, FileMode.Open, FileAccess.Read))
            {
                return await GoogleCredential
                    .FromFile("") // Loads key file  
                    .CreateScoped(scopes) // Gathers scopes requested  
                    .UnderlyingCredential // Gets the credentials  
                    .GetAccessTokenForRequestAsync(); // Gets the Access Token  
            }
        }
        public string ExtraeCadena(string cad, string start, string end) {
            string cadena = "";
            try {
                cadena = cad.Substring(Convert.ToInt32(start), Convert.ToInt32(end) - Convert.ToInt32(start)) + " \n";
            } catch (Exception ex) {
                cadena = "Fallo en " + Convert.ToInt32(start) + "," + Convert.ToInt32(end) + " \n";
            }
            return cadena;
        }
        public string GetContentType(Microsoft.AspNetCore.Http.IFormFile Archivo) {
            string tipo = "application/pdf";
            string extension = System.IO.Path.GetExtension(Archivo.FileName).ToLower();
            if (extension.Equals(".pdf")) {
                tipo = "application/pdf";
            } else if (extension.Equals(".jpeg"))
            {
                tipo = "image/jpeg";
            } else if (extension.Equals(".jpg"))
            {
                tipo = "image/jpg";
            } else if (extension.Equals(".png"))
            {
                tipo = "image/png";
            }
            //application/pdf
            //image/jpeg
            //image/jpg
            //image/png

            return tipo;
        }
        public async Task<ResponseIA> AplicarInteligenciaArtificial(RequestImageIA requestIA)
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
                            ResponseIA response = new ResponseIA();
                            response.status = "ERROR";
                            // ResponseIA response = new ResponseIA();
                            //FileInputStream fileInputStream = new FileInputStream(KEY_FILE_PATH);
                            // ServiceAccountCredential credential = ServiceAccountCredentials.fromStream(fileInputStream);
                            string token = await GoogleCredential.FromFile("Secret key file").CreateScoped("https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/cloud-platform").UnderlyingCredential.GetAccessTokenForRequestAsync();

                            //GoogleCredential googleCredential = GoogleCredential.FromFile("Secret key file");
                            // Hacer la llamada directamente al API sin utilizar librerias de Cloud

                            RequestGoogleIA requestObject = new RequestGoogleIA();
                            //OCR
                            //requestObject.name = "Secret key";
                            //FORM PARSER
                            requestObject.name = "Secret key URL";

                            requestObject.rawDocument = new DocumentG();
                            requestObject.rawDocument.mimeType = "application/pdf";
                            using (var ms = new MemoryStream())
                            {
                                requestIA.Archivo.CopyTo(ms);
                                var fileBytes = ms.ToArray();
                                string base64File = Convert.ToBase64String(fileBytes);
                                requestObject.rawDocument.content = base64File;
                            }
                            //requestObject.rawDocument.content = "Secret key";
                            //OCR
                            //var client = new RestClient("Secret key");
                            //FORM PARSER
                            var client = new RestClient("Secret key");
                            var request = new RestRequest(Method.POST);
                            request.AddHeader("Content-Type", "application/json");
                            request.AddHeader("Authorization", "Bearer " + token);
                            request.AddParameter("undefined", JsonConvert.SerializeObject(requestObject), ParameterType.RequestBody);

                            IRestResponse responsePost = client.Execute(request);
                            if (responsePost.StatusCode != HttpStatusCode.OK)
                            {
                                response.status = "ERROR";
                                response.mensaje = "Ocurrio un error analizando el documento";
                            }
                            else
                            {
                                dynamic api = JObject.Parse(responsePost.Content);

                                JObject objeto = JObject.Parse(responsePost.Content);

                                List<ParCampoIA> listPares = new List<ParCampoIA>();

                                ResponseDocumentApi objetoResp = objeto.ToObject<ResponseDocumentApi>();
                                string strTexto = objetoResp.document.text + " \n";
                                foreach (Page page in objetoResp.document.pages)
                                {
                                    if (page.formFields != null)
                                    {
                                        foreach (FormField formField in page.formFields)
                                        {
                                            string fieldName = "";
                                            string fieldValue = "";
                                            foreach (TextSegments textSeg in formField.fieldName.textAnchor.textSegments)
                                            {
                                                fieldName += ExtraeCadena(objetoResp.document.text, textSeg.startIndex, textSeg.endIndex);
                                            }
                                            foreach (TextSegments textSeg in formField.fieldValue.textAnchor.textSegments)
                                            {
                                                fieldValue += ExtraeCadena(objetoResp.document.text, textSeg.startIndex, textSeg.endIndex);
                                            }
                                            ParCampoIA par = new ParCampoIA();
                                            par.formName = fieldName;
                                            par.formValue = fieldValue;
                                            listPares.Add(par);
                                        }
                                    }
                                }
                                // empiezo a analizar
                                Repositorios.LineaNegocio repo = new Repositorios.LineaNegocio(_conn, transaction);
                                List<CampoLineaNegocioCompleto> campos = await repo.ObtenerCamposLineaCompleto(requestIA.idLineaNegocio);
                                List<ParRetornoIA> listaRetornos = new List<ParRetornoIA>();
                                foreach (CampoLineaNegocioCompleto campo in campos)
                                {
                                    if (campo.campoEnIA != null && !campo.campoEnIA.Equals(""))
                                    {
                                        foreach (ParCampoIA par in listPares)
                                        {
                                            if (campo.campoEnIA.Trim().ToUpper().Equals(par.formName.Trim().ToUpper()))
                                            {
                                                ParRetornoIA parR = new ParRetornoIA();
                                                parR.campo = campo.campoTabla;
                                                /*
                                                if (campo.tipoenbd.Equals("INTEGER") || campo.tipoenbd.Equals("REAL") || campo.tipoenbd.Equals("VARCHAR(50)"))
                                                {
                                                    parR.valor = Regex.Replace(par.formValue.Trim().Replace(" ", ""), @"[^0-9]+", "");
                                                }
                                                else {

                                                }
                                                */
                                                parR.valor = par.formValue.Trim();
                                                listaRetornos.Add(parR);
                                            }
                                        }
                                    }
                                }
                                //response.mensaje = "Obtenido con Exito Token:" + token + ", ImageLength " + requestIA.Archivo.Length + ", Name: " + requestIA.Archivo.FileName ;
                                response.mensaje = "Obtenido con Exito";
                                response.status = "OK";
                                response.pares = listaRetornos;
                                transaction.Commit();
                                // esta parte va estar descomentada solo temporalmente
                                /*
                                foreach (Page page in objetoResp.document.pages)
                                {
                                    strTexto += "-----------------------------------------------------------------------------------------Todo el Texto Layout Principal----------------------------" + " \n";
                                    strTexto += ExtraeCadena(objetoResp.document.text, "0", page.layout.textAnchor.textSegments[0].endIndex);
                                    strTexto += "-----------------------------------------------------------------------------------------Inician Bloques----------------------------" + " \n";
                                    foreach (Block block in page.blocks)
                                    {
                                        strTexto += "-----------------------------------------------------------------------------------------Nuevo Bloque-----------------------------------------------------------" + " \n";

                                        foreach (TextSegments textSeg in block.layout.textAnchor.textSegments)
                                        {
                                            strTexto += "-----------------------------------------------------------------------------------------TextSegmento-----------------------------------------------------------" + " \n";
                                            strTexto += ExtraeCadena(objetoResp.document.text, textSeg.startIndex, textSeg.endIndex);
                                        }
                                    }
                                    foreach (Paragraph paragraph in page.paragraphs)
                                    {
                                        strTexto += "-----------------------------------------------------------------------------------------Nuevo Parrafo-----------------------------------------------------------" + " \n";
                                        foreach (TextSegments textSeg in paragraph.layout.textAnchor.textSegments)
                                        {
                                            strTexto += "-----------------------------------------------------------------------------------------TextSegmento-----------------------------------------------------------" + " \n";
                                            strTexto += ExtraeCadena(objetoResp.document.text, textSeg.startIndex, textSeg.endIndex);
                                        }
                                    }
                                    foreach (Line line in page.lines)
                                    {
                                        strTexto += "-----------------------------------------------------------------------------------------Nueva Line-----------------------------------------------------------" + " \n";
                                        foreach (TextSegments textSeg in line.layout.textAnchor.textSegments)
                                        {
                                            strTexto += "-----------------------------------------------------------------------------------------TextSegmento-----------------------------------------------------------" + " \n";
                                            strTexto += ExtraeCadena(objetoResp.document.text, textSeg.startIndex, textSeg.endIndex);
                                        }
                                    }
                                    foreach (Token tok in page.tokens)
                                    {
                                        strTexto += "-----------------------------------------------------------------------------------------Nuevo Token-----------------------------------------------------------" + " \n";
                                        foreach (TextSegments textSeg in tok.layout.textAnchor.textSegments)
                                        {
                                            strTexto += "-----------------------------------------------------------------------------------------TextSegmento-----------------------------------------------------------" + " \n";
                                            strTexto += ExtraeCadena(objetoResp.document.text, textSeg.startIndex, textSeg.endIndex);
                                        }
                                    }
                                    if (page.formFields != null)
                                    {
                                        foreach (FormField formField in page.formFields)
                                        {
                                            strTexto += "-----------------------------------------------------------------------------------------Nuevo FormField-----------------------------------------------------------" + " \n";

                                            foreach (TextSegments textSeg in formField.fieldName.textAnchor.textSegments)
                                            {
                                                strTexto += "-----------------------------------------------------------------------------------------TextSegmento FieldName-----------------------------------------------------------" + " \n";
                                                strTexto += ExtraeCadena(objetoResp.document.text, textSeg.startIndex, textSeg.endIndex);
                                            }
                                            foreach (TextSegments textSeg in formField.fieldValue.textAnchor.textSegments)
                                            {
                                                strTexto += "-----------------------------------------------------------------------------------------TextSegmento FieldValue-----------------------------------------------------------" + " \n";
                                                strTexto += ExtraeCadena(objetoResp.document.text, textSeg.startIndex, textSeg.endIndex);
                                            }
                                        }
                                    }



                                }

                                string valores = Convert.ToString(api.document);
                                response.response = strTexto;
                                */
                                // esta parte va estar descomentada solo temporalmente
                            }




                            if (responsePost.StatusCode != HttpStatusCode.OK)
                            {
                                //response.response = valores;
                            }
                            else
                            {

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
    }
}






// Este codigo de abajo da error por que no logra resolver las DNS
/*
String projectId = "Secret key";
String location = "us"; // Format is "us" or "eu".
String processorId = "Secret key";
String filePath = "Certificado.pdf";

//Environment.SetEnvironmentVariable("GRPC_VERBOSITY", "debug");
//Environment.SetEnvironmentVariable("GRPC_TRACE", "api,http,cares_resolver,cares_address_sorting");
//Environment.SetEnvironmentVariable("GRPC_DNS_RESOLVER", "native");
DocumentProcessorServiceClientBuilder builder = new DocumentProcessorServiceClientBuilder();
builder.Endpoint = "Secret key";
builder.CredentialsPath = "Secret key";
builder.Scopes = null;

DocumentProcessorServiceClient documentProcessorServiceClient = builder.Build();

String name = "Secret key";
//byte[] imageFileData = File.ReadAllBytes(filePath);
byte[] imageFileData = Convert.FromBase64String("Secret key");
ByteString content = ByteString.CopyFrom(imageFileData);
// seteo el content
RawDocument document = new RawDocument();
document.Content = content;
document.MimeType = "application/pdf";

ProcessRequest processRequest = new ProcessRequest();
processRequest.RawDocument = document;
processRequest.Name = name;

ProcessResponse processResponse = documentProcessorServiceClient.ProcessDocument(processRequest);
Document document1 = processResponse.Document;

string text = document1.Text;
response.response = text;

/*
string endpoint = "Secret key";
string serviceEndpoint = "https://documentai.googleapis.com";
var service = new DiscoveryService(new BaseClientService.Initializer
{
    ApplicationName = "Secret key",
    ApiKey = "Secret key",
});
var result = await service.Apis.List().ExecuteAsync();
// Display the results.
if (result.Items != null)
{
    foreach (DirectoryList.ItemsData api in result.Items)
    {
        string a = api.Id + " - " + api.Title;
        Console.WriteLine(api.Id + " - " + api.Title);
    }
}
*/