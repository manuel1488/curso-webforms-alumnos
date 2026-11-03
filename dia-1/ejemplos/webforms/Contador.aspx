<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Contador.aspx.cs" Inherits="CursoWeb.Contador" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Contador · Web Forms</title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:Label ID="lblContador" runat="server" Text="0" Font-Size="36pt" Font-Bold="true" />
        <br />
        <asp:Button ID="btnSumar" runat="server" Text="Sumar" OnClick="btnSumar_Click" />
    </form>
</body>
</html>
