using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WindowsFormsApp1.Models;
using WindowsFormsApp1.Services;

namespace WindowsFormsApp1.Helpers
{
    public static class FormHelper
    {
        #region Gestión de años

        public static int[] GenerarAniosDisponibles(int cantidad = 9)
        {
            int anioActual = DateTime.Now.Year;
            int[] anios = new int[cantidad];
            for (int i = 0; i < cantidad; i++)
            {
                anios[i] = anioActual - i;
            }
            return anios;
        }

        public static CheckBox CrearCheckBoxAnio(int anio)
        {
            var checkbox = new CheckBox
            {
                Name = $"chk{anio}",
                Text = anio.ToString(),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = UIHelper.Colores.NeutroTexto,
                BackColor = UIHelper.Colores.Neutro,
                Size = new Size(70, 30),
                Margin = new Padding(5),
                TextAlign = ContentAlignment.MiddleCenter,
                Appearance = Appearance.Button,
                FlatStyle = FlatStyle.Flat,
                Tag = anio
            };

            UIHelper.ConfigurarEstiloCheckbox(checkbox);
            return checkbox;
        }

        public static void ConfigurarFlowPanelAnios(FlowLayoutPanel panel, CheckBox[] checkboxes)
        {
            panel.FlowDirection = FlowDirection.LeftToRight;
            panel.WrapContents = true;
            panel.Controls.Clear();
            panel.Controls.AddRange(checkboxes);
        }

        #endregion

        #region Actualización de formulario

        public static void ActualizarFormularioDesdeConfig(MigracionConfig config,
            TextBox txtNit, TextBox txtRutaDescarga,
            TextBox txtUsuarioFront, TextBox txtPasswordFront, TextBox txtIpFront, TextBox txtBaseDatosFront,
            TextBox txtUsuarioBack, TextBox txtPasswordBack, TextBox txtIpBack, TextBox txtBaseDatosBack)
        {
            txtNit.Text = config.NIT ?? "";
            txtRutaDescarga.Text = config.RutaDescarga ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // Front
            txtUsuarioFront.Text = config.DatabaseFront.Usuario ?? "";
            txtPasswordFront.Text = config.DatabaseFront.Password ?? "";
            txtIpFront.Text = config.DatabaseFront.Ip ?? "";
            txtBaseDatosFront.Text = config.DatabaseFront.NombreBaseDatos ?? "";

            // Back
            txtUsuarioBack.Text = config.DatabaseBack.Usuario ?? "";
            txtPasswordBack.Text = config.DatabaseBack.Password ?? "";
            txtIpBack.Text = config.DatabaseBack.Ip ?? "";
            txtBaseDatosBack.Text = config.DatabaseBack.NombreBaseDatos ?? "";
        }

        public static void ActualizarConfigDesdeFormulario(MigracionConfig config,
            CheckBox chkTodos, CheckBox[] checkboxAnios,
            TextBox txtNit, TextBox txtRutaDescarga,
            TextBox txtUsuarioFront, TextBox txtPasswordFront, TextBox txtIpFront, TextBox txtBaseDatosFront,
            TextBox txtUsuarioBack, TextBox txtPasswordBack, TextBox txtIpBack, TextBox txtBaseDatosBack)
        {
            config.NIT = txtNit.Text.Trim();
            config.RutaDescarga = txtRutaDescarga.Text.Trim();
            config.TodosLosAnios = chkTodos.Checked;

            if (!config.TodosLosAnios)
            {
                config.AniosSeleccionados = checkboxAnios
                    .Where(chk => chk.Checked)
                    .Select(chk => (int)chk.Tag)
                    .ToArray();
            }

            // Front
            config.DatabaseFront.Usuario = txtUsuarioFront.Text.Trim();
            config.DatabaseFront.Password = txtPasswordFront.Text;
            config.DatabaseFront.Ip = txtIpFront.Text.Trim();
            config.DatabaseFront.NombreBaseDatos = txtBaseDatosFront.Text.Trim();

            // Back
            config.DatabaseBack.Usuario = txtUsuarioBack.Text.Trim();
            config.DatabaseBack.Password = txtPasswordBack.Text;
            config.DatabaseBack.Ip = txtIpBack.Text.Trim();
            config.DatabaseBack.NombreBaseDatos = txtBaseDatosBack.Text.Trim();
        }

