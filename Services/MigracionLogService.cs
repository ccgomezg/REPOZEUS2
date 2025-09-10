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
        List<(string año, string ruta)> ObtenerAniosErrorNotificar(string nit, TipoMigracion tipo);

        void RegistrarAnioCompletado(string nit, int anio, TipoMigracion tipo, int cantidadArchivos, string rutaZip);
        void ActualizarSubidoS3(string nit, int anio, TipoMigracion tipo, bool subidoS3, string respuesta, string ruta);
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
                        "# Formato: NIT|Año|Tipo|CantidadArchivos|rutaLocal|SubidoS3|Fecha|respuestashttp|",
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

        public List<(string año, string ruta)> ObtenerAniosErrorNotificar(string nit, TipoMigracion tipo)
        {
            var anioSinNotificar = new List<(string año, string ruta)>();
            if (!File.Exists(_rutaLog)) return anioSinNotificar;

            try
            {
                lock (_lockObject)
                {
                    var tipoTexto = ObtenerTextoTipo(tipo);
                    var lineas = File.ReadAllLines(_rutaLog);

                    foreach (var linea in lineas.Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("#")))
                    {
                        var partes = linea.Split('|');
                        int anio = int.TryParse(partes[1]?.Trim(), out var x) ? x : 0;

                        bool enviado = bool.TryParse(partes[5]?.Trim().ToLower(), out bool result) ? result : false;
                        if (partes.Length >= 3 && partes[0] == nit && partes[2] == tipoTexto && !enviado)
                        {
                                string ruta = partes[4];
                                anioSinNotificar.Add((anio.ToString(), ruta));
                        }
                    }
                }
            }
            catch { }

            return anioSinNotificar;
        }


        public void RegistrarAnioCompletado(string nit, int anio, TipoMigracion tipo, int cantidadArchivos,string rutaZip)
        {
            try
            {
                lock (_lockObject)
                {
                    var linea = $"{nit}|{anio}|{ObtenerTextoTipo(tipo)}|{cantidadArchivos}|" +
                               $"{rutaZip}|{false}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                    File.AppendAllText(_rutaLog, linea + Environment.NewLine);
                }
            }
            catch { }
        }

        public void ActualizarSubidoS3(string nit, int anio, TipoMigracion tipo, bool subidoS3, string respuesta, string ruta)
        {
            try
            {
                lock (_lockObject)
                {
                    var tipoTexto = ObtenerTextoTipo(tipo);
                    var lineas = File.ReadAllLines(_rutaLog).ToList();
                    
                    for (int i = lineas.Count - 1; i >= 0; i--)
                    {
                        var partes = lineas[i].Split('|');
                        int cantidad_archivos =int.Parse(partes[3]);

                        if (lineas[i].StartsWith($"{nit}|{anio}|{tipoTexto}|{cantidad_archivos}|{ruta}|"))
                        {
                            if (partes.Length >= 5)
                            {
                                lineas[i] = $"{partes[0]}|{partes[1]}|{partes[2]}|{partes[3]}|{partes[4]}|{subidoS3}|{partes[6]}|{respuesta}";
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