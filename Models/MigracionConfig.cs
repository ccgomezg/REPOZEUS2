using System;

namespace WindowsFormsApp1.Models
{
    public class MigracionConfig
    {
        public string NIT { get; set; }
        public string RutaDescarga { get; set; }
        public bool TodosLosAnios { get; set; }
        public int[] AniosSeleccionados { get; set; }
        public int TotalRegistrosEstimados { get; set; }

        public DatabaseConfig DatabaseFront { get; set; }
        public DatabaseConfig DatabaseBack { get; set; }

        public MigracionConfig()
        {
            DatabaseFront = new DatabaseConfig();
            DatabaseBack = new DatabaseConfig();
            AniosSeleccionados = new int[0];
        }
    }

    public class DatabaseConfig
    {
        public string Usuario { get; set; }
        public string Password { get; set; }
        public string Ip { get; set; }
        public string NombreBaseDatos { get; set; }
        
        public bool SpEjecutado {  get; set; }

        public string SpName { get; set; }

        public bool EstaCompleto()
        {
            return !string.IsNullOrWhiteSpace(Usuario) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(Ip) &&
                   !string.IsNullOrWhiteSpace(NombreBaseDatos);
        }

        public string GetConnectionString()
        {
            var servidor = Ip?.Contains(":") == true && !Ip.Contains(",")
                ? Ip.Replace(":", ",")
                : Ip;

            return $"Server={servidor};Database={NombreBaseDatos};User Id={Usuario};Password={Password};Connection Timeout=30;Encrypt=false;TrustServerCertificate=true";
        }
    }

    public class TransaccionData
    {
        public string Modulo { get; set; }
        public string Mensaje { get; set; }
        public string Parametros { get; set; }
        public DateTime FechaHora { get; set; }
        public string Origen { get; set; }
    }

    public enum TipoMigracion
    {
        Front,
        Back,
        Ambos,
        NULL
    }
}