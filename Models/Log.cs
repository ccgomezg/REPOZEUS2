using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WindowsFormsApp1.Models
{
    public class ProgresoMigracion
    {
        public string NIT { get; set; }
        public int Anio { get; set; }
        public string TipoMigracion { get; set; } // "FRONT", "BACK", "AMBOS"
        public int MesActual { get; set; } // Último mes procesado (1-12)
        public bool CompletadoFront { get; set; }
        public bool CompletadoBack { get; set; }
        public int CantidadFacturas { get; set; }
        public int CantidadZips { get; set; }
        public DateTime FechaUltimaActualizacion { get; set; }
        public string Estado { get; set; } // "EN_PROCESO", "COMPLETADO", "ERROR"
        public string DirectorioArchivos { get; set; }

        public bool EstaCompleto =>
            (TipoMigracion == "FRONT" && CompletadoFront) ||
            (TipoMigracion == "BACK" && CompletadoBack) ||
            (TipoMigracion == "AMBOS" && CompletadoFront && CompletadoBack);

        public string GenerarLinea()
        {
            return $"{NIT}|{Anio}|{TipoMigracion}|{MesActual}|{(CompletadoFront ? "1" : "0")}|{(CompletadoBack ? "1" : "0")}|{CantidadFacturas}|{CantidadZips}|{FechaUltimaActualizacion:yyyy-MM-dd HH:mm:ss}|{Estado}|{DirectorioArchivos}";
        }

        public static ProgresoMigracion ParsearLinea(string linea)
        {
            if (string.IsNullOrWhiteSpace(linea) || linea.StartsWith("#"))
                return null;

            var partes = linea.Split('|');
            if (partes.Length < 11)
                return null;

            try
            {
                return new ProgresoMigracion
                {
                    NIT = partes[0],
                    Anio = int.Parse(partes[1]),
                    TipoMigracion = partes[2],
                    MesActual = int.Parse(partes[3]),
                    CompletadoFront = partes[4] == "1",
                    CompletadoBack = partes[5] == "1",
                    CantidadFacturas = int.Parse(partes[6]),
                    CantidadZips = int.Parse(partes[7]),
                    FechaUltimaActualizacion = DateTime.Parse(partes[8]),
                    Estado = partes[9],
                    DirectorioArchivos = partes.Length > 10 ? partes[10] : ""
                };
            }
            catch
            {
                return null;
            }
        }
    }

    public interface IProgresoService
    {
        List<ProgresoMigracion> CargarProgresosPendientes(string nit = null);
        void GuardarProgreso(ProgresoMigracion progreso);
        void MarcarCompletado(string nit, int anio, string tipo);
        void EliminarProgreso(string nit, int anio);
        string ObtenerRutaLog();
    }

    public class ProgresoService : IProgresoService
    {
        private readonly string _rutaLog;

        public ProgresoService(string rutaBase = null)
        {
            var directorio = rutaBase ?? AppDomain.CurrentDomain.BaseDirectory;
            _rutaLog = Path.Combine(directorio, "migracion_progreso.log");
            InicializarArchivoSiNoExiste();
        }

        public string ObtenerRutaLog() => _rutaLog;

        private void InicializarArchivoSiNoExiste()
        {
            if (!File.Exists(_rutaLog))
            {
                var encabezado = new[]
                {
                    "# Log de Progreso de Migraciones",
                    "# Formato: NIT|Año|Tipo|MesActual|CompletadoFront|CompletadoBack|CantidadFacturas|CantidadZips|FechaActualizacion|Estado|DirectorioArchivos",
                    ""
                };
                File.WriteAllLines(_rutaLog, encabezado);
            }
        }

        public List<ProgresoMigracion> CargarProgresosPendientes(string nit = null)
        {
            var progresos = new List<ProgresoMigracion>();

            if (!File.Exists(_rutaLog))
                return progresos;

            try
            {
                var lineas = File.ReadAllLines(_rutaLog);
                foreach (var linea in lineas)
                {
                    var progreso = ProgresoMigracion.ParsearLinea(linea);
                    if (progreso != null &&
                        progreso.Estado == "EN_PROCESO" &&
                        (string.IsNullOrEmpty(nit) || progreso.NIT == nit))
                    {
                        progresos.Add(progreso);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error cargando progresos: {ex.Message}");
            }

            return progresos;
        }

        public void GuardarProgreso(ProgresoMigracion progreso)
        {
            try
            {
                var lineasExistentes = File.Exists(_rutaLog) ? File.ReadAllLines(_rutaLog).ToList() : new List<string>();

                // Buscar si ya existe un registro para este NIT/Año
                var indiceExistente = lineasExistentes.FindIndex(l =>
                {
                    var p = ProgresoMigracion.ParsearLinea(l);
                    return p != null && p.NIT == progreso.NIT && p.Anio == progreso.Anio;
                });

                progreso.FechaUltimaActualizacion = DateTime.Now;

                if (indiceExistente >= 0)
                {
                    // Actualizar registro existente
                    lineasExistentes[indiceExistente] = progreso.GenerarLinea();
                }
                else
                {
                    // Agregar nuevo registro
                    lineasExistentes.Add(progreso.GenerarLinea());
                }

                File.WriteAllLines(_rutaLog, lineasExistentes);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error guardando progreso: {ex.Message}");
            }
        }

        public void MarcarCompletado(string nit, int anio, string tipo)
        {
            var progresos = CargarTodosLosProgresos();
            ProgresoMigracion progreso = null;

            // Buscar el progreso usando un bucle for en lugar de FirstOrDefault
            foreach (var p in progresos)
            {
                if (p.NIT == nit && p.Anio == anio)
                {
                    progreso = p;
                    break;
                }
            }

            if (progreso != null)
            {
                progreso.Estado = "COMPLETADO";
                progreso.MesActual = 12; // Completó todos los meses
                GuardarProgreso(progreso);
            }
        }

        public void EliminarProgreso(string nit, int anio)
        {
            try
            {
                var lineasExistentes = File.Exists(_rutaLog) ? File.ReadAllLines(_rutaLog).ToList() : new List<string>();

                lineasExistentes.RemoveAll(l =>
                {
                    var p = ProgresoMigracion.ParsearLinea(l);
                    return p != null && p.NIT == nit && p.Anio == anio;
                });

                File.WriteAllLines(_rutaLog, lineasExistentes);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error eliminando progreso: {ex.Message}");
            }
        }

        private List<ProgresoMigracion> CargarTodosLosProgresos()
        {
            var progresos = new List<ProgresoMigracion>();

            if (!File.Exists(_rutaLog))
                return progresos;

            var lineas = File.ReadAllLines(_rutaLog);
            foreach (var linea in lineas)
            {
                var progreso = ProgresoMigracion.ParsearLinea(linea);
                if (progreso != null)
                {
                    progresos.Add(progreso);
                }
            }

            return progresos;
        }
    }
}