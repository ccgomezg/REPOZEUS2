using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1.Services
{
    public interface IMigracionLogService
    {
        List<int> ObtenerAniosCompletados(string nit, TipoMigracion tipo);
        void RegistrarAnioCompletado(string nit, int anio, TipoMigracion tipo, int cantidadArchivos);
        void ActualizarSubidoS3(string nit, int anio, TipoMigracion tipo, int cantidadArchivos, bool subidoS3, string respuesta);
    }

    public class MigracionLogService : IMigracionLogService
    {
        private readonly string _rutaLog;
        private readonly object _lockObject = new object();

        public MigracionLogService(string rutaLog = null)
        {
            _rutaLog = rutaLog ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "migracion_log.txt");
            InicializarLog();
        }

        private void InicializarLog()
        {
            lock (_lockObject)
            {
                if (!File.Exists(_rutaLog))
                {
                    File.WriteAllLines(_rutaLog, new[]
                    {
                        "# Log de Migraciones Completadas",
                        "# Formato: NIT|Año|Tipo|CantidadArchivos|Fecha|SubidoS3",
                        ""
                    });
                }
            }
        }

        public List<int> ObtenerAniosCompletados(string nit, TipoMigracion tipo)
        {
            var aniosCompletados = new List<int>();
            if (!File.Exists(_rutaLog)) return aniosCompletados;

            try
            {
                lock (_lockObject)
                {
                    var tipoTexto = ObtenerTextoTipo(tipo);
                    var lineas = File.ReadAllLines(_rutaLog);

                    foreach (var linea in lineas.Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("#")))
                    {
                        var partes = linea.Split('|');
                        if (partes.Length >= 3 && partes[0] == nit && partes[2] == tipoTexto)
                        {
                            if (int.TryParse(partes[1], out int anio) && !aniosCompletados.Contains(anio))
                                aniosCompletados.Add(anio);
                        }
                    }
                }
            }
            catch { }

            return aniosCompletados;
        }

        public void RegistrarAnioCompletado(string nit, int anio, TipoMigracion tipo, int cantidadArchivos)
        {
            try
            {
                lock (_lockObject)
                {
                    var linea = $"{nit}|{anio}|{ObtenerTextoTipo(tipo)}|{cantidadArchivos}|" +
                               $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}|false";
                    File.AppendAllText(_rutaLog, linea + Environment.NewLine);
                }
            }
            catch { }
        }

        public void ActualizarSubidoS3(string nit, int anio, TipoMigracion tipo, int cantidadArchivos, bool subidoS3, string respuesta)
        {
            try
            {
                lock (_lockObject)
                {
                    var tipoTexto = ObtenerTextoTipo(tipo);
                    var lineas = File.ReadAllLines(_rutaLog).ToList();

                    for (int i = lineas.Count - 1; i >= 0; i--)
                    {
                        if (lineas[i].StartsWith($"{nit}|{anio}|{tipoTexto}|{cantidadArchivos}|"))
                        {
                            var partes = lineas[i].Split('|');
                            if (partes.Length >= 5)
                            {
                                lineas[i] = $"{partes[0]}|{partes[1]}|{partes[2]}|{partes[3]}|{partes[4]}|{subidoS3}|{respuesta}";
                                break;
                            }
                        }
                    }

                    File.WriteAllLines(_rutaLog, lineas);
                }
            }
            catch { }
        }

        private string ObtenerTextoTipo(TipoMigracion tipo)
        {
            switch (tipo)
            {
                case TipoMigracion.Front: return "FRONT";
                case TipoMigracion.Back: return "BACK";
                case TipoMigracion.Ambos: return "AMBOS";
                default: return "DESCONOCIDO";
            }
        }
    }
}