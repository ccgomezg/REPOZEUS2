using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        // Variables para almacenar los valores del formulario
        public string NIT { get; private set; }
        public string UsuarioZeus { get; private set; }
        public string PasswordZeus { get; private set; }
        public string UsuarioFront { get; private set; }
        public string PasswordFront { get; private set; }
        public string IpFront { get; private set; }
        public string UsuarioBack { get; private set; }
        public string PasswordBack { get; private set; }
        public string IpBack { get; private set; }
        public string RutaDescarga { get; private set; }
        public bool TodosLosAnios { get; private set; }
        public int[] AniosSeleccionados { get; private set; }
        public int TotalRegistrosEstimados { get; private set; }

        private CheckBox[] chkAnios;
        private int[] aniosDisponibles;

        public Form1()
        {
            InitializeComponent();
            ConfigurarEventos();
            InicializarAnios();
            ConfigurarEstilos();
        }

        private void ConfigurarEventos()
        {
            // Eventos de botones
            btnBuscarRuta.Click += BtnBuscarRuta_Click;
            btnGuardar.Click += BtnGuardar_Click;
            btnLimpiar.Click += BtnLimpiar_Click;
            btnverificar.Click += BtnVerificar_Click;


            // Evento para el checkbox "TODOS"
            chkTodos.CheckedChanged += ChkTodos_CheckedChanged;

            // Evento Load del formulario
            this.Load += Form1_Load;
        }

        private async Task<(bool Ok, string Mensaje)> VerificarConexionBackAsync(
        string ipServidor, string baseDatos, string usuario, string password,
        int timeoutSeg = 5, CancellationToken cancellationToken = default)
            {
                try
                {
                    // Construimos el connection string usando SqlConnectionStringBuilder
                    var csb = new SqlConnectionStringBuilder
                    {
                        DataSource = ipServidor,        // Ej: "192.168.1.200,1433" o "192.168.1.200\\SQLEXPRESS"
                        InitialCatalog = baseDatos,     // Ej: "BackDB"
                        UserID = usuario,
                        Password = password,
                        ConnectTimeout = timeoutSeg,
                        Encrypt = false,
                        TrustServerCertificate = true
                    };

                    using (var cn = new SqlConnection(csb.ConnectionString))
                    {
                        await cn.OpenAsync(cancellationToken);

                        using (var cmd = new SqlCommand("SELECT 1", cn))
                        {
                            var r = await cmd.ExecuteScalarAsync(cancellationToken);
                            return (Convert.ToInt32(r) == 1, "Conexión OK con el servidor Back.");
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    return (false, "Tiempo de espera agotado al abrir la conexión.");
                }
                catch (SqlException ex)
                {
                    return (false, $"Error SQL ({ex.Number}): {ex.Message}");
                }
                catch (Exception ex)
                {
                    return (false, $"Error: {ex.Message}");
                }
            }




        private async Task VerificarConexionBackUIAsync()
        {
            // Tomar datos desde el formulario
            string ip = txtIpBack.Text.Trim();
            string usuario = txtUsuarioBack.Text.Trim();
            string password = txtPasswordBack.Text;

            if (string.IsNullOrWhiteSpace(ip) ||
                string.IsNullOrWhiteSpace(usuario) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Debe ingresar IP, usuario y contraseña de la Base BACK.",
                    "Datos incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Normalizar ip:puerto -> ip,puerto (SqlClient usa coma)
            if (ip.Contains(":") && !ip.Contains(",")) ip = ip.Replace(":", ",");

            try
            {
                var (ok, mensaje) = await VerificarConexionBackAsync(
                    ip, "TimbradoMasivo_Test", usuario, password, timeoutSeg: 8);

                MessageBox.Show(mensaje,
                    ok ? "Conexión Exitosa" : "Error de Conexión",
                    MessageBoxButtons.OK,
                    ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void InicializarAnios()
        {
            // Generar últimos 7 años desde la fecha actual
            int anioActual = DateTime.Now.Year;
            aniosDisponibles = new int[7];
            for (int i = 0; i < 7; i++)
            {
                aniosDisponibles[i] = anioActual - i;
            }

            // Crear checkboxes para cada año
            chkAnios = new CheckBox[7];
            flowPanelAnios.FlowDirection = FlowDirection.LeftToRight;
            flowPanelAnios.WrapContents = true;

            for (int i = 0; i < 7; i++)
            {
                chkAnios[i] = new CheckBox
                {
                    Name = $"chk{aniosDisponibles[i]}",
                    Text = aniosDisponibles[i].ToString(),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(52, 73, 94),
                    BackColor = Color.FromArgb(236, 240, 241),
                    Size = new Size(70, 30),
                    Margin = new Padding(5),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Appearance = Appearance.Button,
                    FlatStyle = FlatStyle.Flat,
                    Tag = aniosDisponibles[i]
                };

                chkAnios[i].FlatAppearance.BorderSize = 1;
                chkAnios[i].FlatAppearance.BorderColor = Color.FromArgb(189, 195, 199);
                chkAnios[i].FlatAppearance.CheckedBackColor = Color.FromArgb(52, 152, 219);
                chkAnios[i].CheckedChanged += CheckBoxAnio_CheckedChanged;

                flowPanelAnios.Controls.Add(chkAnios[i]);
            }
        }

        private void ConfigurarEstilos()
        {
            // Aplicar efectos hover a los botones
            AplicarEfectoHover(btnGuardar, Color.FromArgb(39, 174, 96), Color.FromArgb(46, 204, 113));
            AplicarEfectoHover(btnLimpiar, Color.FromArgb(192, 57, 43), Color.FromArgb(231, 76, 60));
            AplicarEfectoHover(btnBuscarRuta, Color.FromArgb(142, 68, 173), Color.FromArgb(155, 89, 182));

            // Aplicar efectos focus a los textbox
            AplicarEfectoFocus(txtNit);
            AplicarEfectoFocus(txtUsuarioFront);
            AplicarEfectoFocus(txtPasswordFront);
            AplicarEfectoFocus(txtIpFront);
            AplicarEfectoFocus(txtUsuarioBack);
            AplicarEfectoFocus(txtPasswordBack);
            AplicarEfectoFocus(txtIpBack);
        }

        private void AplicarEfectoHover(Button boton, Color colorHover, Color colorNormal)
        {
            boton.MouseEnter += (s, e) =>
            {
                boton.BackColor = colorHover;
                boton.Cursor = Cursors.Hand;
            };
            boton.MouseLeave += (s, e) =>
            {
                boton.BackColor = colorNormal;
                boton.Cursor = Cursors.Default;
            };
        }

        private void AplicarEfectoFocus(TextBox textBox)
        {
            textBox.Enter += (s, e) =>
            {
                textBox.BackColor = Color.FromArgb(174, 214, 241);
                textBox.BorderStyle = BorderStyle.FixedSingle;
            };
            textBox.Leave += (s, e) =>
            {
                textBox.BackColor = Color.White;
            };
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Configuración inicial
            txtTotalRegistros.Text = "0 registros";
            txtRutaDescarga.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            RutaDescarga = txtRutaDescarga.Text;

            // Mostrar año actual por defecto
            ActualizarTotalRegistros();
        }

        private void BtnBuscarRuta_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Seleccionar carpeta de descarga";
                folderDialog.SelectedPath = txtRutaDescarga.Text;
                folderDialog.ShowNewFolderButton = true;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtRutaDescarga.Text = folderDialog.SelectedPath;
                    RutaDescarga = folderDialog.SelectedPath;
                }
            }
        }

        private void ChkTodos_CheckedChanged(object sender, EventArgs e)
        {
            // Si se selecciona "TODOS", desmarcar todos los años individuales
            if (chkTodos.Checked)
            {
                foreach (var chk in chkAnios)
                {
                    chk.Checked = false;
                }
                chkTodos.BackColor = Color.FromArgb(46, 204, 113);
                chkTodos.ForeColor = Color.White;
            }
            else
            {
                chkTodos.BackColor = Color.FromArgb(52, 152, 219);
                chkTodos.ForeColor = Color.White;
            }
            ActualizarTotalRegistros();
        }

        private void CheckBoxAnio_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkbox = sender as CheckBox;

            // Si se selecciona algún año individual, desmarcar "TODOS"
            if (checkbox.Checked)
            {
                chkTodos.Checked = false;
                checkbox.ForeColor = Color.White;
            }
            else
            {
                checkbox.ForeColor = Color.FromArgb(52, 73, 94);
            }

            ActualizarTotalRegistros();
        }

        private void ActualizarTotalRegistros()
        {
            // Actualizar variables
            TodosLosAnios = chkTodos.Checked;

            if (TodosLosAnios)
            {
                AniosSeleccionados = aniosDisponibles;
                txtTotalRegistros.Text = "Todos los registros";
                txtTotalRegistros.BackColor = Color.FromArgb(46, 204, 113);
                txtTotalRegistros.ForeColor = Color.White;
                TotalRegistrosEstimados = -1; // -1 indica todos
            }
            else
            {
                var aniosSeleccionados = chkAnios
                    .Where(chk => chk.Checked)
                    .Select(chk => (int)chk.Tag)
                    .ToArray();

                AniosSeleccionados = aniosSeleccionados;

                if (aniosSeleccionados.Length == 0)
                {
                    txtTotalRegistros.Text = "0 registros";
                    txtTotalRegistros.BackColor = Color.FromArgb(236, 240, 241);
                    txtTotalRegistros.ForeColor = Color.FromArgb(52, 73, 94);
                    TotalRegistrosEstimados = 0;
                }
                else
                {
                    txtTotalRegistros.Text = $"{aniosSeleccionados.Length} año(s) seleccionado(s)";
                    txtTotalRegistros.BackColor = Color.FromArgb(52, 152, 219);
                    txtTotalRegistros.ForeColor = Color.White;
                    TotalRegistrosEstimados = aniosSeleccionados.Length * 1000; // Estimación
                }
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario())
                return;

            // Guardar todos los valores en las variables de clase
            GuardarValores();

            // Mostrar progress bar
            progressBar.Visible = true;
            progressBar.Style = ProgressBarStyle.Marquee;

            // Mostrar confirmación con resumen
            string resumen = GenerarResumen();
            DialogResult resultado = MessageBox.Show(
                $"¿Confirma que desea proceder con la migración?\n\n{resumen}",
                "Confirmar Migración",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            progressBar.Visible = false;

            if (resultado == DialogResult.Yes)
            {
                EjecutarMigracion();
            }
        }

        private bool ValidarFormulario()
        {
            // Validar NIT
            if (string.IsNullOrWhiteSpace(txtNit.Text))
            {
                MessageBox.Show("Por favor ingrese el NIT", "Campo requerido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNit.Focus();
                return false;
            }

            // Validar que se haya seleccionado al menos un lapso
            if (!chkTodos.Checked && !chkAnios.Any(chk => chk.Checked))
            {
                MessageBox.Show("Por favor seleccione al menos un lapso de tiempo", "Lapso requerido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Validar ruta de descarga
            if (!Directory.Exists(txtRutaDescarga.Text))
            {
                MessageBox.Show("La ruta de descarga no es válida", "Ruta inválida",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void GuardarValores()
        {
            NIT = txtNit.Text.Trim();
            UsuarioFront = txtUsuarioFront.Text.Trim();
            PasswordFront = txtPasswordFront.Text;
            IpFront = txtIpFront.Text.Trim();
            UsuarioBack = txtUsuarioBack.Text.Trim();
            PasswordBack = txtPasswordBack.Text;
            IpBack = txtIpBack.Text.Trim();
            RutaDescarga = txtRutaDescarga.Text.Trim();
        }

        private string GenerarResumen()
        {
            string lapso = TodosLosAnios ? "Todos los años" :
                $"Años: {string.Join(", ", AniosSeleccionados.OrderByDescending(x => x))}";

            return $"RESUMEN DE MIGRACIÓN:\n" +
                   $"• NIT: {NIT}\n" +
                   $"• Usuario Zeus: {UsuarioZeus}\n" +
                   $"• Usuario BD Front: {(string.IsNullOrEmpty(UsuarioFront) ? "No especificado" : UsuarioFront)}\n" +
                   $"• IP Front: {(string.IsNullOrEmpty(IpFront) ? "No especificado" : IpFront)}\n" +
                   $"• Usuario BD Back: {(string.IsNullOrEmpty(UsuarioBack) ? "No especificado" : UsuarioBack)}\n" +
                   $"• IP Back: {(string.IsNullOrEmpty(IpBack) ? "No especificado" : IpBack)}\n" +
                   $"• Lapso: {lapso}\n" +
                   $"• Ruta: {RutaDescarga}\n" +
                   $"• Registros estimados: {(TotalRegistrosEstimados == -1 ? "Todos" : TotalRegistrosEstimados.ToString("N0"))}";
        }

        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
            DialogResult resultado = MessageBox.Show(
                "¿Está seguro que desea limpiar todos los campos?",
                "Confirmar limpieza",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (resultado == DialogResult.Yes)
            {
                LimpiarFormulario();
            }
        }

        private async void BtnVerificar_Click(object sender, EventArgs e)
        {
            // Tomar datos del formulario
            string ip = txtIpBack.Text.Trim();
            string usuario = txtUsuarioBack.Text.Trim();
            string password = txtPasswordBack.Text;

            string ip_front = txtIpFront.Text.Trim();
            string usuario_front = txtUsuarioFront.Text.Trim();
            string password_front = txtPasswordFront.Text;

            if ((string.IsNullOrWhiteSpace(ip) && string.IsNullOrWhiteSpace(ip_front)) ||
                (string.IsNullOrWhiteSpace(usuario) && string.IsNullOrWhiteSpace(usuario_front)) ||
                (string.IsNullOrWhiteSpace(password) && string.IsNullOrWhiteSpace(password_front)))
            {
                MessageBox.Show("Debe ingresar IP, usuario y contraseña \n DE AL MENOS 1 BASE DE DATOS.",
                    "Datos incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Normaliza ip:puerto -> ip,puerto (SqlClient espera coma)
            if (ip.Contains(":") && !ip.Contains(",")) ip = ip.Replace(":", ",");
            try
            {
                var (ok, mensaje) = await VerificarConexionBackAsync(
                    ip, "TimbradoMasivo_Test", usuario, password, timeoutSeg: 8);

                var (ok2, mensaje2) = await VerificarConexionBackAsync(
                    ip_front, "TimbradoMasivo_Test", usuario_front, password_front, timeoutSeg: 8);

                string respuestasPeticiones = $"● Conexion Back : {mensaje} \n\n" + $"● Conexion Front : {mensaje2}";

                if (ok || ok2)
                {
                    MessageBox.Show("✅ Credenciales correctas.\n\n" + respuestasPeticiones,
                        "Resultado de la conexión: ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("❌ Credenciales incorrectas.\n\n" + respuestasPeticiones,
                        "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                //Cursor = Cursors.Default;
                //btnverificar.Enabled = true;
            }
        }


        private void LimpiarFormulario()
        {
            // Limpiar todos los campos
            txtNit.Clear();
            txtUsuarioFront.Clear();
            txtPasswordFront.Clear();
            txtIpFront.Clear();
            txtUsuarioBack.Clear();
            txtPasswordBack.Clear();
            txtIpBack.Clear();
            txtRutaDescarga.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // Desmarcar todos los checkboxes
            chkTodos.Checked = false;
            foreach (var chk in chkAnios)
            {
                chk.Checked = false;
            }

            // Limpiar variables
            NIT = string.Empty;
            UsuarioZeus = string.Empty;
            PasswordZeus = string.Empty;
            UsuarioFront = string.Empty;
            PasswordFront = string.Empty;
            IpFront = string.Empty;
            UsuarioBack = string.Empty;
            PasswordBack = string.Empty;
            IpBack = string.Empty;
            RutaDescarga = txtRutaDescarga.Text;
            TodosLosAnios = false;
            AniosSeleccionados = new int[0];
            TotalRegistrosEstimados = 0;

            ActualizarTotalRegistros();
            txtNit.Focus();
        }

        private async void EjecutarMigracion()
        {
            try
            {
                progressBar.Visible = true;
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Value = 0;
                progressBar.Maximum = 100;
                btnGuardar.Enabled = false;
                btnLimpiar.Enabled = false;

                // Determinar años a procesar
                int[] aniosAProcesar = TodosLosAnios ? aniosDisponibles : AniosSeleccionados;

                if (aniosAProcesar.Length == 0)
                {
                    MessageBox.Show("No hay años seleccionados para procesar.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                progressBar.Value = 10;

                // Crear directorio para archivos si no existe
                string directorioArchivos = Path.Combine(RutaDescarga, $"Migracion_{NIT}_{DateTime.Now:yyyyMMdd_HHmmss}");
                Directory.CreateDirectory(directorioArchivos);

                List<string> archivosGenerados = new List<string>();
                int progresoPorAnio = 80 / aniosAProcesar.Length;

                // Procesar cada año
                foreach (int anio in aniosAProcesar)
                {
                    try
                    {
                        string archivoAnio = await ProcesarAnio(anio, directorioArchivos);
                        if (!string.IsNullOrEmpty(archivoAnio))
                        {
                            archivosGenerados.Add(archivoAnio);
                        }
                        progressBar.Value += progresoPorAnio;
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = $"Error procesando año {anio}: {ex.Message}";
                        MessageBox.Show(errorMsg, "Error de Procesamiento",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        // Crear archivo de error
                        File.WriteAllText(Path.Combine(directorioArchivos, $"ERROR_{NIT}_{anio}.txt"),
                            $"Error: {errorMsg}\nFecha: {DateTime.Now}");
                    }
                }

                progressBar.Value = 90;

                // Enviar archivos a la API
                if (archivosGenerados.Count > 0)
                {
                    await EnviarArchivosAPI(archivosGenerados);
                }

                progressBar.Value = 100;
                progressBar.Visible = false;
                btnGuardar.Enabled = true;
                btnLimpiar.Enabled = true;

                string mensaje = $"Migración completada exitosamente!\n\n" +
                               $"Archivos generados: {archivosGenerados.Count}\n" +
                               $"Directorio: {directorioArchivos}\n" +
                               $"Años procesados: {string.Join(", ", aniosAProcesar.OrderByDescending(x => x))}\n" +
                               $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}";

                MessageBox.Show(mensaje, "Proceso Completado",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                progressBar.Visible = false;
                btnGuardar.Enabled = true;
                btnLimpiar.Enabled = true;

                MessageBox.Show($"Error durante la migración:\n\n{ex.Message}",
                    "Error de Proceso", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<string> ProcesarAnio(int anio, string directorioDestino)
        {
            List<TransaccionData> transaccionesFront = new List<TransaccionData>();
            List<TransaccionData> transaccionesBack = new List<TransaccionData>();

            // Consultar BD Front si hay datos de conexión
            if (!string.IsNullOrEmpty(UsuarioFront) && !string.IsNullOrEmpty(IpFront))
            {
                transaccionesFront = await ConsultarBaseFront(anio);
            }

            // Consultar BD Back si hay datos de conexión
            if (!string.IsNullOrEmpty(UsuarioBack) && !string.IsNullOrEmpty(IpBack))
            {
                transaccionesBack = await ConsultarBaseBack(anio);
            }

            // Si no hay transacciones, retornar null
            if (transaccionesFront.Count == 0 && transaccionesBack.Count == 0)
            {
                return null;
            }

            // Generar archivo para el año
            string nombreArchivo = $"{NIT}_{anio}.txt";
            string rutaCompleta = Path.Combine(directorioDestino, nombreArchivo);

            return GenerarArchivoTransacciones(rutaCompleta, anio, transaccionesFront, transaccionesBack);
        }

        private async Task<List<TransaccionData>> ConsultarBaseFront(int anio)
        {
            List<TransaccionData> transacciones = new List<TransaccionData>();

            try
            {
                string connectionString = $"Server={IpFront};Database=FrontDB;User Id={UsuarioFront};Password={PasswordFront};Connection Timeout=30;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string query = @"
                        SELECT Modulo, Mensaje_numero_cd, ParametrosAdicionales_Xml, Fechahoragrabacion 
                        FROM Ho_facturaelectronica_transaccion 
                        WHERE Operacion = 'insert' 
                        AND Estado = 'ok' 
                        AND YEAR(Fechahoragrabacion) = @anio
                        ORDER BY Fechahoragrabacion";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@anio", anio);
                        command.CommandTimeout = 300; // 5 minutos timeout

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                transacciones.Add(new TransaccionData
                                {
                                    Modulo = reader["Modulo"]?.ToString() ?? "",
                                    Mensaje = reader["Mensaje_numero_cd"]?.ToString() ?? "",
                                    Parametros = reader["ParametrosAdicionales_Xml"]?.ToString() ?? "",
                                    FechaHora = Convert.ToDateTime(reader["Fechahoragrabacion"]),
                                    Origen = "FRONT"
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error consultando BD Front: {ex.Message}");
            }

            return transacciones;
        }

        private async Task<List<TransaccionData>> ConsultarBaseBack(int anio)
        {
            List<TransaccionData> transacciones = new List<TransaccionData>();

            try
            {
                string connectionString = $"Server={IpBack};Database=BackDB;User Id={UsuarioBack};Password={PasswordBack};Connection Timeout=30;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string query = @"
                        SELECT Modulo, Mensaje_respuesta_ds, ParametrosAdiconles, FechaHoraGrabacion 
                        FROM FacturaElectronica_Transaccion 
                        WHERE Operacion = 'insert' 
                        AND Estado = 'ok' 
                        AND YEAR(FechaHoraGrabacion) = @anio
                        ORDER BY FechaHoraGrabacion";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@anio", anio);
                        command.CommandTimeout = 300; // 5 minutos timeout

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                transacciones.Add(new TransaccionData
                                {
                                    Modulo = reader["Modulo"]?.ToString() ?? "",
                                    Mensaje = reader["Mensaje_respuesta_ds"]?.ToString() ?? "",
                                    Parametros = reader["ParametrosAdiconles"]?.ToString() ?? "",
                                    FechaHora = Convert.ToDateTime(reader["FechaHoraGrabacion"]),
                                    Origen = "BACK"
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error consultando BD Back: {ex.Message}");
            }

            return transacciones;
        }

        private string GenerarArchivoTransacciones(string rutaArchivo, int anio,
            List<TransaccionData> transaccionesFront, List<TransaccionData> transaccionesBack)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(rutaArchivo, false, Encoding.UTF8))
                {
                    // Encabezado del archivo
                    writer.WriteLine("=".PadRight(80, '='));
                    writer.WriteLine($"MIGRACION MASIVA DE DATOS - AÑO {anio}");
                    writer.WriteLine($"NIT: {NIT}");
                    writer.WriteLine($"FECHA GENERACION: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                    writer.WriteLine($"TOTAL TRANSACCIONES FRONT: {transaccionesFront.Count}");
                    writer.WriteLine($"TOTAL TRANSACCIONES BACK: {transaccionesBack.Count}");
                    writer.WriteLine("=".PadRight(80, '='));
                    writer.WriteLine();

                    // Escribir transacciones FRONT
                    if (transaccionesFront.Count > 0)
                    {
                        writer.WriteLine("[TRANSACCIONES FRONT]");
                        writer.WriteLine("-".PadRight(50, '-'));

                        foreach (var transaccion in transaccionesFront)
                        {
                            writer.WriteLine($"MODULO: {transaccion.Modulo}");
                            writer.WriteLine($"MENSAJE: {transaccion.Mensaje}");
                            writer.WriteLine($"PARAMETROS: {transaccion.Parametros}");
                            writer.WriteLine($"FECHA: {transaccion.FechaHora:dd/MM/yyyy HH:mm:ss}");
                            writer.WriteLine($"ORIGEN: {transaccion.Origen}");
                            writer.WriteLine();
                        }
                        writer.WriteLine();
                    }

                    // Escribir transacciones BACK
                    if (transaccionesBack.Count > 0)
                    {
                        writer.WriteLine("[TRANSACCIONES BACK]");
                        writer.WriteLine("-".PadRight(50, '-'));

                        foreach (var transaccion in transaccionesBack)
                        {
                            writer.WriteLine($"MODULO: {transaccion.Modulo}");
                            writer.WriteLine($"MENSAJE: {transaccion.Mensaje}");
                            writer.WriteLine($"PARAMETROS: {transaccion.Parametros}");
                            writer.WriteLine($"FECHA: {transaccion.FechaHora:dd/MM/yyyy HH:mm:ss}");
                            writer.WriteLine($"ORIGEN: {transaccion.Origen}");
                            writer.WriteLine();
                        }
                    }

                    // Pie de archivo
                    writer.WriteLine("=".PadRight(80, '='));
                    writer.WriteLine($"FIN DEL ARCHIVO - TOTAL REGISTROS: {transaccionesFront.Count + transaccionesBack.Count}");
                    writer.WriteLine("=".PadRight(80, '='));
                }

                return rutaArchivo;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generando archivo {rutaArchivo}: {ex.Message}");
            }
        }

        private async Task EnviarArchivosAPI(List<string> archivos)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(10); // Timeout de 10 minutos

                    foreach (string archivo in archivos)
                    {
                        await EnviarArchivoIndividual(client, archivo);

                        // Pausa pequeña entre envíos
                        await Task.Delay(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error enviando archivos a la API: {ex.Message}");
            }
        }

        private async Task EnviarArchivoIndividual(HttpClient client, string rutaArchivo)
        {
            try
            {
                // Leer contenido del archivo
                byte[] fileContent = File.ReadAllBytes(rutaArchivo);
                string fileName = Path.GetFileName(rutaArchivo);

                // Crear contenido multipart/form-data
                using (var formData = new MultipartFormDataContent())
                {
                    // Agregar NIT y clave (usar password de Zeus como clave de API)
                    formData.Add(new StringContent(NIT), "nit");
                    formData.Add(new StringContent(PasswordZeus), "clave");

                    // Agregar archivo
                    var fileContent_multipart = new ByteArrayContent(fileContent);
                    fileContent_multipart.Headers.ContentType =
                        System.Net.Http.Headers.MediaTypeHeaderValue.Parse("text/plain");
                    formData.Add(fileContent_multipart, "archivo", fileName);

                    // Configurar URL de la API (cambiar por la URL real)
                    string apiUrl = "https://api.ejemplo.com/migracion/upload";

                    // Enviar request
                    HttpResponseMessage response = await client.PostAsync(apiUrl, formData);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        // Log exitoso
                        string logPath = Path.Combine(Path.GetDirectoryName(rutaArchivo),
                            $"LOG_API_{Path.GetFileNameWithoutExtension(rutaArchivo)}.txt");
                        File.WriteAllText(logPath,
                            $"ENVIO EXITOSO - {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n" +
                            $"Archivo: {fileName}\n" +
                            $"Respuesta: {responseContent}");
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        throw new Exception($"Error HTTP {response.StatusCode}: {errorContent}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log de error
                string errorLogPath = Path.Combine(Path.GetDirectoryName(rutaArchivo),
                    $"ERROR_API_{Path.GetFileNameWithoutExtension(rutaArchivo)}.txt");
                File.WriteAllText(errorLogPath,
                    $"ERROR ENVIO API - {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n" +
                    $"Archivo: {Path.GetFileName(rutaArchivo)}\n" +
                    $"Error: {ex.Message}");

                throw new Exception($"Error enviando {Path.GetFileName(rutaArchivo)}: {ex.Message}");
            }
        }

        // Métodos públicos para acceder a los valores desde fuera de la clase
        public void SetNIT(string nit) => txtNit.Text = nit;
        public void SetRutaDescarga(string ruta) => txtRutaDescarga.Text = ruta;
        public void SeleccionarAnio(int anio)
        {
            var checkbox = chkAnios.FirstOrDefault(chk => (int)chk.Tag == anio);
            if (checkbox != null)
                checkbox.Checked = true;
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            // Método generado por el diseñador - puede permanecer vacío
        }

        private void grpFront_Enter(object sender, EventArgs e)
        {
            // Método generado por el diseñador - puede permanecer vacío
        }

        private void chkTodos_CheckedChanged_1(object sender, EventArgs e)
        {
            // Método generado por el diseñador - puede permanecer vacío
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void txtIpFront_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtNit_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '.')
            {
                e.KeyChar = ' ';
            }

            // Permitir SOLO números, backspace y delete
            // NO permitir puntos (.), comas (,), espacios, ni otros caracteres
            if (!(e.KeyChar >= '0' && e.KeyChar <= '9') &&
                e.KeyChar != (char)Keys.Back &&
                e.KeyChar != (char)Keys.Delete

                )
            {
                e.Handled = true;
            }
        }
    }

    // Clase auxiliar para manejar datos de transacciones
    public class TransaccionData
    {
        public string Modulo { get; set; }
        public string Mensaje { get; set; }
        public string Parametros { get; set; }
        public DateTime FechaHora { get; set; }
        public string Origen { get; set; }
    }
}

