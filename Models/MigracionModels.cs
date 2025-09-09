using System;
using System.Collections.Generic;

namespace WindowsFormsApp1.Models
{
    public class MigracionResult
    {
        public bool Exitoso { get; set; }
        public string NIT { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string DirectorioArchivos { get; set; }
        public string ArchivosGenerados { get; set; }
        public List<int> AniosProcesados { get; set; }
        public List<ErrorInfo> Errores { get; set; }
        public int ArchivosEnviadosAPI { get; set; }
        public string MensajeRecuperacion { get; set; }

        public MigracionResult()
        {
            ArchivosGenerados = string.Empty;
            AniosProcesados = new List<int>();
            Errores = new List<ErrorInfo>();
        }

        public TimeSpan DuracionTotal => FechaFin - FechaInicio;
    }

    public class ErrorInfo
    {
        public int? Anio { get; set; }
        public string Mensaje { get; set; }
        public DateTime Fecha { get; set; }
    }

    public class ArchivoZipInfo
    {
        public string RutaArchivo { get; set; }
        public int NumeroParte { get; set; }
        public long TamañoBytes { get; set; }
    }

    public class ArchivoGenerado
    {
        public string NombreArchivo { get; set; }
        public string RutaArchivo { get; set; }
        public int NumeroParte { get; set; }
        public bool SubidoExitosamente { get; set; }
        public string Error { get; set; }
    }

    public class ApiResponse
    {
        public int StatusCode { get; set; }
        public string Content { get; set; }
    }

}