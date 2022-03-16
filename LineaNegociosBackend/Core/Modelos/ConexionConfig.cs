using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Modelos
{
    public class ConexionConfig
    {
        public string SQLServerPool { get; set; }
        public string AzureBlobStorageKey1 { get; set; }
        public string AzureProdConexion { get; set; }
        public string ContainerProductosIMG { get; set; }
    }
}
