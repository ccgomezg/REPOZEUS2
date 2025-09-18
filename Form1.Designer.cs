using System.Drawing;
using System.Windows.Forms;
using WindowsFormsApp1.Helpers;

namespace WindowsFormsApp1
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        // Controles del formulario
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Panel panelPrincipal;
        private System.Windows.Forms.TableLayoutPanel tableLayoutMain;

        private System.Windows.Forms.Label lblNit;
        private System.Windows.Forms.TextBox txtNit;

        // Conexión BD Back
        private System.Windows.Forms.GroupBox grpBack;
        private System.Windows.Forms.TableLayoutPanel tableBack;
        private System.Windows.Forms.Label lblUsuarioBack;
        private System.Windows.Forms.TextBox txtUsuarioBack;
        private System.Windows.Forms.Label lblPasswordBack;
        private System.Windows.Forms.TextBox txtPasswordBack;
        private System.Windows.Forms.Label lblIpBack;
        private System.Windows.Forms.TextBox txtIpBack;
        private System.Windows.Forms.Label lblBaseDatosBack;
        private System.Windows.Forms.TextBox txtBaseDatosBack;

        // Ruta Descarga
        private System.Windows.Forms.GroupBox grpRuta;
        private System.Windows.Forms.TableLayoutPanel tableRuta;
        private System.Windows.Forms.TextBox txtRutaDescarga;
        private System.Windows.Forms.Button btnBuscarRuta;

        // Lapso
        private System.Windows.Forms.GroupBox grpLapso;
        private System.Windows.Forms.CheckBox chkTodos;
        private System.Windows.Forms.FlowLayoutPanel flowPanelAnios;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitulo = new System.Windows.Forms.Label();
            this.panelPrincipal = new System.Windows.Forms.Panel();
            this.tableLayoutMain = new System.Windows.Forms.TableLayoutPanel();
            this.tableNit = new System.Windows.Forms.TableLayoutPanel();
            this.lblNit = new System.Windows.Forms.Label();
            this.txtNit = new System.Windows.Forms.TextBox();
            this.grpFront = new System.Windows.Forms.GroupBox();
            this.tableFront = new System.Windows.Forms.TableLayoutPanel();
            this.lblUsuarioFront = new System.Windows.Forms.Label();
            this.lblPasswordFront = new System.Windows.Forms.Label();
            this.txtUsuarioFront = new System.Windows.Forms.TextBox();
            this.txtPasswordFront = new System.Windows.Forms.TextBox();
            this.lblIpFront = new System.Windows.Forms.Label();
            this.lblBaseDatosFront = new System.Windows.Forms.Label();
            this.txtBaseDatosFront = new System.Windows.Forms.TextBox();
            this.txtIpFront = new System.Windows.Forms.TextBox();
            this.grpBack = new System.Windows.Forms.GroupBox();
            this.tableBack = new System.Windows.Forms.TableLayoutPanel();
            this.lblUsuarioBack = new System.Windows.Forms.Label();
            this.lblIpBack = new System.Windows.Forms.Label();
            this.txtIpBack = new System.Windows.Forms.TextBox();
            this.txtUsuarioBack = new System.Windows.Forms.TextBox();
            this.txtPasswordBack = new System.Windows.Forms.TextBox();
            this.lblPasswordBack = new System.Windows.Forms.Label();
            this.lblBaseDatosBack = new System.Windows.Forms.Label();
            this.txtBaseDatosBack = new System.Windows.Forms.TextBox();
            this.grpRuta = new System.Windows.Forms.GroupBox();
            this.tableRuta = new System.Windows.Forms.TableLayoutPanel();
            this.btnBuscarRuta = new System.Windows.Forms.Button();
            this.txtRutaDescarga = new System.Windows.Forms.TextBox();
            this.grpRdbtn = new System.Windows.Forms.GroupBox();
            this.tablegrpRdbtn = new System.Windows.Forms.TableLayoutPanel();
            this.rbMigrarAmbos = new System.Windows.Forms.RadioButton();
            this.rbMigrarBack = new System.Windows.Forms.RadioButton();
            this.rbMigrarFront = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxSp = new System.Windows.Forms.CheckBox();
            this.grpLapso = new System.Windows.Forms.GroupBox();
            this.chkTodos = new System.Windows.Forms.CheckBox();
            this.flowPanelAnios = new System.Windows.Forms.FlowLayoutPanel();
            this.lblTotalRegistros = new System.Windows.Forms.Label();
            this.txtTotalRegistros = new System.Windows.Forms.TextBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.verificar_conexion_btn = new System.Windows.Forms.TableLayoutPanel();
            this.btnverificar = new System.Windows.Forms.Button();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnLimpiar = new System.Windows.Forms.Button();
            this.txtsummarymigrate = new System.Windows.Forms.TextBox();
            this.panelPrincipal.SuspendLayout();
            this.tableLayoutMain.SuspendLayout();
            this.tableNit.SuspendLayout();
            this.grpFront.SuspendLayout();
            this.tableFront.SuspendLayout();
            this.grpBack.SuspendLayout();
            this.tableBack.SuspendLayout();
            this.grpRuta.SuspendLayout();
            this.tableRuta.SuspendLayout();
            this.grpRdbtn.SuspendLayout();
            this.tablegrpRdbtn.SuspendLayout();
            this.grpLapso.SuspendLayout();
            this.verificar_conexion_btn.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitulo
            // 
            this.lblTitulo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(128)))), ((int)(((byte)(185)))));
            this.lblTitulo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(0, 0);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(872, 52);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "🔄 MIGRACIÓN MASIVA DE DATOS";
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelPrincipal
            // 
            this.panelPrincipal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.panelPrincipal.Controls.Add(this.tableLayoutMain);
            this.panelPrincipal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPrincipal.Location = new System.Drawing.Point(0, 52);
            this.panelPrincipal.Name = "panelPrincipal";
            this.panelPrincipal.Padding = new System.Windows.Forms.Padding(17);
            this.panelPrincipal.Size = new System.Drawing.Size(872, 556);
            this.panelPrincipal.TabIndex = 1;
            // 
            // tableLayoutMain
            // 
            this.tableLayoutMain.ColumnCount = 2;
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutMain.Controls.Add(this.tableNit, 0, 0);
            this.tableLayoutMain.Controls.Add(this.grpFront, 1, 1);
            this.tableLayoutMain.Controls.Add(this.grpBack, 0, 1);
            this.tableLayoutMain.Controls.Add(this.grpRuta, 0, 2);
            this.tableLayoutMain.Controls.Add(this.grpRdbtn, 1, 2);
            this.tableLayoutMain.Controls.Add(this.grpLapso, 0, 3);
            this.tableLayoutMain.Controls.Add(this.progressBar, 0, 5);
            this.tableLayoutMain.Controls.Add(this.verificar_conexion_btn, 0, 6);
            this.tableLayoutMain.Controls.Add(this.txtsummarymigrate, 0, 4);
            this.tableLayoutMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutMain.Location = new System.Drawing.Point(17, 17);
            this.tableLayoutMain.Name = "tableLayoutMain";
            this.tableLayoutMain.RowCount = 7;
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 135F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 123F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 103F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutMain.Size = new System.Drawing.Size(838, 522);
            this.tableLayoutMain.TabIndex = 0;
            // 
            // tableNit
            // 
            this.tableNit.ColumnCount = 2;
            this.tableLayoutMain.SetColumnSpan(this.tableNit, 2);
            this.tableNit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.16346F));
            this.tableNit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 72.83654F));
            this.tableNit.Controls.Add(this.lblNit, 0, 0);
            this.tableNit.Controls.Add(this.txtNit, 1, 0);
            this.tableNit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableNit.Location = new System.Drawing.Point(3, 3);
            this.tableNit.Name = "tableNit";
            this.tableNit.RowCount = 1;
            this.tableNit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableNit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableNit.Size = new System.Drawing.Size(832, 33);
            this.tableNit.TabIndex = 0;
            // 
            // lblNit
            // 
            this.lblNit.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblNit.AutoSize = true;
            this.lblNit.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblNit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblNit.Location = new System.Drawing.Point(4, 8);
            this.lblNit.Name = "lblNit";
            this.lblNit.Size = new System.Drawing.Size(217, 19);
            this.lblNit.TabIndex = 0;
            this.lblNit.Text = "NIT (Sin dígito de verificación):";
            this.lblNit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtNit
            // 
            this.txtNit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txtNit.BackColor = System.Drawing.Color.White;
            this.txtNit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtNit.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtNit.Location = new System.Drawing.Point(228, 5);
            this.txtNit.Name = "txtNit";
            this.txtNit.Size = new System.Drawing.Size(224, 25);
            this.txtNit.TabIndex = 1;
            // 
            // grpFront
            // 
            this.grpFront.BackColor = System.Drawing.Color.White;
            this.grpFront.Controls.Add(this.tableFront);
            this.grpFront.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpFront.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpFront.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.grpFront.Location = new System.Drawing.Point(426, 42);
            this.grpFront.Margin = new System.Windows.Forms.Padding(7, 3, 3, 3);
            this.grpFront.Name = "grpFront";
            this.grpFront.Padding = new System.Windows.Forms.Padding(9);
            this.grpFront.Size = new System.Drawing.Size(409, 129);
            this.grpFront.TabIndex = 3;
            this.grpFront.TabStop = false;
            this.grpFront.Text = "🗄️ BASE DATOS FRONT";
            // 
            // tableFront
            // 
            this.tableFront.ColumnCount = 2;
            this.tableFront.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableFront.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableFront.Controls.Add(this.lblUsuarioFront, 0, 0);
            this.tableFront.Controls.Add(this.lblPasswordFront, 1, 0);
            this.tableFront.Controls.Add(this.txtUsuarioFront, 0, 1);
            this.tableFront.Controls.Add(this.txtPasswordFront, 1, 1);
            this.tableFront.Controls.Add(this.lblIpFront, 0, 2);
            this.tableFront.Controls.Add(this.lblBaseDatosFront, 1, 2);
            this.tableFront.Controls.Add(this.txtBaseDatosFront, 1, 3);
            this.tableFront.Controls.Add(this.txtIpFront, 0, 3);
            this.tableFront.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableFront.Location = new System.Drawing.Point(9, 27);
            this.tableFront.Name = "tableFront";
            this.tableFront.RowCount = 4;
            this.tableFront.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 14F));
            this.tableFront.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableFront.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableFront.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableFront.Size = new System.Drawing.Size(391, 93);
            this.tableFront.TabIndex = 0;
            // 
            // lblUsuarioFront
            // 
            this.lblUsuarioFront.AutoSize = true;
            this.lblUsuarioFront.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblUsuarioFront.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblUsuarioFront.Location = new System.Drawing.Point(3, 0);
            this.lblUsuarioFront.Name = "lblUsuarioFront";
            this.lblUsuarioFront.Size = new System.Drawing.Size(50, 14);
            this.lblUsuarioFront.TabIndex = 0;
            this.lblUsuarioFront.Text = "Usuario:";
            // 
            // lblPasswordFront
            // 
            this.lblPasswordFront.AutoSize = true;
            this.lblPasswordFront.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPasswordFront.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblPasswordFront.Location = new System.Drawing.Point(198, 0);
            this.lblPasswordFront.Name = "lblPasswordFront";
            this.lblPasswordFront.Size = new System.Drawing.Size(70, 14);
            this.lblPasswordFront.TabIndex = 2;
            this.lblPasswordFront.Text = "Contraseña:";
            // 
            // txtUsuarioFront
            // 
            this.txtUsuarioFront.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtUsuarioFront.BackColor = System.Drawing.Color.White;
            this.txtUsuarioFront.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtUsuarioFront.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtUsuarioFront.Location = new System.Drawing.Point(3, 20);
            this.txtUsuarioFront.Margin = new System.Windows.Forms.Padding(3, 3, 7, 3);
            this.txtUsuarioFront.Name = "txtUsuarioFront";
            this.txtUsuarioFront.Size = new System.Drawing.Size(185, 23);
            this.txtUsuarioFront.TabIndex = 1;
            // 
            // txtPasswordFront
            // 
            this.txtPasswordFront.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPasswordFront.BackColor = System.Drawing.Color.White;
            this.txtPasswordFront.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPasswordFront.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtPasswordFront.Location = new System.Drawing.Point(198, 20);
            this.txtPasswordFront.Name = "txtPasswordFront";
            this.txtPasswordFront.Size = new System.Drawing.Size(190, 23);
            this.txtPasswordFront.TabIndex = 3;
            this.txtPasswordFront.UseSystemPasswordChar = true;
            // 
            // lblIpFront
            // 
            this.lblIpFront.AutoSize = true;
            this.lblIpFront.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblIpFront.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblIpFront.Location = new System.Drawing.Point(3, 50);
            this.lblIpFront.Name = "lblIpFront";
            this.lblIpFront.Size = new System.Drawing.Size(66, 15);
            this.lblIpFront.TabIndex = 4;
            this.lblIpFront.Text = "IP Servidor:";
            // 
            // lblBaseDatosFront
            // 
            this.lblBaseDatosFront.AutoSize = true;
            this.lblBaseDatosFront.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblBaseDatosFront.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblBaseDatosFront.Location = new System.Drawing.Point(198, 50);
            this.lblBaseDatosFront.Name = "lblBaseDatosFront";
            this.lblBaseDatosFront.Size = new System.Drawing.Size(114, 15);
            this.lblBaseDatosFront.TabIndex = 6;
            this.lblBaseDatosFront.Text = "Nombre Base Datos:";
            // 
            // txtBaseDatosFront
            // 
            this.txtBaseDatosFront.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBaseDatosFront.BackColor = System.Drawing.Color.White;
            this.txtBaseDatosFront.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtBaseDatosFront.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtBaseDatosFront.Location = new System.Drawing.Point(198, 69);
            this.txtBaseDatosFront.Name = "txtBaseDatosFront";
            this.txtBaseDatosFront.Size = new System.Drawing.Size(190, 23);
            this.txtBaseDatosFront.TabIndex = 7;
            // 
            // txtIpFront
            // 
            this.txtIpFront.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtIpFront.BackColor = System.Drawing.Color.White;
            this.txtIpFront.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtIpFront.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtIpFront.Location = new System.Drawing.Point(3, 69);
            this.txtIpFront.Name = "txtIpFront";
            this.txtIpFront.Size = new System.Drawing.Size(189, 23);
            this.txtIpFront.TabIndex = 5;
            // 
            // grpBack
            // 
            this.grpBack.BackColor = System.Drawing.Color.White;
            this.grpBack.Controls.Add(this.tableBack);
            this.grpBack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpBack.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpBack.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.grpBack.Location = new System.Drawing.Point(3, 42);
            this.grpBack.Margin = new System.Windows.Forms.Padding(3, 3, 7, 3);
            this.grpBack.Name = "grpBack";
            this.grpBack.Padding = new System.Windows.Forms.Padding(9);
            this.grpBack.Size = new System.Drawing.Size(409, 129);
            this.grpBack.TabIndex = 2;
            this.grpBack.TabStop = false;
            this.grpBack.Text = "🗃️ BASE DATOS BACK";
            // 
            // tableBack
            // 
            this.tableBack.ColumnCount = 2;
            this.tableBack.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableBack.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableBack.Controls.Add(this.lblUsuarioBack, 0, 0);
            this.tableBack.Controls.Add(this.lblIpBack, 0, 2);
            this.tableBack.Controls.Add(this.txtIpBack, 0, 3);
            this.tableBack.Controls.Add(this.txtUsuarioBack, 0, 1);
            this.tableBack.Controls.Add(this.txtPasswordBack, 1, 1);
            this.tableBack.Controls.Add(this.lblPasswordBack, 1, 0);
            this.tableBack.Controls.Add(this.lblBaseDatosBack, 1, 2);
            this.tableBack.Controls.Add(this.txtBaseDatosBack, 1, 3);
            this.tableBack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableBack.Location = new System.Drawing.Point(9, 27);
            this.tableBack.Name = "tableBack";
            this.tableBack.RowCount = 4;
            this.tableBack.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 13F));
            this.tableBack.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableBack.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 19F));
            this.tableBack.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 13F));
            this.tableBack.Size = new System.Drawing.Size(391, 93);
            this.tableBack.TabIndex = 0;
            // 
            // lblUsuarioBack
            // 
            this.lblUsuarioBack.AutoSize = true;
            this.lblUsuarioBack.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblUsuarioBack.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblUsuarioBack.Location = new System.Drawing.Point(3, 0);
            this.lblUsuarioBack.Name = "lblUsuarioBack";
            this.lblUsuarioBack.Size = new System.Drawing.Size(50, 13);
            this.lblUsuarioBack.TabIndex = 0;
            this.lblUsuarioBack.Text = "Usuario:";
            // 
            // lblIpBack
            // 
            this.lblIpBack.AutoSize = true;
            this.lblIpBack.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblIpBack.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblIpBack.Location = new System.Drawing.Point(3, 48);
            this.lblIpBack.Name = "lblIpBack";
            this.lblIpBack.Size = new System.Drawing.Size(66, 15);
            this.lblIpBack.TabIndex = 4;
            this.lblIpBack.Text = "IP Servidor:";
            // 
            // txtIpBack
            // 
            this.txtIpBack.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtIpBack.BackColor = System.Drawing.Color.White;
            this.txtIpBack.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtIpBack.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtIpBack.Location = new System.Drawing.Point(3, 70);
            this.txtIpBack.Name = "txtIpBack";
            this.txtIpBack.Size = new System.Drawing.Size(189, 23);
            this.txtIpBack.TabIndex = 5;
            // 
            // txtUsuarioBack
            // 
            this.txtUsuarioBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtUsuarioBack.BackColor = System.Drawing.Color.White;
            this.txtUsuarioBack.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtUsuarioBack.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtUsuarioBack.Location = new System.Drawing.Point(3, 19);
            this.txtUsuarioBack.Margin = new System.Windows.Forms.Padding(3, 3, 7, 3);
            this.txtUsuarioBack.Name = "txtUsuarioBack";
            this.txtUsuarioBack.Size = new System.Drawing.Size(185, 23);
            this.txtUsuarioBack.TabIndex = 1;
            // 
            // txtPasswordBack
            // 
            this.txtPasswordBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPasswordBack.BackColor = System.Drawing.Color.White;
            this.txtPasswordBack.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPasswordBack.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtPasswordBack.Location = new System.Drawing.Point(198, 19);
            this.txtPasswordBack.Name = "txtPasswordBack";
            this.txtPasswordBack.Size = new System.Drawing.Size(190, 23);
            this.txtPasswordBack.TabIndex = 3;
            this.txtPasswordBack.UseSystemPasswordChar = true;
            // 
            // lblPasswordBack
            // 
            this.lblPasswordBack.AutoSize = true;
            this.lblPasswordBack.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPasswordBack.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblPasswordBack.Location = new System.Drawing.Point(198, 0);
            this.lblPasswordBack.Name = "lblPasswordBack";
            this.lblPasswordBack.Size = new System.Drawing.Size(70, 13);
            this.lblPasswordBack.TabIndex = 2;
            this.lblPasswordBack.Text = "Contraseña:";
            // 
            // lblBaseDatosBack
            // 
            this.lblBaseDatosBack.AutoSize = true;
            this.lblBaseDatosBack.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblBaseDatosBack.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblBaseDatosBack.Location = new System.Drawing.Point(198, 48);
            this.lblBaseDatosBack.Name = "lblBaseDatosBack";
            this.lblBaseDatosBack.Size = new System.Drawing.Size(114, 15);
            this.lblBaseDatosBack.TabIndex = 6;
            this.lblBaseDatosBack.Text = "Nombre Base Datos:";
            // 
            // txtBaseDatosBack
            // 
            this.txtBaseDatosBack.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBaseDatosBack.BackColor = System.Drawing.Color.White;
            this.txtBaseDatosBack.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtBaseDatosBack.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtBaseDatosBack.Location = new System.Drawing.Point(198, 70);
            this.txtBaseDatosBack.Name = "txtBaseDatosBack";
            this.txtBaseDatosBack.Size = new System.Drawing.Size(190, 23);
            this.txtBaseDatosBack.TabIndex = 7;
            // 
            // grpRuta
            // 
            this.grpRuta.BackColor = System.Drawing.Color.White;
            this.grpRuta.Controls.Add(this.tableRuta);
            this.grpRuta.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpRuta.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpRuta.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.grpRuta.Location = new System.Drawing.Point(3, 177);
            this.grpRuta.Name = "grpRuta";
            this.grpRuta.Padding = new System.Windows.Forms.Padding(9);
            this.grpRuta.Size = new System.Drawing.Size(413, 117);
            this.grpRuta.TabIndex = 5;
            this.grpRuta.TabStop = false;
            this.grpRuta.Text = "📁 RUTA DE DESCARGA";
            // 
            // tableRuta
            // 
            this.tableRuta.ColumnCount = 2;
            this.tableRuta.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 91.13924F));
            this.tableRuta.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.86076F));
            this.tableRuta.Controls.Add(this.btnBuscarRuta, 1, 0);
            this.tableRuta.Controls.Add(this.txtRutaDescarga, 0, 0);
            this.tableRuta.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableRuta.Location = new System.Drawing.Point(9, 27);
            this.tableRuta.Name = "tableRuta";
            this.tableRuta.RowCount = 1;
            this.tableRuta.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableRuta.Size = new System.Drawing.Size(395, 81);
            this.tableRuta.TabIndex = 0;
            // 
            // btnBuscarRuta
            // 
            this.btnBuscarRuta.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnBuscarRuta.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnBuscarRuta.FlatAppearance.BorderSize = 0;
            this.btnBuscarRuta.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBuscarRuta.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnBuscarRuta.ForeColor = System.Drawing.Color.White;
            this.btnBuscarRuta.Location = new System.Drawing.Point(363, 30);
            this.btnBuscarRuta.Name = "btnBuscarRuta";
            this.btnBuscarRuta.Size = new System.Drawing.Size(29, 20);
            this.btnBuscarRuta.TabIndex = 1;
            this.btnBuscarRuta.Text = "...";
            this.btnBuscarRuta.UseVisualStyleBackColor = false;
            // 
            // txtRutaDescarga
            // 
            this.txtRutaDescarga.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRutaDescarga.BackColor = System.Drawing.Color.White;
            this.txtRutaDescarga.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRutaDescarga.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtRutaDescarga.Location = new System.Drawing.Point(3, 29);
            this.txtRutaDescarga.Margin = new System.Windows.Forms.Padding(3, 3, 4, 3);
            this.txtRutaDescarga.Name = "txtRutaDescarga";
            this.txtRutaDescarga.ReadOnly = true;
            this.txtRutaDescarga.Size = new System.Drawing.Size(353, 23);
            this.txtRutaDescarga.TabIndex = 0;
            // 
            // grpRdbtn
            // 
            this.grpRdbtn.BackColor = System.Drawing.Color.White;
            this.grpRdbtn.Controls.Add(this.tablegrpRdbtn);
            this.grpRdbtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpRdbtn.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpRdbtn.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.grpRdbtn.Location = new System.Drawing.Point(422, 177);
            this.grpRdbtn.Name = "grpRdbtn";
            this.grpRdbtn.Padding = new System.Windows.Forms.Padding(9);
            this.grpRdbtn.Size = new System.Drawing.Size(413, 117);
            this.grpRdbtn.TabIndex = 5;
            this.grpRdbtn.TabStop = false;
            this.grpRdbtn.Text = "📁 MiGRACION";
            // 
            // tablegrpRdbtn
            // 
            this.tablegrpRdbtn.ColumnCount = 4;
            this.tablegrpRdbtn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 116F));
            this.tablegrpRdbtn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 104F));
            this.tablegrpRdbtn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 106F));
            this.tablegrpRdbtn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 65F));
            this.tablegrpRdbtn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tablegrpRdbtn.Controls.Add(this.rbMigrarAmbos, 2, 0);
            this.tablegrpRdbtn.Controls.Add(this.rbMigrarBack, 0, 0);
            this.tablegrpRdbtn.Controls.Add(this.rbMigrarFront, 1, 0);
            this.tablegrpRdbtn.Controls.Add(this.label1, 0, 1);
            this.tablegrpRdbtn.Controls.Add(this.checkBoxSp, 1, 1);
            this.tablegrpRdbtn.Location = new System.Drawing.Point(10, 19);
            this.tablegrpRdbtn.Name = "tablegrpRdbtn";
            this.tablegrpRdbtn.RowCount = 2;
            this.tablegrpRdbtn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 51F));
            this.tablegrpRdbtn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tablegrpRdbtn.Size = new System.Drawing.Size(391, 89);
            this.tablegrpRdbtn.TabIndex = 0;
            // 
            // rbMigrarAmbos
            // 
            this.rbMigrarAmbos.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.rbMigrarAmbos.AutoSize = true;
            this.rbMigrarAmbos.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.rbMigrarAmbos.ForeColor = System.Drawing.Color.Black;
            this.rbMigrarAmbos.Location = new System.Drawing.Point(239, 16);
            this.rbMigrarAmbos.Name = "rbMigrarAmbos";
            this.rbMigrarAmbos.Size = new System.Drawing.Size(68, 19);
            this.rbMigrarAmbos.TabIndex = 5;
            this.rbMigrarAmbos.Text = "AMBOS";
            this.rbMigrarAmbos.UseVisualStyleBackColor = true;
            // 
            // rbMigrarBack
            // 
            this.rbMigrarBack.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.rbMigrarBack.AutoSize = true;
            this.rbMigrarBack.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.rbMigrarBack.ForeColor = System.Drawing.Color.Black;
            this.rbMigrarBack.Location = new System.Drawing.Point(20, 16);
            this.rbMigrarBack.Name = "rbMigrarBack";
            this.rbMigrarBack.Size = new System.Drawing.Size(76, 19);
            this.rbMigrarBack.TabIndex = 4;
            this.rbMigrarBack.Text = "BD BACK";
            this.rbMigrarBack.UseVisualStyleBackColor = true;
            // 
            // rbMigrarFront
            // 
            this.rbMigrarFront.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.rbMigrarFront.AutoSize = true;
            this.rbMigrarFront.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.rbMigrarFront.ForeColor = System.Drawing.Color.Black;
            this.rbMigrarFront.Location = new System.Drawing.Point(126, 16);
            this.rbMigrarFront.Name = "rbMigrarFront";
            this.rbMigrarFront.Size = new System.Drawing.Size(84, 19);
            this.rbMigrarFront.TabIndex = 3;
            this.rbMigrarFront.Text = "BD FRONT";
            this.rbMigrarFront.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AllowDrop = true;
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Location = new System.Drawing.Point(16, 60);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 19);
            this.label1.TabIndex = 6;
            this.label1.Text = "Config (Sp)";
            // 
            // checkBoxSp
            // 
            this.checkBoxSp.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.checkBoxSp.AutoSize = true;
            this.checkBoxSp.ForeColor = System.Drawing.Color.Black;
            this.checkBoxSp.Location = new System.Drawing.Point(160, 63);
            this.checkBoxSp.Name = "checkBoxSp";
            this.checkBoxSp.Size = new System.Drawing.Size(15, 14);
            this.checkBoxSp.TabIndex = 7;
            this.checkBoxSp.UseVisualStyleBackColor = true;
            // 
            // grpLapso
            // 
            this.grpLapso.BackColor = System.Drawing.Color.White;
            this.tableLayoutMain.SetColumnSpan(this.grpLapso, 2);
            this.grpLapso.Controls.Add(this.chkTodos);
            this.grpLapso.Controls.Add(this.flowPanelAnios);
            this.grpLapso.Controls.Add(this.lblTotalRegistros);
            this.grpLapso.Controls.Add(this.txtTotalRegistros);
            this.grpLapso.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpLapso.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpLapso.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.grpLapso.Location = new System.Drawing.Point(3, 300);
            this.grpLapso.Name = "grpLapso";
            this.grpLapso.Padding = new System.Windows.Forms.Padding(9);
            this.grpLapso.Size = new System.Drawing.Size(832, 97);
            this.grpLapso.TabIndex = 6;
            this.grpLapso.TabStop = false;
            this.grpLapso.Text = "📅 LAPSO DE TIEMPO";
            // 
            // chkTodos
            // 
            this.chkTodos.AutoSize = true;
            this.chkTodos.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.chkTodos.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.chkTodos.ForeColor = System.Drawing.Color.White;
            this.chkTodos.Location = new System.Drawing.Point(15, 64);
            this.chkTodos.Name = "chkTodos";
            this.chkTodos.Padding = new System.Windows.Forms.Padding(9, 4, 9, 4);
            this.chkTodos.Size = new System.Drawing.Size(98, 27);
            this.chkTodos.TabIndex = 0;
            this.chkTodos.Text = "✓ TODOS";
            this.chkTodos.UseVisualStyleBackColor = false;
            // 
            // flowPanelAnios
            // 
            this.flowPanelAnios.Location = new System.Drawing.Point(9, 20);
            this.flowPanelAnios.Name = "flowPanelAnios";
            this.flowPanelAnios.Size = new System.Drawing.Size(814, 38);
            this.flowPanelAnios.TabIndex = 1;
            // 
            // lblTotalRegistros
            // 
            this.lblTotalRegistros.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblTotalRegistros.AutoSize = true;
            this.lblTotalRegistros.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTotalRegistros.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.lblTotalRegistros.Location = new System.Drawing.Point(143, 70);
            this.lblTotalRegistros.Name = "lblTotalRegistros";
            this.lblTotalRegistros.Size = new System.Drawing.Size(178, 19);
            this.lblTotalRegistros.TabIndex = 7;
            this.lblTotalRegistros.Text = "📊 Total Registros Lapso:";
            // 
            // txtTotalRegistros
            // 
            this.txtTotalRegistros.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txtTotalRegistros.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.txtTotalRegistros.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTotalRegistros.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.txtTotalRegistros.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.txtTotalRegistros.Location = new System.Drawing.Point(323, 66);
            this.txtTotalRegistros.Name = "txtTotalRegistros";
            this.txtTotalRegistros.ReadOnly = true;
            this.txtTotalRegistros.Size = new System.Drawing.Size(129, 25);
            this.txtTotalRegistros.TabIndex = 8;
            this.txtTotalRegistros.Text = "0 registros";
            this.txtTotalRegistros.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // progressBar
            // 
            this.tableLayoutMain.SetColumnSpan(this.progressBar, 2);
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressBar.Location = new System.Drawing.Point(3, 441);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(832, 18);
            this.progressBar.TabIndex = 9;
            this.progressBar.Visible = false;
            // 
            // verificar_conexion_btn
            // 
            this.verificar_conexion_btn.ColumnCount = 6;
            this.tableLayoutMain.SetColumnSpan(this.verificar_conexion_btn, 2);
            this.verificar_conexion_btn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.verificar_conexion_btn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.verificar_conexion_btn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.verificar_conexion_btn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 135F));
            this.verificar_conexion_btn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 156F));
            this.verificar_conexion_btn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 169F));
            this.verificar_conexion_btn.Controls.Add(this.btnverificar, 0, 0);
            this.verificar_conexion_btn.Controls.Add(this.btnGuardar, 4, 0);
            this.verificar_conexion_btn.Controls.Add(this.btnLimpiar, 5, 0);
            this.verificar_conexion_btn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.verificar_conexion_btn.Location = new System.Drawing.Point(3, 465);
            this.verificar_conexion_btn.Name = "verificar_conexion_btn";
            this.verificar_conexion_btn.RowCount = 1;
            this.verificar_conexion_btn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.verificar_conexion_btn.Size = new System.Drawing.Size(832, 54);
            this.verificar_conexion_btn.TabIndex = 10;
            // 
            // btnverificar
            // 
            this.btnverificar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnverificar.BackColor = System.Drawing.Color.Gray;
            this.btnverificar.FlatAppearance.BorderSize = 0;
            this.btnverificar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnverificar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnverificar.ForeColor = System.Drawing.Color.White;
            this.btnverificar.Location = new System.Drawing.Point(3, 5);
            this.btnverificar.Name = "btnverificar";
            this.btnverificar.Size = new System.Drawing.Size(146, 44);
            this.btnverificar.TabIndex = 2;
            this.btnverificar.Text = "Verificar Conexión";
            this.btnverificar.UseVisualStyleBackColor = false;
            // 
            // btnGuardar
            // 
            this.btnGuardar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnGuardar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnGuardar.FlatAppearance.BorderSize = 0;
            this.btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnGuardar.ForeColor = System.Drawing.Color.White;
            this.btnGuardar.Location = new System.Drawing.Point(515, 3);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(140, 48);
            this.btnGuardar.TabIndex = 0;
            this.btnGuardar.Text = "🔄 MIGRAR";
            this.btnGuardar.UseVisualStyleBackColor = false;
            // 
            // btnLimpiar
            // 
            this.btnLimpiar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnLimpiar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnLimpiar.FlatAppearance.BorderSize = 0;
            this.btnLimpiar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLimpiar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnLimpiar.ForeColor = System.Drawing.Color.White;
            this.btnLimpiar.Location = new System.Drawing.Point(669, 3);
            this.btnLimpiar.Name = "btnLimpiar";
            this.btnLimpiar.Size = new System.Drawing.Size(157, 48);
            this.btnLimpiar.TabIndex = 1;
            this.btnLimpiar.Text = "🗑️ ELIMINAR DATA FORMULARIOS";
            this.btnLimpiar.UseVisualStyleBackColor = false;
            // 
            // txtsummarymigrate
            // 
            this.tableLayoutMain.SetColumnSpan(this.txtsummarymigrate, 2);
            this.txtsummarymigrate.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtsummarymigrate.Enabled = false;
            this.txtsummarymigrate.Location = new System.Drawing.Point(3, 403);
            this.txtsummarymigrate.Multiline = true;
            this.txtsummarymigrate.Name = "txtsummarymigrate";
            this.txtsummarymigrate.Size = new System.Drawing.Size(832, 32);
            this.txtsummarymigrate.TabIndex = 2;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(872, 608);
            this.Controls.Add(this.panelPrincipal);
            this.Controls.Add(this.lblTitulo);
            this.MinimumSize = new System.Drawing.Size(688, 525);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Migración Masiva v2.0 - Responsive";
            this.panelPrincipal.ResumeLayout(false);
            this.tableLayoutMain.ResumeLayout(false);
            this.tableLayoutMain.PerformLayout();
            this.tableNit.ResumeLayout(false);
            this.tableNit.PerformLayout();
            this.grpFront.ResumeLayout(false);
            this.tableFront.ResumeLayout(false);
            this.tableFront.PerformLayout();
            this.grpBack.ResumeLayout(false);
            this.tableBack.ResumeLayout(false);
            this.tableBack.PerformLayout();
            this.grpRuta.ResumeLayout(false);
            this.tableRuta.ResumeLayout(false);
            this.tableRuta.PerformLayout();
            this.grpRdbtn.ResumeLayout(false);
            this.tablegrpRdbtn.ResumeLayout(false);
            this.tablegrpRdbtn.PerformLayout();
            this.grpLapso.ResumeLayout(false);
            this.grpLapso.PerformLayout();
            this.verificar_conexion_btn.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private TableLayoutPanel tableNit;
        private GroupBox grpFront;
        private TableLayoutPanel tableFront;
        private Label lblUsuarioFront;
        private Label lblPasswordFront;
        private TextBox txtUsuarioFront;
        private TextBox txtPasswordFront;
        private Label lblIpFront;
        private Label lblBaseDatosFront;
        private TextBox txtBaseDatosFront;
        private TextBox txtIpFront;
        private GroupBox grpRdbtn;
        private TableLayoutPanel tablegrpRdbtn;
        private RadioButton rbMigrarAmbos;
        private RadioButton rbMigrarBack;
        private RadioButton rbMigrarFront;
        private Label label1;
        private CheckBox checkBoxSp;
        private Label lblTotalRegistros;
        private TextBox txtTotalRegistros;
        private TableLayoutPanel verificar_conexion_btn;
        private Button btnverificar;
        private Button btnGuardar;
        private Button btnLimpiar;
        private TextBox txtsummarymigrate;
        private ProgressBar progressBar;
    }
}