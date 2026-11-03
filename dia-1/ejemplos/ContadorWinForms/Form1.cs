using System;
using System.Drawing;
using System.Windows.Forms;

namespace ContadorWinForms
{
    // El mismo código que la diapositiva D11.
    // Los controles se crean en código para no depender del diseñador.
    public class Form1 : Form
    {
        private int contador = 0;

        private readonly Label lblContador;
        private readonly Button btnSumar;

        public Form1()
        {
            Text = "Contador · WinForms";
            ClientSize = new Size(360, 200);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 12);

            lblContador = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 36, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(40, 30)
            };

            btnSumar = new Button
            {
                Text = "Sumar",
                Size = new Size(140, 44),
                Location = new Point(40, 120)
            };
            btnSumar.Click += btnSumar_Click;

            Controls.Add(lblContador);
            Controls.Add(btnSumar);
        }

        private void btnSumar_Click(object sender, EventArgs e)
        {
            contador++;
            lblContador.Text = contador.ToString();   // 1, 2, 3...
        }
    }
}
