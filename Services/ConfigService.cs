using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using WindowsFormsApp1.Models;



namespace WindowsFormsApp1.Services
{
    public interface IConfigService
    {
        bool CargarConfiguracion(string rutaArchivo, out MigracionConfig config);
        void GuardarConfiguracion(string rutaArchivo, MigracionConfig config);
        void ActualizarValorConfig(string rutaArchivo, string clave, string nuevoValor);
        bool ActualizarConfiguracionExistente(string rutaArchivo, ref MigracionConfig configExistente);
    }

    public class ConfigService : IConfigService
    {
        private static readonly HashSet<string> ClavesPermitidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "NIT", "RutaDescarga",
            "UsuarioFront", "PasswordFront", "IpFront", "BaseDatosFront",
            "UsuarioBack", "PasswordBack", "IpBack", "BaseDatosBack","SpBackEjecutado","SpFrontEjecutado", "SpNameFront", "SpNameBack", "DividirZip","Tamañozip"
        };

        public bool CargarConfiguracion(string rutaArchivo, out MigracionConfig config)
        {
            config = new MigracionConfig();

            try
            {
                if (!File.Exists(rutaArchivo))
                    return false;

                var propiedades = LeerPropiedadesArchivo(rutaArchivo);
                if (propiedades.Count == 0)
                    return false;

                AsignarPropiedadesAConfig(propiedades, config);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void GuardarConfiguracion(string rutaArchivo, MigracionConfig config)
        {
            
            //1) Lee el archivo si existe
            string[] existentes = Array.Empty<string>();
            if (File.Exists(rutaArchivo))
                existentes = File.ReadAllLines(rutaArchivo, Encoding.UTF8);

            // 2) Busca si ya existen SELECT_BACK / SELECT_FRONT (comentados o no)
            string lineaSelectBackExistente = BuscarLineaConfigExistente(existentes, "SELECT_BACK");
            string lineaSelectFrontExistente = BuscarLineaConfigExistente(existentes, "SELECT_FRONT");

            //2.1 Buscar SP CONFIG
            // 2) Busca si ya existen PARAMETROS DE CONFIG
            string SpEjecutadoBack= BuscarLineaConfigExistente(existentes, "SpBackEjecutado");
            string SpNameBack = BuscarLineaConfigExistente(existentes, "SpNameBack");

            string SpEjecutadoFront = BuscarLineaConfigExistente(existentes, "SpFrontEjecutado");
            string SpNameFront = BuscarLineaConfigExistente(existentes, "SpNameFront");

            // 3) Define los valores por defecto (comentados) SOLO si no existen
            const string SELECT_DEF = "{\"modulo\":\"\", \"ldf\":\"\",\"parametros_Adicionales\":\"\", \"fecha\":\"\", \"nombre_de_tabla_Sql\":\"\"}";
            string selectBackFinal = lineaSelectBackExistente ?? $"# SELECT_BACK={SELECT_DEF}";
            string selectFrontFinal = lineaSelectFrontExistente ?? $"# SELECT_FRONT={SELECT_DEF}";

            string SpNameFrontFinal = SpNameFront ?? "SpNameFront =SpFacturaElectronica_MigracionFacturaCol_To_PTSIESA_Contabilidad";
            string SpEjecutadoFrontFinal = SpEjecutadoFront ?? "SpFrontEjecutado=false" ;

            string SpNameBackFinal = SpNameBack ?? "SpNameBack =SpFacturaElectronica_MigracionFacturaCol_To_PTSIESA_Contabilidad";
            string SpEjecutadoBackFinal = SpEjecutadoBack ?? "SpBackEjecutado=false";

            //4)zips tamaño y division
            string dividirZip = BuscarLineaConfigExistente(existentes, "DividirZip")  ?? "DividirZip=1";
            string tamañozip =  BuscarLineaConfigExistente(existentes, "Tamañozip") ?? "Tamañozip = 5";
 



            // 4) Escribe TODO el archivo NUEVO, pero con las líneas SELECT preservadas
            try
            {
                using (var writer = new StreamWriter(rutaArchivo, false, Encoding.UTF8))
                {
                    writer.WriteLine("# Configuración de Migración");
                    writer.WriteLine($"NIT={config.NIT}");
                    writer.WriteLine($"RutaDescarga={config.RutaDescarga}");
                    writer.WriteLine();

                    writer.WriteLine("# Base de datos Front");
                    writer.WriteLine($"UsuarioFront={config.DatabaseFront.Usuario}");
                    writer.WriteLine($"PasswordFront={config.DatabaseFront.Password}");
                    writer.WriteLine($"IpFront={config.DatabaseFront.Ip}");
                    writer.WriteLine($"BaseDatosFront={config.DatabaseFront.NombreBaseDatos}");
                    writer.WriteLine();

                    writer.WriteLine("# Base de datos Back");
                    writer.WriteLine($"UsuarioBack={config.DatabaseBack.Usuario}");
                    writer.WriteLine($"PasswordBack={config.DatabaseBack.Password}");
                    writer.WriteLine($"IpBack={config.DatabaseBack.Ip}");
                    writer.WriteLine($"BaseDatosBack={config.DatabaseBack.NombreBaseDatos}");
                    writer.WriteLine();

                    writer.WriteLine("# Select Técnicos");
                    writer.WriteLine(selectBackFinal);
                    writer.WriteLine(selectFrontFinal);
                    writer.WriteLine();


                    writer.WriteLine("#Sp Config");
                    writer.WriteLine(SpEjecutadoBackFinal);
                    writer.WriteLine(SpNameBackFinal);
                    writer.WriteLine();

                    writer.WriteLine(SpEjecutadoFrontFinal);
                    writer.WriteLine(SpNameFrontFinal);
                    writer.WriteLine();

                    writer.WriteLine("#DIVIDIR ZIP (DividirZip  -> 1 sirve para activar la division (0 para desactivar), TAMAÑO DEL ZIP -> ENTEROS ENTRE 1-5 QUE REPRESENTA MB)");
                    writer.WriteLine(dividirZip);
                    writer.WriteLine(tamañozip);


                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error guardando configuración: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Busca una línea que empiece con 'SELECT_KEY=' o '# SELECT_KEY=' (ignorando espacios)
        /// y la devuelve tal cual está en el archivo. Si no existe, retorna null.
        /// </summary>
        private static string BuscarLineaConfigExistente(string[] lineas, string clave)
        {
            if (lineas == null || lineas.Length == 0) return null;

            // patrón: ^\s*#?\s*CLAVE\s*=
            var rx = new Regex(@"^\s*#?\s*" + Regex.Escape(clave) + @"\s*=", RegexOptions.IgnoreCase);

            // Devolver la PRIMERA coincidencia tal cual está
            return lineas.FirstOrDefault(l => rx.IsMatch(l));
        }
        private Dictionary<string, string> LeerPropiedadesArchivo(string rutaArchivo)
        {
            var propiedades = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var linea in File.ReadAllLines(rutaArchivo, Encoding.UTF8))
            {
                var lineaLimpia = linea.Trim();

                if (EsLineaValida(lineaLimpia))
                {
                    var (clave, valor) = ParsearLinea(lineaLimpia);
                    if (ClavesPermitidas.Contains(clave))
                    {
                        propiedades[clave] = ProcesarValor(valor);
                    }
                }
            }

            return propiedades;
        }

        public void ActualizarValorConfig(string rutaArchivo, string clave, string nuevoValor)
        {
            try
            {
                if (!ClavesPermitidas.Contains(clave))
                {
                    throw new ArgumentException($"La clave '{clave}' no es una clave permitida.");
                }

                if (!File.Exists(rutaArchivo))
                {
                    throw new FileNotFoundException($"El archivo de configuración no existe: {rutaArchivo}");
                }

                var lineas = File.ReadAllLines(rutaArchivo, Encoding.UTF8).ToList();
                bool claveEncontrada = false;

                for (int i = 0; i < lineas.Count; i++)
                {
                    var lineaLimpia = lineas[i].Trim();

                    if (!string.IsNullOrWhiteSpace(lineaLimpia) &&
                        !lineaLimpia.StartsWith("#") &&
                        !lineaLimpia.StartsWith("//") &&
                        !lineaLimpia.StartsWith(";"))
                    {
                        if (lineaLimpia.StartsWith(clave + "=", StringComparison.OrdinalIgnoreCase) ||
                            lineaLimpia.StartsWith(clave + " =", StringComparison.OrdinalIgnoreCase))
                        {

                            lineas[i] = $"{clave}={nuevoValor}";
                            claveEncontrada = true;
                            break;
                        }
                    }
                }


                if (!claveEncontrada)
                {
                    lineas.Add($"{clave}={nuevoValor}");
                }

                
                File.WriteAllLines(rutaArchivo, lineas, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error actualizando la configuración: {ex.Message}", ex);
            }
        }

        private static bool EsLineaValida(string linea)
        {
            return !string.IsNullOrWhiteSpace(linea) &&
                   !linea.StartsWith("#") &&
                   !linea.StartsWith("//") &&
                   !linea.StartsWith(";") &&
                   linea.Contains("=");
        }

        private static (string clave, string valor) ParsearLinea(string linea)
        {
            var indiceIgual = linea.IndexOf('=');
            var clave = linea.Substring(0, indiceIgual).Trim();
            var valor = linea.Substring(indiceIgual + 1).Trim();
            return (clave, valor);
        }

        private static string ProcesarValor(string valor)
        {
            // Quitar comillas
            if (valor.Length >= 2 && valor.StartsWith("\"") && valor.EndsWith("\""))
            {
                valor = valor.Substring(1, valor.Length - 2);
            }

            // Expandir variables de entorno
            if (valor.StartsWith("${") && valor.EndsWith("}"))
            {
                var nombreVariable = valor.Substring(2, valor.Length - 3);
                var valorVariable = Environment.GetEnvironmentVariable(nombreVariable);
                if (!string.IsNullOrEmpty(valorVariable))
                {
                    valor = valorVariable;
                }
            }

            return valor;
        }

        public bool ActualizarConfiguracionExistente(string rutaArchivo, ref MigracionConfig configExistente)
        {
            try
            {
                if (!File.Exists(rutaArchivo))
                    return false;

                var propiedades = LeerPropiedadesArchivo(rutaArchivo);
                if (propiedades.Count == 0)
                    return false;

                // Actualizar solo las propiedades que existen en el archivo
                AsignarPropiedadesAConfig(propiedades, configExistente);
                return true;
            }
            catch (Exception ex)
            {
                // Opcionalmente, puedes loggear el error
                System.Diagnostics.Debug.WriteLine($"Error actualizando configuración: {ex.Message}");
                return false;
            }
        }


        private static void AsignarPropiedadesAConfig(Dictionary<string, string> propiedades, MigracionConfig config)
        {
            if (propiedades.TryGetValue("NIT", out var nit))
                config.NIT = nit;

            if (propiedades.TryGetValue("RutaDescarga", out var ruta) && !string.IsNullOrWhiteSpace(ruta))
                config.RutaDescarga = ruta;

            // Front
            if (propiedades.TryGetValue("UsuarioFront", out var usuarioFront))
                config.DatabaseFront.Usuario = usuarioFront;
            if (propiedades.TryGetValue("PasswordFront", out var passwordFront))
                config.DatabaseFront.Password = passwordFront;
            if (propiedades.TryGetValue("IpFront", out var ipFront))
                config.DatabaseFront.Ip = ipFront;
            if (propiedades.TryGetValue("BaseDatosFront", out var bdFront))
                config.DatabaseFront.NombreBaseDatos = bdFront;
            if (propiedades.TryGetValue("SpFrontEjecutado", out var SpFront))
                config.DatabaseFront.SpEjecutado = bool.TryParse(SpFront, out bool valor) ? valor : false;
            if (propiedades.TryGetValue("SpNameFront", out var SpNameFront))
                config.DatabaseFront.SpName = SpNameFront;

            // Back
            if (propiedades.TryGetValue("UsuarioBack", out var usuarioBack))
                config.DatabaseBack.Usuario = usuarioBack;
            if (propiedades.TryGetValue("PasswordBack", out var passwordBack))
                config.DatabaseBack.Password = passwordBack;
            if (propiedades.TryGetValue("IpBack", out var ipBack))
                config.DatabaseBack.Ip = ipBack;
            if (propiedades.TryGetValue("BaseDatosBack", out var bdBack))
                config.DatabaseBack.NombreBaseDatos = bdBack;
            if (propiedades.TryGetValue("SpBackEjecutado", out var SpBack))
                config.DatabaseBack.SpEjecutado = bool.TryParse(SpBack, out bool valor) ? valor : false;
            if (propiedades.TryGetValue("SpNameBack", out var SpNameBack))
                config.DatabaseBack.SpName = SpNameBack;

            //ZIP
            if (propiedades.TryGetValue("DividirZip", out var dividirZip))
                config.DividirZip = (dividirZip == "1");
            if (propiedades.TryGetValue("Tamañozip", out var tamaniozip))
                config.tamanioZip = long.TryParse(tamaniozip, out long n) && n >= 1 && n <= 35 ? n *1024 * 1024 : 5 * 1024 * 1024;


        }
    }
}