<%@ Page Title="Piezas" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Piezas.aspx.cs" Inherits="CursoWeb.Piezas" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h1 class="mb-0">Piezas</h1>
        <button type="button" id="btnNuevo" class="btn btn-primary">Nueva pieza</button>
    </div>
    <div id="divMensaje" class="alert d-none" role="alert"></div>

    <!-- Filtro del listado -->
    <div class="row g-3 align-items-end mb-3">
        <div class="col-md-4">
            <label for="cboFiltroCliente" class="form-label">Cliente</label>
            <select id="cboFiltroCliente" class="form-select"></select>
        </div>
        <div class="col-md-4">
            <label for="cboFiltroFamilia" class="form-label">Familia</label>
            <select id="cboFiltroFamilia" class="form-select" disabled></select>
        </div>
        <div class="col-md-4">
            <button type="button" id="btnBuscar" class="btn btn-primary">Buscar</button>
            <button type="button" id="btnLimpiarFiltro" class="btn btn-outline-secondary">Limpiar</button>
            <button type="button" id="btnExportar" class="btn btn-success">Exportar a Excel</button>
        </div>
    </div>

    <table id="tblPiezas"></table>

    <!-- Modal: alta y edición -->
    <div class="modal fade" id="mdlPieza" tabindex="-1" aria-labelledby="lblTituloFormulario" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 id="lblTituloFormulario" class="modal-title">Nueva pieza</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>
                <div class="modal-body">
                    <div id="divMensajeModal" class="alert d-none" role="alert"></div>
                    <div class="row g-3">
                        <div class="col-12">
                            <label for="cboCliente" class="form-label">Cliente</label>
                            <select id="cboCliente" class="form-select"></select>
                        </div>
                        <div class="col-12">
                            <label for="cboFamilia" class="form-label">Familia</label>
                            <select id="cboFamilia" class="form-select" disabled></select>
                        </div>
                        <div class="col-12">
                            <label for="txtNumeroParte" class="form-label">Número de parte</label>
                            <input type="text" id="txtNumeroParte" class="form-control" maxlength="50" />
                        </div>
                        <div class="col-12">
                            <label for="txtDescripcion" class="form-label">Descripción</label>
                            <textarea id="txtDescripcion" class="form-control" rows="3" maxlength="250"></textarea>
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
                    <h5 id="lblTituloConfirmar" class="modal-title">Eliminar pieza</h5>
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
    <script src="js/piezas.js"></script>
</asp:Content>
