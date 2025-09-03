using System;
using System.IO;
using System.Linq;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1.Services
{
    public interface IValidationService
    {
        ValidationResult ValidarConfiguracion(MigracionConfig config, TipoMigracion tipoMigracion);
    }

    public class ValidationService : IValidationService
    {
        public ValidationResult ValidarConfiguracion(MigracionConfig config, TipoMigracion tipoMigracion)
        {
            var resultado = new ValidationResult();

            // Validar NIT
            if (string.IsNullOrWhiteSpace(config.NIT))
            {
                resultado.AgregarError("NIT es requerido");
                return resultado;
            }

            // Validar años seleccionados
            if (!config.TodosLosAnios && (config.AniosSeleccionados == null || config.AniosSeleccionados.Length == 0))
            {
                resultado.AgregarError("Debe seleccionar al menos un año o la opción 'Todos los años'");
                return resultado;
            }

            // Validar ruta de descarga
            if (string.IsNullOrWhiteSpace(config.RutaDescarga) || !Directory.Exists(config.RutaDescarga))
            {
                resultado.AgregarError("La ruta de descarga no es válida");
                return resultado;
            }

            // Validar configuración según el tipo de migración
            var validacionTipo = ValidarTipoMigracion(config, tipoMigracion);
            if (!validacionTipo.EsValido)
            {
                return validacionTipo;
            }

            if ((validacionTipo?.Advertencias?.Length ?? 0) > 0)
            {
                foreach ( var item in validacionTipo.Advertencias)
                {
                    resultado.AgregarAdvertencia(item);
                }
            }

            resultado.EsValido = true;
            return resultado;
        }

        private ValidationResult ValidarTipoMigracion(MigracionConfig config, TipoMigracion tipo = TipoMigracion.NULL)
        {
            var resultado = new ValidationResult();
            bool frontCompleto = config.DatabaseFront.EstaCompleto();
            bool backCompleto = config.DatabaseBack.EstaCompleto();

            switch (tipo)
            {
                case TipoMigracion.NULL:
                        resultado.AgregarError("Seleccione un TIPO DE MIGRACÓN");
                        return resultado;

                case TipoMigracion.Front:
                    
                    if (!frontCompleto)
                    {
                        resultado.AgregarError("Para migrar FRONT, debe completar todos los campos de la base de datos Front (IP, Base de datos, Usuario, Contraseña)");
                        return resultado;
                    }

                    if (backCompleto)
                    {
                        resultado.AgregarAdvertencia("Seleccionó migrar solo FRONT, pero hay credenciales de BACK completas. Se ignorarán los datos de BACK.");
                    }
                    break;

                case TipoMigracion.Back:
                    if (!backCompleto)
                    {
                        resultado.AgregarError("Para migrar BACK, debe completar todos los campos de la base de datos Back (IP, Base de datos, Usuario, Contraseña)");
                        return resultado;
                    }

                    if (frontCompleto)
                    {
                        resultado.AgregarAdvertencia("Seleccionó migrar solo BACK, pero hay credenciales de FRONT completas. Se ignorarán los datos de FRONT.");
                    }
                    break;

                case TipoMigracion.Ambos:
                    if (!frontCompleto && !backCompleto)
                    {
                        resultado.AgregarError("Para migrar AMBOS, debe completar los campos de al menos una base de datos");
                        return resultado;
                    }

                    if (!frontCompleto)
                    {
                        resultado.AgregarAdvertencia("No se completaron los campos de FRONT. Solo se migrará BACK.");
                    }

                    if (!backCompleto)
                    {
                        resultado.AgregarAdvertencia("No se completaron los campos de BACK. Solo se migrará FRONT.");
                    }
                    break;
            }

            resultado.EsValido = true;
            return resultado;
        }
    }

    public class ValidationResult
    {
        public bool EsValido { get; set; }
        public string[] Errores { get; private set; }
        public string[] Advertencias { get; private set; }

        private readonly System.Collections.Generic.List<string> _errores;
        private readonly System.Collections.Generic.List<string> _advertencias;

        public ValidationResult()
        {
            _errores = new System.Collections.Generic.List<string>();
            _advertencias = new System.Collections.Generic.List<string>();
            EsValido = false;
        }

        public void AgregarError(string error)
        {
            _errores.Add(error);
            ActualizarArrays();
        }

        public void AgregarAdvertencia(string advertencia)
        {
            _advertencias.Add(advertencia);
            ActualizarArrays();
        }

        private void ActualizarArrays()
        {
            Errores = _errores.ToArray();
            Advertencias = _advertencias.ToArray();
        }

        public string ObtenerMensajeCompleto()
        {
            var mensaje = "";

            if (Errores.Length > 0)
            {
                mensaje += "ERRORES:\n" + string.Join("\n• ", Errores.Select(e => "• " + e)) + "\n\n";
            }

            if (Advertencias.Length > 0)
            {
                mensaje += "ADVERTENCIAS:\n" + string.Join("\n• ", Advertencias.Select(a => "• " + a));
            }

            return mensaje.Trim();
        }
    }
}