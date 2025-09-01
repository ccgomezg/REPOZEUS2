using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.Helpers;
using WindowsFormsApp1.Models;
using WindowsFormsApp1.Services;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        #region Servicios y Estado
        private IValidationService _validationService;
        private IConfigService _configService;
        private IDatabaseService _databaseService;
        private IMigracionService _migracionService;

        private MigracionConfig _config;
        private CheckBox[] _checkboxAnios;
        private int[] _aniosDisponibles;
        #endregion

        public Form1()
        {
            InitializeComponent();
            InicializarServicios();
            InicializarFormulario();
        }

        #region Inicialización
        private void InicializarServicios()
        {
            _validationService = new ValidationService();
            _configService = new ConfigService();
            _databaseService = new DatabaseService();
            
            var fileService = new FileService();
            var apiService = new ApiService(fileService);
            _migracionService = new MigracionService(_databaseService, fileService, apiService);
        }

        private void InicializarFormulario()
        {
            InicializarEstado();
            ConfigurarEventos();
            InicializarAnios();
            ConfigurarEstilos();
            CargarConfiguracionInicial();
            FormHelper.ConfigurarFormularioInicial(this, progressBar, txtTotalRegistros, txtRutaDescarga);
        }

        private void InicializarEstado()
        {
            _config = new MigracionConfig
            {
                RutaDescarga = AppDomain.CurrentDomain.BaseDirectory
            };
        }

        private void ConfigurarEventos()
        {
            btnBuscarRuta.Click += (s, e) => BuscarRuta();
            btnGuardar.Click += async (s, e) => await ProcesarMigracionAsync();
            btnLimpiar.Click += (s, e) => LimpiarFormulario();
            btnverificar.Click += async (s, e) => await VerificarConexionesAsync();
            chkTodos.CheckedChanged += (s, e) => ManejarCambioTodos();
            FormHelper.ConfigurarValidacionNIT(txtNit);
            this.Load += (s, e) => FormHelper.ActualizarEstadoTotalRegistros(txtTotalRegistros, chkTodos, _checkboxAnios);
        }

        

        private void InicializarAnios()
        {
            _aniosDisponibles = FormHelper.GenerarAniosDisponibles();
            _checkboxAnios = new CheckBox[_aniosDisponibles.Length];

            for (int i = 0; i < _aniosDisponibles.Length; i++)
            {
                _checkboxAnios[i] = FormHelper.CrearCheckBoxAnio(_aniosDisponibles[i]);
                _checkboxAnios[i].CheckedChanged += (s, e) => ManejarCambioAnio((CheckBox)s);
            }

            FormHelper.ConfigurarFlowPanelAnios(flowPanelAnios, _checkboxAnios);
        }

        private void ConfigurarEstilos()
        {
            FormHelper.ConfigurarEstilosBotones(btnGuardar, btnLimpiar, btnBuscarRuta, btnverificar);
            FormHelper.ConfigurarEstilosTextBoxes(txtNit, txtUsuarioFront, txtPasswordFront,
                txtIpFront, txtBaseDatosFront, txtUsuarioBack, txtPasswordBack, txtIpBack, txtBaseDatosBack);
        }

        private void CargarConfiguracionInicial()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt");

            if (_configService.CargarConfiguracion(configPath, out var config))
            {
                _config = config;
                FormHelper.ActualizarFormularioDesdeConfig(_config, txtNit, txtRutaDescarga,
                    txtUsuarioFront, txtPasswordFront, txtIpFront, txtBaseDatosFront,
                    txtUsuarioBack, txtPasswordBack, txtIpBack, txtBaseDatosBack);
                FormHelper.ActualizarEstadoTotalRegistros(txtTotalRegistros, chkTodos, _checkboxAnios);
            }
        }
        #endregion

        #region Event Handlers
        private void BuscarRuta()
        {
            var nuevaRuta = FormHelper.SeleccionarCarpeta(txtRutaDescarga.Text);
            txtRutaDescarga.Text = nuevaRuta;
            _config.RutaDescarga = nuevaRuta;
        }

        private void ManejarCambioTodos()
        {
            FormHelper.ActualizarEstadoCheckboxTodos(chkTodos, _checkboxAnios);
            FormHelper.ActualizarEstadoTotalRegistros(txtTotalRegistros, chkTodos, _checkboxAnios);
        }

        private void ManejarCambioAnio(CheckBox checkboxAnio)
        {
            FormHelper.ActualizarEstadoCheckboxAnio(checkboxAnio, chkTodos, _checkboxAnios);
            FormHelper.ActualizarEstadoTotalRegistros(txtTotalRegistros, chkTodos, _checkboxAnios);
        }

        private async Task VerificarConexionesAsync()
        {
            btnverificar.Enabled = false;
            btnverificar.Text = "Verificando...";

            try
            {
                FormHelper.ActualizarConfigDesdeFormulario(_config, chkTodos, _checkboxAnios,
                    txtNit, txtRutaDescarga, txtUsuarioFront, txtPasswordFront, txtIpFront, txtBaseDatosFront,
                    txtUsuarioBack, txtPasswordBack, txtIpBack, txtBaseDatosBack);

                var resultados = await ObtenerResultadosVerificacion();
                MostrarResultadosVerificacion(resultados);
            }
            catch (Exception ex)
            {
                UIHelper.MostrarError($"Error durante la verificación: {ex.Message}");
            }
            finally
            {
                btnverificar.Enabled = true;
                btnverificar.Text = "Verificar Conexión";
            }
        }

        private async Task<(string front, string back)> ObtenerResultadosVerificacion()
        {
            string resultadoFront = "";
            string resultadoBack = "";

            if (_config.DatabaseFront.EstaCompleto())
            {
                var (ok, mensaje) = await _databaseService.VerificarConexionAsync(_config.DatabaseFront, 8);
                resultadoFront = $"● Conexión Front: {mensaje}";
            }

            if (_config.DatabaseBack.EstaCompleto())
            {
                var (ok, mensaje) = await _databaseService.VerificarConexionAsync(_config.DatabaseBack, 8);
                resultadoBack = $"● Conexión Back: {mensaje}";
            }

            return (resultadoFront, resultadoBack);
        }

        private void MostrarResultadosVerificacion((string front, string back) resultados)
        {
            if (string.IsNullOrEmpty(resultados.front) && string.IsNullOrEmpty(resultados.back))
            {
                UIHelper.MostrarAdvertencia("Debe completar al menos una configuración de base de datos para verificar.",
                    "Datos incompletos");
                return;
            }

            var mensaje = string.Join("\n\n", new[] { resultados.front, resultados.back }
                .Where(r => !string.IsNullOrEmpty(r)));

            UIHelper.MostrarInformacion(mensaje, "Resultado de Verificación");
        }

        private async Task ProcesarMigracionAsync()
        {
            FormHelper.ActualizarConfigDesdeFormulario(_config, chkTodos, _checkboxAnios,
                txtNit, txtRutaDescarga, txtUsuarioFront, txtPasswordFront, txtIpFront, txtBaseDatosFront,
                txtUsuarioBack, txtPasswordBack, txtIpBack, txtBaseDatosBack);

            if (!ValidarYConfirmarMigracion())
                return;

            await EjecutarMigracionAsync();
        }

        private bool ValidarYConfirmarMigracion()
        {
            var tipoMigracion = ObtenerTipoMigracionSeleccionado();
            var validacion = _validationService.ValidarConfiguracion(_config, tipoMigracion);

            if (!validacion.EsValido)
            {
                UIHelper.MostrarAdvertencia(validacion.ObtenerMensajeCompleto(), "Error de Validación");
                return false;
            }


            // Mostrar advertencias si las hay
            if ((validacion?.Advertencias?.Length ?? 0) > 0)
            {
                if (!UIHelper.ConfirmarAccion(validacion.ObtenerMensajeCompleto() + "\n\n¿Desea continuar?", "Advertencias"))
                    return false;
            }

            // Confirmación final
            var resumen = FormHelper.GenerarResumenMigracion(_config);
            return UIHelper.ConfirmarAccion($"¿Confirma que desea proceder con la migración?\n\n{resumen}",
                "Confirmar Migración");
        }

        private async Task EjecutarMigracionAsync()
        {
            ConfigurarUIParaMigracion(true);

            try
            {
                var progreso = new Progress<int>(valor => progressBar.Value = valor);
                var resultado = await _migracionService.EjecutarMigracionAsync(_config, progreso);

                var mensaje = FormHelper.GenerarMensajeResultado(resultado);
                var icono = resultado.Exitoso ? MessageBoxIcon.Information : MessageBoxIcon.Warning;

                MessageBox.Show(mensaje, "Proceso Completado", MessageBoxButtons.OK, icono);
            }
            catch (Exception ex)
            {
                UIHelper.MostrarError($"Error durante la migración:\n\n{ex.Message}", "Error de Proceso");
            }
            finally
            {
                ConfigurarUIParaMigracion(false);
            }
        }

        private void ConfigurarUIParaMigracion(bool iniciando)
        {
            if (iniciando)
            {
                progressBar.Visible = true;
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Value = 0;
            }
            else
            {
                progressBar.Visible = false;
            }

            var controles = new Control[] { btnGuardar, btnLimpiar, btnverificar, btnBuscarRuta };
            UIHelper.HabilitarControles(controles, !iniciando);
        }

        private void LimpiarFormulario()
        {
            if (UIHelper.ConfirmarAccion("¿Está seguro que desea limpiar todos los campos?", "Confirmar limpieza"))
            {
                FormHelper.LimpiarFormularioCompleto(_config, chkTodos, _checkboxAnios,
                    txtNit, txtRutaDescarga, txtUsuarioFront, txtPasswordFront, txtIpFront, txtBaseDatosFront,
                    txtUsuarioBack, txtPasswordBack, txtIpBack, txtBaseDatosBack);
                FormHelper.ActualizarEstadoTotalRegistros(txtTotalRegistros, chkTodos, _checkboxAnios);
            }
        }
        #endregion

        #region Métodos Auxiliares
        private TipoMigracion ObtenerTipoMigracionSeleccionado()
        {
            if (rbMigrarAmbos.Checked) return TipoMigracion.Ambos;
            if (rbMigrarFront.Checked) return TipoMigracion.Front;
            return TipoMigracion.Back;
        }
        #endregion

        #region Métodos heredados del diseñador
        private void Form1_Load_1(object sender, EventArgs e) { }
        private void grpFront_Enter(object sender, EventArgs e) { }
        private void chkTodos_CheckedChanged_1(object sender, EventArgs e) { }
        private void button1_Click(object sender, EventArgs e) { }
        private void txtIpFront_TextChanged(object sender, EventArgs e) { }
        #endregion
    }
}