        #endregion

        #region Limpieza de formulario

        public static void LimpiarFormularioCompleto(MigracionConfig config,
            CheckBox chkTodos, CheckBox[] checkboxAnios,
            TextBox txtNit, TextBox txtRutaDescarga,
            TextBox txtUsuarioFront, TextBox txtPasswordFront, TextBox txtIpFront, TextBox txtBaseDatosFront,
            TextBox txtUsuarioBack, TextBox txtPasswordBack, TextBox txtIpBack, TextBox txtBaseDatosBack)
        {
            // Reiniciar configuración
            config.NIT = "";
            config.RutaDescarga = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            config.TodosLosAnios = false;
            config.AniosSeleccionados = new int[0];
            config.DatabaseFront = new DatabaseConfig();
            config.DatabaseBack = new DatabaseConfig();

            // Limpiar controles
            var textBoxes = new[] {
                txtNit, txtUsuarioFront, txtPasswordFront, txtIpFront, txtBaseDatosFront,
                txtUsuarioBack, txtPasswordBack, txtIpBack, txtBaseDatosBack
            };

            UIHelper.LimpiarTextBoxes(textBoxes);
            txtRutaDescarga.Text = config.RutaDescarga;

            // Desmarcar checkboxes
            UIHelper.DesmarcarCheckBoxes(checkboxAnios.Concat(new[] { chkTodos }).ToArray());

            txtNit.Focus();
        }

        #endregion

        #region Gestión de estados visuales

        public static void ActualizarEstadoTotalRegistros(TextBox txtTotalRegistros,
            CheckBox chkTodos, CheckBox[] checkboxAnios)
        {
            if (chkTodos.Checked)
            {
                UIHelper.EstablecerEstadoTotalRegistros(txtTotalRegistros,
                    "Todos los registros", EstadoRegistro.Todos);
            }
            else
            {
                var cantidadSeleccionados = checkboxAnios.Count(chk => chk.Checked);

                if (cantidadSeleccionados == 0)
                {
                    UIHelper.EstablecerEstadoTotalRegistros(txtTotalRegistros,
                        "0 registros", EstadoRegistro.Ninguno);
                }
                else
                {
                    UIHelper.EstablecerEstadoTotalRegistros(txtTotalRegistros,
                        $"{cantidadSeleccionados} año(s) seleccionado(s)", EstadoRegistro.Algunos);
                }
            }
        }

        public static void ActualizarEstadoCheckboxTodos(CheckBox chkTodos, CheckBox[] checkboxAnios)
        {
            if (chkTodos.Checked)
            {
                UIHelper.DesmarcarCheckBoxes(checkboxAnios);
                UIHelper.EstablecerEstadoCheckboxTodos(chkTodos, true);
            }
            else
            {
                UIHelper.EstablecerEstadoCheckboxTodos(chkTodos, false);
            }
        }

        public static void ActualizarEstadoCheckboxAnio(CheckBox checkboxAnio,
            CheckBox chkTodos, CheckBox[] todosLosCheckbox)
        {
            if (checkboxAnio.Checked)
            {
                chkTodos.Checked = false;
                UIHelper.EstablecerEstadoCheckboxAnio(checkboxAnio, true);
            }
            else
            {
                UIHelper.EstablecerEstadoCheckboxAnio(checkboxAnio, false);
            }
        }

        #endregion

        #region Configuración inicial

        public static void ConfigurarFormularioInicial(Form formulario, ProgressBar progressBar,
            TextBox txtTotalRegistros, TextBox txtRutaDescarga)
        {
            txtTotalRegistros.Text = "0 registros";
            txtRutaDescarga.Text = AppDomain.CurrentDomain.BaseDirectory;
            progressBar.Visible = false;
        }

        public static void ConfigurarEstilosBotones(Button btnGuardar, Button btnLimpiar,
            Button btnBuscarRuta, Button btnVerificar)
        {
            UIHelper.ConfigurarBotonPrimario(btnGuardar);
            UIHelper.ConfigurarBotonPeligro(btnLimpiar);
            UIHelper.ConfigurarBotonSecundario(btnBuscarRuta);
            UIHelper.ConfigurarBotonVerificacion(btnVerificar);
        }

