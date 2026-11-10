<%@ Page Title="Inicio" Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="CursoWeb.Default" MasterPageFile="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row">
        <div class="col-md-8">
            <h1>Hola desde el servidor</h1>
            <p class="lead">Catálogo de clientes, familias y piezas.</p>
        </div>
        <div class="col-md-4">
            <div class="card">
                <div class="card-body">
                    <h5 class="card-title">Servidor</h5>
                    <p class="card-text">Fecha y hora: <asp:Label ID="lblFecha" runat="server"></asp:Label></p>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
