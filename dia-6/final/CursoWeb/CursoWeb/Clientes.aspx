<%@ Page Title="Clientes" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Clientes.aspx.cs" Inherits="CursoWeb.Clientes" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h1 class="mb-0">Clientes</h1>
        <button type="button" id="btnNuevo" class="btn btn-primary">Nuevo cliente</button>
    </div>
    <div id="divMensaje" class="alert d-none" role="alert"></div>

    <table id="tblClientes"></table>

    <!-- Modal: alta y edición -->
    <div class="modal fade" id="mdlCliente" tabindex="-1" aria-labelledby="lblTituloFormulario" aria-hidden="true">
        <div class="modal-dialog modal-lg">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 id="lblTituloFormulario" class="modal-title">Nuevo cliente</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>
                <div class="modal-body">
                    <div id="divMensajeModal" class="alert d-none" role="alert"></div>
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
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Cancelar</button>
                    <button type="button" id="btnGuardar" class="btn btn-primary">Guardar</button>
                </div>
            </div>
        </div>
    </div>

    <!-- Modal: confirmar eliminación -->
    <div class="modal fade" id="mdlConfirmar" tabindex="-1" aria-labelledby="lblTituloConfirmar" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 id="lblTituloConfirmar" class="modal-title">Eliminar cliente</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>
                <div class="modal-body">
                    <p id="lblConfirmar" class="mb-0"></p>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Cancelar</button>
                    <button type="button" id="btnConfirmarEliminar" class="btn btn-danger">Eliminar</button>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="Scripts" runat="server">
    <script src="js/clientes.js"></script>
</asp:Content>
