using Azure.Storage.Blobs;
using Core.Modelos;
using Core.Servicios.Interfaces;
using ExcelDataReader;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core.Servicios.Impl
{
    public class LineaNegocio: ILineaNegocio
    {
        private IDbConnection conexionMD;
        private IDbTransaction transaction;
        public ConexionConfig conf;
        public LineaNegocio(ConexionConfig conf)
        {
            this.conf = conf;
        }
        public LineaNegocio(IDbConnection conn, IDbTransaction transaction)
        {
            this.conexionMD = conn;
            this.transaction = transaction;
        }
        public async Task<ResponseAnalisisFile> Analiza(InformacionLineaNegocio infoLineaNegocio)
        {
            try
            {
                

                using (IDbConnection _conn = new SqlConnection(conf.SQLServerPool)) //inicio la conexion
                {
                    _conn.Open();
                    using (var transaction = _conn.BeginTransaction()) //inicio la transaccion
                    {
                        try
                        {
                            // se crea el objeto de respuesta por parte del servidor
                            ResponseAnalisisFile response = new ResponseAnalisisFile("Error","Desconocido");
                            

                            //1. Traer de base de datos la Metada correspondiente de configuracion en base al archivo que se analizara
                            Repositorios.LineaNegocio repo = new Repositorios.LineaNegocio(_conn, transaction);
                            List<TiposDato> listaTiposDato = await repo.ObtenerTiposDato();

                            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                            using (var stream1 = infoLineaNegocio.Archivo.OpenReadStream())
                            {
                                using (var reader = ExcelReaderFactory.CreateReader(stream1))
                                {
                                    reader.Read(); //nos posicionamos en la primera fila
                                    //se llena una lista con los encabezados para verificar que coincidan
                                    List<ColumnaAnalizada> columnas = new List<ColumnaAnalizada>();
                                    for (int i = 0; i < reader.FieldCount; i++) {
                                        string encabezado = (reader.GetValue(i) != null) ? reader.GetValue(i).ToString() : "";
                                        if (encabezado.Trim().Equals(""))
                                            i = reader.FieldCount;
                                        else
                                        {
                                            columnas.Add(new ColumnaAnalizada(encabezado.Trim()));
                                        }
                                    }
                                    int linea = 1;
                                    int lineasEnBlanco = 0;
                                    int numRegistros = 0;

                                    while (reader.Read()) //Each row of the file
                                    {
                                        linea++;
                                        int cont = 0;
                                        //verificacion de linea vacia
                                        bool esVacia = true;
                                        for (int i = 0; i < columnas.Count; i++)
                                        {
                                            if (reader.GetValue(i) != null && !reader.GetValue(i).ToString().Trim().Equals(""))
                                            {
                                                esVacia = false; //levantamos bandera
                                                i = columnas.Count; //terminamos el ciclo
                                            }
                                        }
                                        if (!esVacia)
                                        {
                                            numRegistros++;
                                        }
                                        else
                                        {
                                            lineasEnBlanco++;
                                        }
                                    }
                                    response.status = "OK";
                                    response.mensaje = "Analisis realizado";
                                    response.columnas = columnas;
                                    response.columnasEncontradas = columnas.Count;
                                    response.lineasVacias = lineasEnBlanco;
                                    response.registrosEncontrados = numRegistros;

                                    transaction.Commit();
                                }
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
            return null;
        }
        public async Task<List<TipoNegocio>> ObtenerTiposNegocio()
        {
            try
            {
                using (IDbConnection _conn = new SqlConnection(conf.SQLServerPool))
                {
                    try
                    {
                        List<TipoNegocio> tiposNegocios = new List<TipoNegocio>();
                        Repositorios.LineaNegocio repo = new Repositorios.LineaNegocio(_conn);
                        tiposNegocios = await repo.ObtenerTiposNegocio();
                        _conn.Close();
                        return tiposNegocios;
                    }
                    catch (Exception ex)
                    {
                        _conn.Close();
                        throw new Exception(ex.Message);
                    }

                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<List<TiposDatoD>> ObtenerTiposDato()
        {
            try
            {
                using (IDbConnection _conn = new SqlConnection(conf.SQLServerPool))
                {
                    try
                    {
                        List<TiposDatoD> tiposNegocios = new List<TiposDatoD>();
                        Repositorios.LineaNegocio repo = new Repositorios.LineaNegocio(_conn);
                        tiposNegocios = await repo.ObtenerTiposDatoD();
                        _conn.Close();
                        return tiposNegocios;
                    }
                    catch (Exception ex)
                    {
                        _conn.Close();
                        throw new Exception(ex.Message);
                    }

                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        
        public async Task<ConfiguracionLinea> ObtenerLineaNegocioDatos(BusquedaData busqueda, string idLinea, string inicioPuntero, string finPuntero)
        {
            try
            {
                using (IDbConnection _conn = new SqlConnection(conf.SQLServerPool))
                {
                    try
                    {
                        Repositorios.LineaNegocio repo = new Repositorios.LineaNegocio(_conn);
                        ConfiguracionLinea response = new ConfiguracionLinea();
                        response = await repo.ObtenerLineaNegocioPorID(idLinea);
                        response.campos = await repo.ObtenerCamposLineaCompleto(idLinea);
                        string llavePrimaria = response.campos.Find(x => x.esLlavePrimaria.Equals("S")).descripcionCampo;
                        string queryWhere = "( ";
                        string texto = busqueda.texto.Replace("'", "''");
                        for (int i = 1; i < response.campos.Count; i++) {
                            if (i == 1) {
                                queryWhere += response.campos[i].campoTabla + " like '%" + texto + "%' ";
                            }
                            else {
                                queryWhere += " or "+ response.campos[i].campoTabla + " like '%" + texto + "%' ";
                            }
                        }
                        queryWhere += " )";
                        if (busqueda.texto.Trim().Equals(""))
                        {
                            queryWhere = "";
                        }
                        response.data = await repo.ObtenerData(queryWhere,response.tabla, inicioPuntero, finPuntero, llavePrimaria);
                        List<object> total = await repo.ObtenerTotal(queryWhere, response.tabla, llavePrimaria);
                        if (total.Count == 1) {
                            var parametrosRequest = JObject.FromObject(total[0]).ToObject<Dictionary<string, object>>();
                            response.totalItems = parametrosRequest["total"].ToString();
                        }

                        _conn.Close();
                        return response;
                    }
                    catch (Exception ex)
                    {
                        _conn.Close();
                        throw new Exception(ex.Message);
                    }

                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<List<object>> GetDataRegistroID(string idLinea, string idRegistro)
        {
            try
            {
                using (IDbConnection _conn = new SqlConnection(conf.SQLServerPool))
                {
                    try
                    {
                        Repositorios.LineaNegocio repo = new Repositorios.LineaNegocio(_conn);
                        ConfiguracionLinea response = new ConfiguracionLinea();
                        response = await repo.ObtenerLineaNegocioPorID(idLinea);
                        response.campos = await repo.ObtenerCamposLineaCompleto(idLinea);
                        string llavePrimaria = response.campos.Find(x => x.esLlavePrimaria.Equals("S")).descripcionCampo;
                        List<object> data = await repo.ObtenerDataDeUnRegistro(response.tabla,llavePrimaria, idRegistro);

                        _conn.Close();
                        return data;
                    }
                    catch (Exception ex)
                    {
                        _conn.Close();
                        throw new Exception(ex.Message);
                    }

                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<List<LineaNegocioGeneral>> ObtenerLineasNegocio(string idCliente)
        {
            try
            {
                using (IDbConnection _conn = new SqlConnection(conf.SQLServerPool))
                {
                    try
                    {
                        List<LineaNegocioGeneral> tiposNegocios = new List<LineaNegocioGeneral>();
                        Repositorios.LineaNegocio repo = new Repositorios.LineaNegocio(_conn);
                        tiposNegocios = await repo.ObtenerLineasNegocio(idCliente);
                        foreach (LineaNegocioGeneral linea in tiposNegocios) {
                            List<CampoLineaGeneral> campos = await repo.ObtenerCamposLinea(linea.idLineaNegocio);
                            linea.campos = campos;
                        }
                        _conn.Close();
                        return tiposNegocios;
                    }
                    catch (Exception ex)
                    {
                        _conn.Close();
                        throw new Exception(ex.Message);
                    }

                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<ResponseLineaNegocio> NuevaLinea(LineaNegocioModel lineaNegocio)
        {
            try
            {


                using (IDbConnection _conn = new SqlConnection(conf.SQLServerPool)) //inicio la conexion
                {
                    _conn.Open();
                    using (var transaction = _conn.BeginTransaction()) //inicio la transaccion
                    {
                        try
                        {
                            // se crea el objeto de respuesta por parte del servidor
                            ResponseLineaNegocio response = new ResponseLineaNegocio("Error", "Desconocido");


                            //1. Traer de base de datos la Metada correspondiente de configuracion en base al archivo que se analizara
                            Repositorios.LineaNegocio repo = new Repositorios.LineaNegocio(_conn, transaction);
                            List<TipoDatoBD> tiposDato = await repo.ObtenerTiposDatosCompleto();

                            string nombreTabla = "LN_" + lineaNegocio.idClienteSistema + lineaNegocio.nombre.Replace(" ","");
                            nombreTabla = Regex.Replace(nombreTabla, @"[^0-9a-zA-Z]+", "").ToLower();
                            List<LineaNegocioNombreTabla> listaNegocios = await repo.ObtenerNegociosNombre(nombreTabla);
                            if (listaNegocios.Count > 0)
                            {
                                response.status = "Error";
                                response.mensaje = "Ya posee una linea de negocio con este nombre";
                            }
                            else {
                                string createTable = "CREATE TABLE " + nombreTabla + "( ";
                                string camposBasedeDatos = "id_" + Regex.Replace(lineaNegocio.nombre.Replace(" ", ""), @"[^0-9a-zA-Z]+", "").ToLower() + " INTEGER NOT NULL identity (1,1) ";
                                // inserta linea de negocio
                                int id = await repo.InsertaLineaNegocio(lineaNegocio, nombreTabla);
                                lineaNegocio.idLineaNegocio = Convert.ToString(id);
                                // inserta cada campo asociado
                                int numOrden = 0;
                                // se inserta campo primario
                                //string campoIDPrimario = "id_" + Regex.Replace(lineaNegocio.nombre.Replace(" ", ""), @"[^\w\s.!@$%^&*()\-\/]+", "").ToLower();
                                string campoIDPrimario = "id_" + Regex.Replace(lineaNegocio.nombre.Replace(" ", ""), @"[^0-9a-zA-Z]+", "").ToLower();
                                CampoLineaNegocio campoPrimario = new CampoLineaNegocio("1", campoIDPrimario, campoIDPrimario, true,true,"S");
                                int a = await repo.InsertaLineaNegocioCampo(Convert.ToString(id), campoPrimario, numOrden);
                                //lineaNegocio.campos.Add(campoPrimario);
                                // se insertan los demas campos
                                foreach (CampoLineaNegocio campo in lineaNegocio.campos)
                                {
                                    string campoExcel = campo.nombreCampo;
                                    campo.descripcionCampo = campoExcel;
                                    string nombreCampo = Regex.Replace(campo.nombreCampo, @"[^0-9a-zA-Z]+", "").ToLower().Replace(" ", "");
                                    campo.nombreCampo = nombreCampo;
                                    campo.esLlavePrimaria = "N";
                                    numOrden++;
                                    int asa = await repo.InsertaLineaNegocioCampo(Convert.ToString(id), campo, numOrden);
                                    TipoDatoBD tipo = tiposDato.Find(x => x.idTipoDato.Equals(campo.idTipodato));
                                    if (tipo != null)
                                    {
                                        camposBasedeDatos += (camposBasedeDatos.Equals("")) ? " " + nombreCampo + " " + tipo.TipoEnBD : ", " + nombreCampo + " " + tipo.TipoEnBD;
                                    }
                                }
                                createTable += camposBasedeDatos;
                                createTable += " ); ";
                                // crea la tabla donde van a ser insertados todos los registros
                                int crear = await repo.CreaTabla(createTable);
                                // iniciamos el proceso de carga de informacion
                                response = await CargarData(lineaNegocio, response, repo);

                                response.nombre = lineaNegocio.nombre;
                                response.descripcion = lineaNegocio.descripcion;
                                response.campos = lineaNegocio.campos;
                                foreach (CampoLineaNegocio campo in response.campos) { 
                                    campo.descripcionCampo = tiposDato.Find(x => x.idTipoDato.Equals(campo.idTipodato)).descripcion;
                                }
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
            return null;
        }
        public async Task<ResponseLineaNegocio> CargarData(LineaNegocioModel lineaNegocio, ResponseLineaNegocio response, Repositorios.LineaNegocio repo) {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using (var stream1 = lineaNegocio.Archivo.OpenReadStream()) {
                using (var reader = ExcelReaderFactory.CreateReader(stream1)) {
                    reader.Read(); //nos posicionamos en la primera fila
                    List<InformacionMDParaInsertar> listaDatos = await repo.ObtenerInformacionParaParser(lineaNegocio.idLineaNegocio);
                    List<string> encabezados = new List<string>();
                    for (int i = 0; i < reader.FieldCount; i++)
                        encabezados.Add((reader.GetValue(i) != null) ? reader.GetValue(i).ToString() : "");
                    Regex rgx = null;
                    //verifica que los encabezados cumplan con la plantilla correcta
                    List<int> listaIndices = new List<int>();
                    if (await VerificaEncabezados(listaDatos, response,encabezados, listaIndices)) {
                        int linea = 1;
                        int lineasSuccess = 0;
                        int lineasConError = 0;
                        int numeroErrores = 0;
                        int lineasEnBlanco = 0;
                        int lineasNoInsertadas = 0;
                        List<string> listaValoresInsert = new List<string>();
                        
                        // Este codigo es para poder llenar los registros que fueron fallando
                        List<RegistroExcel> listaRegistros = new List<RegistroExcel>();
                        RegistroExcel encabezadosExcel = new RegistroExcel();
                        foreach (InformacionMDParaInsertar campoDefinido in listaDatos) {
                            encabezadosExcel.valores.Add(campoDefinido.descripcionCampo);
                        }
                        listaRegistros.Add(encabezadosExcel);
                        // termina el trozo de codigo

                        bool llevaErrorlaLinea = false;
                        while (reader.Read()) //Each row of the file
                        {
                            RegistroExcel registro = new RegistroExcel();
                            linea++;
                            int cont = 0;
                            //verificacion de linea vacia
                            bool esVacia = true;
                            for (int i = 0; i < listaDatos.Count; i++)
                            {
                                if (reader.GetValue(listaIndices[i]) != null && !reader.GetValue(listaIndices[i]).ToString().Trim().Equals(""))
                                {
                                    esVacia = false; //levantamos bandera
                                    i = listaDatos.Count; //terminamos el ciclo
                                }
                            }
                            if (!esVacia)
                            {
                                foreach (InformacionMDParaInsertar campoDefinido in listaDatos)
                                {
                                    string valorCelda = (reader.GetValue(listaIndices[cont]) != null) ? reader.GetValue(listaIndices[cont]).ToString().Trim().ToLower() : "";
                                    string valCelda = (reader.GetValue(listaIndices[cont]) != null) ? reader.GetValue(listaIndices[cont]).ToString().Trim() : "";
                                    if (campoDefinido.obligatorio == "S" && valorCelda.Equals(""))
                                    {
                                        response.logs.Add(new Log("Error dato", "Campo obligatorio vacio", "Coloque un valor para la columna " + campoDefinido.nombreCampo, campoDefinido.nombreCampo, linea.ToString()));
                                        //esta linea lleva un error dato obligatorio vacio
                                        numeroErrores++;
                                        llevaErrorlaLinea = true;
                                    }
                                    else if (!valorCelda.Equals(""))
                                    {
                                        bool datoValido = await VerificaDatoERBD(campoDefinido.nombre, valorCelda, rgx, listaDatos);
                                        if (!datoValido)
                                        {
                                            response.logs.Add(new Log("Error dato", "Valor no cumple la definicion de la columna, se encontro : [" + valorCelda + "]", campoDefinido.ER, campoDefinido.descripcionCampo, linea.ToString()));
                                            numeroErrores++;
                                            llevaErrorlaLinea = true;
                                        }
                                        else
                                        {
                                            listaValoresInsert.Add(valCelda); //agrego valor para el insert
                                        }
                                    }
                                    else if (campoDefinido.obligatorio == "N" && valorCelda.Equals(""))
                                    { //si no es obligatorio y viene en blanco es valido 
                                        listaValoresInsert.Add(valCelda); //agrego valor para el insert
                                    }
                                    cont++;
                                }
                            }
                            else
                            {
                                response.logs.Add(new Log("Aviso", "Se omitio linea vacia", "", "", linea.ToString()));
                                lineasEnBlanco++;
                            }

                            //si hubo algun error ya se toma como que la linea dio un error y no se inserta
                            if (llevaErrorlaLinea)
                            {
                                llevaErrorlaLinea = false; //reinicio la variable
                                lineasConError++; //se suma una linea con error
                                //se crea la linea para el excel de errores
                                for (int a = 0; a < listaDatos.Count; a++) {
                                    string valCelda = (reader.GetValue(listaIndices[a]) != null) ? reader.GetValue(listaIndices[a]).ToString().Trim() : "";
                                    registro.valores.Add(valCelda);
                                }
                                listaRegistros.Add(registro);
                            }
                            else if (!esVacia)
                            {
                                lineasSuccess++; //como no existieron errores se suma una linea con exito
                                                 //se realiza la rutina para insertar el registro a la base de datos
                                bool respuestaInsert = await this.insertarRegistro(listaDatos, listaValoresInsert, repo);
                                if (!respuestaInsert)
                                {
                                    response.logs.Add(new Log("Error", "Ocurrio un error al insertar en base de datos", "Contacte al administrador", "", linea.ToString()));
                                    lineasNoInsertadas++;
                                }
                            }
                            listaValoresInsert.Clear();
                            listaValoresInsert = new List<string>();
                          
                        }
                        // aqui iria el response
                        response.status = "OK";
                        response.mensaje = "Exito";
                        response.lineasFallidas = lineasConError;
                        response.lineasSuccess = lineasSuccess;
                        double total = lineasSuccess + lineasConError;
                        response.porcentajeSucceed = Math.Round((lineasSuccess * 100) / total, 2);
                        response.porcentajeFailed = Math.Round((lineasConError * 100) / total, 2);
                        await CrearExcel(listaRegistros, response);
                    }
                    
                }
            }
            return response;
        }
        public async Task<ResponseLineaNegocio> CrearExcel(List<RegistroExcel> listaRegistros, ResponseLineaNegocio response) {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                //Set some properties of the Excel document
                excelPackage.Workbook.Properties.Author = "BL";
                excelPackage.Workbook.Properties.Title = "BL";
                excelPackage.Workbook.Properties.Subject = "BL";
                excelPackage.Workbook.Properties.Created = DateTime.Now;

                //Create the WorkSheet
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Data");
                int fila = 1;
                foreach (RegistroExcel registro in listaRegistros)
                {
                    int col = 1;
                    foreach (string columna in registro.valores) {
                        worksheet.Cells[fila, col].Value = columna;
                        col++;
                    }
                    fila++;
                }
                //Add some text to cell A1
                // worksheet.Cells["A1"].Value = "My first EPPlus spreadsheet!";
                //You could also use [line, column] notation:
                // worksheet.Cells[1, 2].Value = "This is cell B1!";

                //Save your file
                //FileInfo fi = new FileInfo(@"Plantilla.xlsx");
                //excelPackage.SaveAs(fi);
                byte[] array = { };
                array = excelPackage.GetAsByteArray();
                response.archivo = array;
            }
            return response;
        }
        async Task<bool> insertarRegistro(List<InformacionMDParaInsertar> listaDatos, List<string> valores, Repositorios.LineaNegocio repo)
        {
            if (listaDatos.Count == valores.Count)
            {
                string paramsSTR = "";
                string valoresSTR = "";
                //se arma la lista de parametros a insertar
                List<ParametroDatoBD> listaParametros = new List<ParametroDatoBD>();
                for (int i = 0; i < listaDatos.Count; i++)
                {
                    listaParametros.Add(new ParametroDatoBD(listaDatos[i].nombreCampo, valores[i]));
                    paramsSTR += (i == 0) ? (listaDatos[i].nombreCampo) : (", " + listaDatos[i].nombreCampo);
                    valoresSTR += (i == 0) ? ("@" + listaDatos[i].nombreCampo) : (", @" + listaDatos[i].nombreCampo);
                }
                //se arma el query que inserta
                string query = "INSERT INTO " + listaDatos[0].tablaInformacion + " ( " + paramsSTR + " ) VALUES ( " + valoresSTR + " )";
                int response = await repo.InsertaRegistro(query, listaParametros);
                if (response == 1)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }
        async Task<bool> VerificaDatoERBD(string tipoDato, string valor, Regex rgx, List<InformacionMDParaInsertar> listaMDCampos)
        {
            for (int i = 0; i < listaMDCampos.Count; i++)
            {
                if (tipoDato.Equals(listaMDCampos[i].nombre))
                {
                    rgx = new Regex(listaMDCampos[i].ER);
                    i = listaMDCampos.Count;
                    return rgx.IsMatch(valor);
                }
            }
            return false;
        }
        async Task<bool> VerificaEncabezados(List<InformacionMDParaInsertar> listaDatos, ResponseLineaNegocio response, List<string> encabezados, List<int> indices)
        {
            // esta validacion es por que el excel puede traer mas columnas que vienen en blanco por lo tanto puede ser mayor el numero o igual
            if (listaDatos.Count <= encabezados.Count)
            {
                string encabezadosTXT = "";
                bool encabezadosValidos = true;
                for (int i = 0; i < listaDatos.Count; i++)
                {
                    encabezadosTXT += " " + listaDatos[i].descripcionCampo.Trim().ToUpper();
                    for (int j = 0; j < encabezados.Count; j++) {
                        string campoI = listaDatos[i].descripcionCampo.Trim().ToUpper();
                        string encabezadoJ = encabezados[j].Trim().ToUpper();
                        if (campoI.Equals(encabezadoJ)) {
                            indices.Add(j);
                        }
                    }
                }
                if (!(listaDatos.Count == indices.Count)) {
                    response.mensaje = "No se encuentran los encabezados necesarios (" + encabezadosTXT +")";
                    return false;
                }
            }
            else
            {
                response.mensaje = "Hacen falta encabezados para este tipo de archivo. Asegurese que esten definidos como se le muestra.";
                foreach (InformacionMDParaInsertar mdCampo in listaDatos)
                {
                    response.logs.Add(new Log("Encabezado", mdCampo.nombreCampo, "", mdCampo.nombreCampo, ""));
                }
                return false;
            }

            return true;
        }
        public async Task<ResponseOperacionLineaNegocio> OperacionLineaNegocio(OperacionLineaNegocio operacion) {
            try
            {
                using (IDbConnection _conn = new SqlConnection(conf.SQLServerPool))
                {
                    _conn.Open();
                    using (var transaction = _conn.BeginTransaction())
                    {
                        try
                        {
                            ResponseOperacionLineaNegocio response = new ResponseOperacionLineaNegocio();
                            response.status = "Error";
                            Repositorios.LineaNegocio repo = new Repositorios.LineaNegocio(_conn, transaction);
                            if (operacion.operacion.Equals("eliminar")) {
                                await repo.EliminaLineaNegocio(operacion.idLineaNegocio);
                                response.status = "OK";
                                response.mensaje = "Eliminada con exito";
                                response.operacion = "Eliminación";
                            }
                            else if (operacion.operacion.Equals("formaMuestra")) {
                                await repo.ActualizaFormaMuestra(operacion.valor, operacion.idLineaNegocio);
                                response.status = "OK";
                                response.mensaje = "Actualizado con exito";
                                response.operacion = "Actualizacion";
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
        public async Task<ResponseOperacionLineaNegocio> OperacionSobreRegistro(OperacionData operacion)
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
                            ResponseOperacionLineaNegocio response = new ResponseOperacionLineaNegocio();
                            response.status = "Error";
                            Repositorios.LineaNegocio repo = new Repositorios.LineaNegocio(_conn, transaction);
                            ConfiguracionLinea confLinea = await repo.ObtenerLineaNegocioPorID(operacion.idLineaNegocio);
                            List<CampoLineaNegocioCompleto> campos = await repo.ObtenerCamposLineaCompleto(operacion.idLineaNegocio);

                            if (operacion.operacion.Equals("eliminar"))
                            {
                                string idLlavePrimaria = campos.Find(x => x.esLlavePrimaria.Equals("S")).descripcionCampo;
                                int idRespuesta = await repo.EliminaRegistro(confLinea.tabla, idLlavePrimaria, operacion.valor);
                                if (idRespuesta >= 0)
                                {
                                    response.status = "OK";
                                    response.mensaje = "Registro eliminado con exito";
                                    response.operacion = "eliminar";
                                    transaction.Commit();
                                }
                            }
                            else if (operacion.operacion.Equals("insertar"))
                            {
                                if (operacion.valores.Count == (campos.Count - 1))
                                {
                                    int respuesta = await repo.insertaRegistro(confLinea.tabla, operacion.valores, campos);
                                    if (respuesta == 1)
                                    {
                                        response.status = "OK";
                                        response.mensaje = "Registro insertado con exito";
                                        response.operacion = "insertar";
                                        transaction.Commit();
                                    }
                                }
                                else {
                                    response.mensaje = "No se enviaron los valores correctos";
                                }
                            }
                            else if (operacion.operacion.Equals("modificar")) {
                                if (operacion.valores.Count == (campos.Count - 1))
                                {
                                    string idLlavePrimaria = campos.Find(x => x.esLlavePrimaria.Equals("S")).descripcionCampo;
                                    int respuesta = await repo.modificaRegistro(confLinea.tabla, operacion.valores, campos,idLlavePrimaria,operacion.valorLlavePrimaria);
                                    if (respuesta == 1)
                                    {
                                        response.status = "OK";
                                        response.mensaje = "Registro modificado con exito";
                                        response.operacion = "modificar";
                                        transaction.Commit();
                                    }
                                }
                                else
                                {
                                    response.mensaje = "No se enviaron los valores correctos";
                                }
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
        public async Task<ResponseOperacionLineaNegocio> SetImage(RequestConImagen req)
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
                            ResponseOperacionLineaNegocio response = new ResponseOperacionLineaNegocio();
                            response.status = "Error";
                            string connectionString = this.conf.AzureProdConexion;
                            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                            string containerName = this.conf.ContainerProductosIMG;

                            // Create the container and return a container client object
                            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                            string extension = System.IO.Path.GetExtension(req.Archivo.FileName);
                            string fileName = Guid.NewGuid().ToString() + extension;

                            // Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);

                            //instancia de repositorio
                            Repositorios.LineaNegocio repo = new Repositorios.LineaNegocio(_conn, transaction);
                            BlobClient blobClient = containerClient.GetBlobClient(fileName);
                            using (var stream = req.Archivo.OpenReadStream())
                            {
                                await blobClient.UploadAsync(stream, true);
                            }
                            // await repo.EliminaOtrasIMG(req.idLinea, req.valorLlave);
                            await repo.InsertaRegistroIMG(req.idLinea, req.valorLlave, fileName);
                            response.status = "OK";
                            response.mensaje = "Imagen Guardada con exito";
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
                ResponseOperacionLineaNegocio response = new ResponseOperacionLineaNegocio();
                response.status = "Error";
                response.mensaje = "Error en la operacion";
                throw new Exception(e.Message);
            }


            return null;
        }
        public async Task<ResponseOperacionLineaNegocio> DelImage(RequestOpImagen req)
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
                            ResponseOperacionLineaNegocio response = new ResponseOperacionLineaNegocio();
                            response.status = "Error";
                            Repositorios.LineaNegocio repo = new Repositorios.LineaNegocio(_conn, transaction);
                            await repo.EliminaOtrasIMG(req.idLinea, req.valorLlave, req.guid);
                            response.status = "OK";
                            response.mensaje = "Imagen Eliminada con exito";
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
                ResponseOperacionLineaNegocio response = new ResponseOperacionLineaNegocio();
                response.status = "Error";
                response.mensaje = "Error en la operacion";
                throw new Exception(e.Message);
            }


            return null;
        }
        public async Task<ResponseImagenesProducto> GetImagesID(string idLinea, string valorLLave)
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
                            ResponseImagenesProducto response = new ResponseImagenesProducto();
                            response.status = "Error";
                            Repositorios.LineaNegocio repo = new Repositorios.LineaNegocio(_conn, transaction);
                            List<LineaNegocioTablaInformacion> lista = await repo.ObtenerImagenesInformacion(idLinea, valorLLave);
                            string connectionString = this.conf.AzureProdConexion;
                            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                            string containerName = this.conf.ContainerProductosIMG;

                            // Create the container and return a container client object
                            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                            foreach (LineaNegocioTablaInformacion img in lista) {
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
                            response.status = "OK";
                            response.mensaje = "Exito";
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
                ResponseOperacionLineaNegocio response = new ResponseOperacionLineaNegocio();
                response.status = "Error";
                response.mensaje = "Error en la operacion";
                throw new Exception(e.Message);
            }


            return null;
        }
        public async Task<List<ResponseDatosPorColumna>> GetDataColumnaG1(string idLinea, string columna, int numDatos)
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
                            List<ResponseDatosPorColumna> response = new List<ResponseDatosPorColumna>();
                            Repositorios.LineaNegocio repo = new Repositorios.LineaNegocio(_conn, transaction);
                            ConfiguracionLinea confLinea = await repo.ObtenerLineaNegocioPorID(idLinea);
                            response = await repo.ObtenerDataPorColumna(confLinea.tabla, columna, numDatos);
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
                ResponseOperacionLineaNegocio response = new ResponseOperacionLineaNegocio();
                response.status = "Error";
                response.mensaje = "Error en la operacion";
                throw new Exception(e.Message);
            }


            return null;
        }
        public async Task<List<object>> GetDataGrafica1(RequestDataGrafica1 request)
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
                            List<object> response = new List<object>();
                            Repositorios.LineaNegocio repo = new Repositorios.LineaNegocio(_conn, transaction);
                            ConfiguracionLinea confLinea = await repo.ObtenerLineaNegocioPorID(request.idLinea);
                            string strPivot = "";
                            string strCampos = "";
                            for (int i = 0; i < request.valoresPivot.Count; i++) {
                                if (i == 0) {
                                    strPivot += "'" + request.valoresPivot[i] + "'";
                                }
                                else {
                                    strPivot += ", '" + request.valoresPivot[i] + "'";
                                }
                            }
                            for (int i = 0; i < request.valoresCamposNumericos.Count; i++)
                            {
                                strCampos += ", SUM(CAST(" + request.valoresCamposNumericos[i] + " as real))" + request.valoresCamposNumericos[i];
                            }
                            response = await repo.ObtenerDataGrafica1(confLinea.tabla, strCampos, strPivot, request.campoPivot);
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
                ResponseOperacionLineaNegocio response = new ResponseOperacionLineaNegocio();
                response.status = "Error";
                response.mensaje = "Error en la operacion";
                throw new Exception(e.Message);
            }


            return null;
        }

        public async Task<ResponseOperacionLineaNegocio> SetImgCliente(RequestConImagen req, string idCliente, string tipo)
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
                            ResponseOperacionLineaNegocio response = new ResponseOperacionLineaNegocio();
                            response.status = "Error";
                            string connectionString = this.conf.AzureProdConexion;
                            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                            string containerName = this.conf.ContainerProductosIMG;

                            // Create the container and return a container client object
                            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                            string extension = System.IO.Path.GetExtension(req.Archivo.FileName);
                            string fileName = Guid.NewGuid().ToString() + extension;

                            // Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);

                            //instancia de repositorio
                            Repositorios.LineaNegocio repo = new Repositorios.LineaNegocio(_conn, transaction);
                            BlobClient blobClient = containerClient.GetBlobClient(fileName);
                            using (var stream = req.Archivo.OpenReadStream())
                            {
                                await blobClient.UploadAsync(stream, true);
                            }
                            // await repo.EliminaOtrasIMG(req.idLinea, req.valorLlave);
                            string campo = "";
                            if (tipo.Equals("slide1"))
                                campo = "pathSlide1";
                            else if (tipo.Equals("slide2"))
                                campo = "pathSlide2";
                            else if (tipo.Equals("slide3"))
                                campo = "pathSlide3";
                            else if (tipo.Equals("icono"))
                                campo = "pathLogoEsquina";

                            await repo.ActualizarImgConfiguracionCliente(idCliente, campo, fileName);
                            response.status = "OK";
                            response.mensaje = "Imagen Guardada con exito";
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
                ResponseOperacionLineaNegocio response = new ResponseOperacionLineaNegocio();
                response.status = "Error";
                response.mensaje = "Error en la operacion";
                throw new Exception(e.Message);
            }


            return null;
        }

        public async Task<ResponseConfiguracionesCLiente> GetImagesConfCliente(string idCliente)
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
                            ResponseConfiguracionesCLiente response = new ResponseConfiguracionesCLiente();
                            response.status = "Error";
                            response.imagenes = new List<ObjetoImagenDatos>();
                            Repositorios.LineaNegocio repo = new Repositorios.LineaNegocio(_conn, transaction);
                            List<ConfClientCompleta> lista = await repo.ObtenerInfoCliente(idCliente);

                            if (lista.Count == 1) {
                                string connectionString = this.conf.AzureProdConexion;
                                BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                                string containerName = this.conf.ContainerProductosIMG;
                                response.idClienteSistema = lista[0].idClienteSistema;
                                response.numMaxLineasNegocio = lista[0].numMaxLineasNegocio;
                                response.numMaxUsuarios = lista[0].numMaxUsuarios;
                                response.tipoPeriodo = lista[0].tipoPeriodo;

                                // Create the container and return a container client object
                                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                                // obtener las tres imagenes
                                if (!lista[0].pathLogoEsquina.Trim().Equals("")) {
                                    //Imagen de Icono
                                    ObjetoImagenDatos objetoIcono = new ObjetoImagenDatos();
                                    objetoIcono.guid = lista[0].pathLogoEsquina;
                                    string fileNameIcono = lista[0].pathLogoEsquina;
                                    byte[] arrayIcono = { };
                                    // Get a reference to a blob
                                    BlobClient blobClientIcono = containerClient.GetBlobClient(fileNameIcono);
                                    using (var ms = new MemoryStream())
                                    {
                                        blobClientIcono.DownloadTo(ms);
                                        objetoIcono.bytes = ms.ToArray();

                                        string[] valores = fileNameIcono.Split('.');
                                        if (valores.Length == 2)
                                        {
                                            objetoIcono.tipo = "image/" + valores[1];
                                        }
                                    }
                                    response.imagenes.Add(objetoIcono);
                                }
                                if (!lista[0].pathSlide1.Trim().Equals(""))
                                {
                                    //Imagen 1
                                    ObjetoImagenDatos objeto = new ObjetoImagenDatos();
                                    objeto.guid = lista[0].pathSlide1;
                                    string fileName = lista[0].pathSlide1;
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
                                if (!lista[0].pathSlide2.Trim().Equals(""))
                                {
                                    //Imagen 2
                                    ObjetoImagenDatos objeto2 = new ObjetoImagenDatos();
                                    objeto2.guid = lista[0].pathSlide2;
                                    string fileName2 = lista[0].pathSlide2;
                                    byte[] array2 = { };
                                    // Get a reference to a blob
                                    BlobClient blobClient2 = containerClient.GetBlobClient(fileName2);
                                    using (var ms = new MemoryStream())
                                    {
                                        blobClient2.DownloadTo(ms);
                                        objeto2.bytes = ms.ToArray();

                                        string[] valores = fileName2.Split('.');
                                        if (valores.Length == 2)
                                        {
                                            objeto2.tipo = "image/" + valores[1];
                                        }
                                    }
                                    response.imagenes.Add(objeto2);
                                }
                                if (!lista[0].pathSlide3.Trim().Equals(""))
                                {
                                    //Imagen 3
                                    ObjetoImagenDatos objeto3 = new ObjetoImagenDatos();
                                    objeto3.guid = lista[0].pathSlide3;
                                    string fileName3 = lista[0].pathSlide3;
                                    byte[] array3 = { };
                                    // Get a reference to a blob
                                    BlobClient blobClient3 = containerClient.GetBlobClient(fileName3);
                                    using (var ms = new MemoryStream())
                                    {
                                        blobClient3.DownloadTo(ms);
                                        objeto3.bytes = ms.ToArray();

                                        string[] valores = fileName3.Split('.');
                                        if (valores.Length == 2)
                                        {
                                            objeto3.tipo = "image/" + valores[1];
                                        }
                                    }
                                    response.imagenes.Add(objeto3);
                                }
                                
                                response.status = "OK";
                                response.mensaje = "Exito";
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
                ResponseOperacionLineaNegocio response = new ResponseOperacionLineaNegocio();
                response.status = "Error";
                response.mensaje = "Error en la operacion";
                throw new Exception(e.Message);
            }


            return null;
        }
    }
}