        public static void ConfigurarEstilosTextBoxes(params TextBox[] textBoxes)
        {
            foreach (var textBox in textBoxes)
            {
                UIHelper.AplicarEfectoFocus(textBox);
            }
        }

        #endregion

        #region Selector de carpeta

        public static string SeleccionarCarpeta(string rutaActual, string descripcion = "Seleccionar carpeta de descarga")
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = descripcion;
                folderDialog.SelectedPath = rutaActual;
                folderDialog.ShowNewFolderButton = true;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    return folderDialog.SelectedPath;
                }
            }
            return rutaActual; // Retornar la ruta actual si se cancela
        }

        #endregion

        #region Generación de resúmenes

        public static string GenerarResumenMigracion(MigracionConfig config)
        {
            var lapso = config.TodosLosAnios ? "Todos los años" :
                $"Años: {string.Join(", ", config.AniosSeleccionados.OrderByDescending(x => x))}";

            var front = config.DatabaseFront.EstaCompleto() ? "Configurado" : "No configurado";
            var back = config.DatabaseBack.EstaCompleto() ? "Configurado" : "No configurado";

            return $"RESUMEN DE MIGRACIÓN:\n" +
                   $"• NIT: {config.NIT}\n" +
                   $"• Base Front: {front}\n" +
                   $"• Base Back: {back}\n" +
                   $"• Lapso: {lapso}\n" +
                   $"• Ruta: {config.RutaDescarga}";
        }

        public static string GenerarMensajeResultado(MigracionResult resultado, MigracionResult resultadoBack = null, TipoMigracion tipo = TipoMigracion.NULL)
        {
            if (tipo != TipoMigracion.NULL && tipo==TipoMigracion.Ambos)
            {
                var mensajeBack = $"Migración BACK {(resultadoBack.Exitoso ? "completada" : "completada con errores")}!\n\n" +
                                $"Archivos generados: {resultadoBack.ArchivosGenerados}\n" +
                                $"Años procesados: {resultadoBack.AniosProcesados.Count}\n" +
                                $"Archivos enviados a API: {resultadoBack.ArchivosEnviadosAPI}\n" +
                                $"Errores: {resultadoBack.Errores.Count}\n" +
                                $"Duración: {resultadoBack.DuracionTotal:hh\\:mm\\:ss}\n" +
                                $"Directorio: {resultadoBack.DirectorioArchivos}";

                var mensajeFront =
                    $"Migración FRONT {(resultado.Exitoso ? "completada" : "completada con errores")}!\n\n" +
                    $"Archivos generados: {resultado.ArchivosGenerados}\n" +
                    $"Años procesados: {resultado.AniosProcesados.Count}\n" +
                    $"Archivos enviados a API: {resultado.ArchivosEnviadosAPI}\n" +
                    $"Errores: {resultado.Errores.Count}\n" +
                    $"Duración: {resultado.DuracionTotal:hh\\:mm\\:ss}\n" +
                    $"Directorio: {resultado.DirectorioArchivos}";

                return mensajeBack + "\n\n" + mensajeFront;

            }
            return $"Migración {(resultado.Exitoso ? "completada" : "completada con errores")}!\n\n" +
                       $"Archivos generados: {resultado.ArchivosGenerados}\n" +
                       $"Años procesados: {resultado.AniosProcesados.Count}\n" +
                       $"Archivos enviados a API: {resultado.ArchivosEnviadosAPI}\n" +
                       $"Errores: {resultado.Errores.Count}\n" +
                       $"Duración: {resultado.DuracionTotal:hh\\:mm\\:ss}\n" +
                       $"Directorio: {resultado.DirectorioArchivos}";
        }

        public static void ConfigurarValidacionNIT(TextBox txtNit)
        {
            txtNit.KeyPress += (sender, e) =>
            {
                // Convertir punto en espacio
                if (e.KeyChar == '.')
                {
                    e.KeyChar = ' ';
                }

                // Permitir solo números, backspace y delete
                if (!(char.IsDigit(e.KeyChar) ||
                      e.KeyChar == (char)Keys.Back ||
                      e.KeyChar == (char)Keys.Delete))
                {
                    e.Handled = true;
                }
            };
        }
        #endregion
    }
}