using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp1.Helpers
{
    public static class UIHelper
    {
        #region Efectos visuales

        public static void AplicarEfectoHover(Button boton, Color colorHover, Color colorNormal)
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

        public static void AplicarEfectoFocus(TextBox textBox)
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

        public static void ConfigurarEstiloCheckbox(CheckBox checkbox)
        {
            checkbox.FlatAppearance.BorderSize = 1;
            checkbox.FlatAppearance.BorderColor = Color.FromArgb(189, 195, 199);
            checkbox.FlatAppearance.CheckedBackColor = Color.FromArgb(52, 152, 219);
        }

        #endregion

        #region Colores del tema

        public static class Colores
        {
            public static readonly Color Primario = Color.FromArgb(52, 152, 219);
            public static readonly Color Secundario = Color.FromArgb(52, 73, 94);
            public static readonly Color Exitoso = Color.FromArgb(46, 204, 113);
            public static readonly Color ExitosoHover = Color.FromArgb(39, 174, 96);
            public static readonly Color Peligro = Color.FromArgb(231, 76, 60);
            public static readonly Color PeligroHover = Color.FromArgb(192, 57, 43);
            public static readonly Color Neutro = Color.FromArgb(236, 240, 241);
            public static readonly Color NeutroTexto = Color.FromArgb(52, 73, 94);
            public static readonly Color FocusInput = Color.FromArgb(174, 214, 241);
            public static readonly Color Violeta = Color.FromArgb(155, 89, 182);
            public static readonly Color VioletaHover = Color.FromArgb(142, 68, 173);
        }

        #endregion

        #region Configuración de controles

        public static void ConfigurarBotonPrimario(Button boton)
        {
            AplicarEfectoHover(boton, Colores.ExitosoHover, Colores.Exitoso);
        }

        public static void ConfigurarBotonSecundario(Button boton)
        {
            AplicarEfectoHover(boton, Colores.VioletaHover, Colores.Violeta);
        }

        public static void ConfigurarBotonPeligro(Button boton)
        {
            AplicarEfectoHover(boton, Colores.PeligroHover, Colores.Peligro);
        }

        public static void ConfigurarBotonVerificacion(Button boton)
        {
            AplicarEfectoHover(boton, Color.FromArgb(74, 144, 226), Colores.Primario);
        }

        public static void HabilitarControles(Control[] controles, bool habilitar)
        {
            foreach (var control in controles)
            {
                control.Enabled = habilitar;
            }
        }

        public static void LimpiarTextBoxes(params TextBox[] textBoxes)
        {
            foreach (var textBox in textBoxes)
            {
                textBox.Clear();
            }
        }

        public static void DesmarcarCheckBoxes(params CheckBox[] checkBoxes)
        {
            foreach (var checkBox in checkBoxes)
            {
                checkBox.Checked = false;
            }
        }

        #endregion

        #region Validación de entrada

        public static void ConfigurarSoloNumeros(TextBox textBox, bool permitirPuntos = false)
        {
            textBox.KeyPress += (sender, e) =>
            {
                // Convertir punto en espacio si no se permiten puntos
                if (!permitirPuntos && e.KeyChar == '.')
                {
                    e.KeyChar = ' ';
                }

                // Permitir solo números, backspace y delete
                if (!(char.IsDigit(e.KeyChar) ||
                      e.KeyChar == (char)Keys.Back ||
                      e.KeyChar == (char)Keys.Delete ||
                      (permitirPuntos && e.KeyChar == '.')))
                {
                    e.Handled = true;
                }
            };
        }

        #endregion

        #region Estados visuales de controles

        public static void EstablecerEstadoTotalRegistros(TextBox txtTotalRegistros, string texto, EstadoRegistro estado)
        {
            txtTotalRegistros.Text = texto;

            switch (estado)
            {
                case EstadoRegistro.Todos:
                    txtTotalRegistros.BackColor = Colores.Exitoso;
                    txtTotalRegistros.ForeColor = Color.White;
                    break;
                case EstadoRegistro.Algunos:
                    txtTotalRegistros.BackColor = Colores.Primario;
                    txtTotalRegistros.ForeColor = Color.White;
                    break;
                case EstadoRegistro.Ninguno:
                default:
                    txtTotalRegistros.BackColor = Colores.Neutro;
                    txtTotalRegistros.ForeColor = Colores.NeutroTexto;
                    break;
            }
        }

        public static void EstablecerEstadoCheckboxTodos(CheckBox chkTodos, bool seleccionado)
        {
            if (seleccionado)
            {
                chkTodos.BackColor = Colores.Exitoso;
                chkTodos.ForeColor = Color.White;
            }
            else
            {
                chkTodos.BackColor = Colores.Primario;
                chkTodos.ForeColor = Color.White;
            }
        }

        public static void EstablecerEstadoCheckboxAnio(CheckBox checkbox, bool seleccionado)
        {
            checkbox.ForeColor = seleccionado ? Color.White : Colores.NeutroTexto;
        }

        #endregion

        #region Mensajes

        public static bool ConfirmarAccion(string mensaje, string titulo = "Confirmar")
        {
            var resultado = MessageBox.Show(mensaje, titulo,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return resultado == DialogResult.Yes;
        }

        public static void MostrarError(string mensaje, string titulo = "Error")
        {
            MessageBox.Show(mensaje, titulo,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void MostrarInformacion(string mensaje, string titulo = "Información")
        {
            MessageBox.Show(mensaje, titulo,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void MostrarAdvertencia(string mensaje, string titulo = "Advertencia")
        {
            MessageBox.Show(mensaje, titulo,
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        #endregion
    }

    public enum EstadoRegistro
    {
        Ninguno,
        Algunos,
        Todos
    }
}