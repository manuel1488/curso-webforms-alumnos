using System;

namespace CursoWeb
{
    public partial class Contador : System.Web.UI.Page
    {
        private int contador = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnSumar_Click(object sender, EventArgs e)
        {
            contador++;
            lblContador.Text = contador.ToString();   // siempre 1
        }
    }
}
