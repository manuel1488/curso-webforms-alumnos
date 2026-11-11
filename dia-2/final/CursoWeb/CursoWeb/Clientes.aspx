<%@ Page Title="Clientes" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Clientes.aspx.cs" Inherits="CursoWeb.Clientes" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1 class="mb-4">Clientes</h1>
    <div id="divMensaje" class="alert d-none" role="alert"></div>

    <div class="card mb-4">
        <div class="card-body">
            <h5 class="card-title">Nuevo cliente</h5>
            <div class="row g-3">
                <div class="col-md-6">
                    <label for="txtNombre" class="form-label">Nombre</label>
                    <input type="text" id="txtNombre" class="form-control" maxlength="150" />
                </div>
                <div class="col-md-3">
                    <label for="txtRFC" class="form-label">RFC</label>
                    <input type="text" id="txtRFC" class="form-control" maxlength="13" />
                </div>
                <div class="col-md-3">
                    <label for="txtContacto" class="form-label">Contacto</label>
                    <input type="text" id="txtContacto" class="form-control" maxlength="100" />
                </div>
                <div class="col-md-6">
                    <label for="txtCorreo" class="form-label">Correo</label>
                    <input type="email" id="txtCorreo" class="form-control" maxlength="150" />
                </div>
            </div>
            <div class="mt-3">
                <button type="button" id="btnGuardar" class="btn btn-primary">Guardar</button>
                <button type="button" id="btnLimpiar" class="btn btn-outline-secondary">Limpiar</button>
            </div>
        </div>
    </div>

    <table id="tblClientes"></table>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="Scripts" runat="server">
    <script src="js/clientes.js"></script>
</asp:Content>
