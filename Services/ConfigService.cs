using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1.Services
{
    public interface IConfigService
    {
        bool CargarConfiguracion(string rutaArchivo, out MigracionConfig config);
        void GuardarConfiguracion(string rutaArchivo, MigracionConfig config);
    }

    public class ConfigService : IConfigService
    {
        private static readonly HashSet<string> ClavesPermitidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "NIT", "RutaDescarga",
            "UsuarioFront", "PasswordFront", "IpFront", "BaseDatosFront",
            "UsuarioBack", "PasswordBack", "IpBack", "BaseDatosBack"
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
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error guardando configuración: {ex.Message}", ex);
            }
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

            // Back
            if (propiedades.TryGetValue("UsuarioBack", out var usuarioBack))
                config.DatabaseBack.Usuario = usuarioBack;
            if (propiedades.TryGetValue("PasswordBack", out var passwordBack))
                config.DatabaseBack.Password = passwordBack;
            if (propiedades.TryGetValue("IpBack", out var ipBack))
                config.DatabaseBack.Ip = ipBack;
            if (propiedades.TryGetValue("BaseDatosBack", out var bdBack))
                config.DatabaseBack.NombreBaseDatos = bdBack;
        }
    }
}