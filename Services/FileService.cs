using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1.Services
{
    public interface IFileService
    {
        string GenerarArchivoTransacciones(string rutaArchivo, int anio, List<TransaccionData> transaccionesFront, List<TransaccionData> transaccionesBack);
        string CrearDirectorioMigracion(string rutaBase, string nit);
        void CrearArchivoLog(string directorio, string nombreArchivo, string contenido, bool esError = false);
    }

    public class FileService : IFileService
    {
        public string GenerarArchivoTransacciones(string rutaArchivo, int anio,
            List<TransaccionData> transaccionesFront, List<TransaccionData> transaccionesBack)
        {
            try
            {
                var todas = new List<TransaccionData>();

                if (transaccionesFront != null)
                    todas.AddRange(transaccionesFront);

                if (transaccionesBack != null)
                    todas.AddRange(transaccionesBack);

                var ordenadas = todas.OrderBy(t => t.FechaHora);

                using (var writer = new StreamWriter(rutaArchivo, false, Encoding.UTF8))
                {
                    writer.WriteLine("modulo|ldf|parametros|fecha");

                    foreach (var transaccion in ordenadas)
                    {
                        writer.WriteLine(
                            $"{LimpiarTexto(transaccion.Modulo)}|" +
                            $"{LimpiarTexto(transaccion.Mensaje)}|" +
                            $"{LimpiarTexto(transaccion.Parametros)}|" +
                            $"{transaccion.FechaHora:yyyy-MM-dd HH:mm:ss}"
                        );
                    }
                }

                return rutaArchivo;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generando archivo {rutaArchivo}: {ex.Message}", ex);
            }
        }

        public string CrearDirectorioMigracion(string rutaBase, string nit)
        {
            var nombreDirectorio = $"Migracion_{nit}_{DateTime.Now:yyyyMMdd_HHmmss}";
            var rutaCompleta = Path.Combine(rutaBase, nombreDirectorio);

            Directory.CreateDirectory(rutaCompleta);
            return rutaCompleta;
        }

        public void CrearArchivoLog(string directorio, string nombreArchivo, string contenido, bool esError = false)
        {
            var prefijo = esError ? "ERROR" : "LOG";
            var nombreCompleto = $"{prefijo}_{nombreArchivo}.txt";
            var rutaCompleta = Path.Combine(directorio, nombreCompleto);

            var timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            var contenidoCompleto = $"{timestamp}\n{contenido}";

            File.WriteAllText(rutaCompleta, contenidoCompleto, Encoding.UTF8);
        }

        private static string LimpiarTexto(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return string.Empty;

            return texto.Replace("\r", "")
                       .Replace("\n", "")
                       .Replace("\t", "")
                       .Trim();
        }
    }
}