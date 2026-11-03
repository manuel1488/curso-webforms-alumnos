using System;
using System.Windows.Forms;

namespace ContadorWinForms
{
    public partial class Form1 : Form
    {
        private int contador = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnSumar_Click(object sender, EventArgs e)
        {
            contador++;
            lblContador.Text = contador.ToString();   // 1, 2, 3...
        }
    }
}
