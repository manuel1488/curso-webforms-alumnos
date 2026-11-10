<%@ Page Title="Inicio" Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="CursoWeb.Default" MasterPageFile="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Hola desde el servidor</h1>
    <p>Fecha y hora del servidor: <asp:Label ID="lblFecha" runat="server"></asp:Label></p>
</asp:Content>
