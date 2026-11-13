<%@ Page Title="Producción" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Produccion.aspx.cs" Inherits="CursoWeb.Produccion" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h1 class="mb-0">Producción</h1>
        <button type="button" id="btnNuevo" class="btn btn-primary">Nuevo lote</button>
    </div>
    <div id="divMensaje" class="alert d-none" role="alert"></div>

    <!-- Filtro del listado -->
    <div class="row g-3 align-items-end mb-3">
        <div class="col-md-4">
            <label for="cboFiltroPieza" class="form-label">Pieza</label>
            <select id="cboFiltroPieza" class="form-select"></select>
        </div>
        <div class="col-md-2">
            <label for="txtDesde" class="form-label">Desde</label>
            <input type="date" id="txtDesde" class="form-control" />
        </div>
        <div class="col-md-2">
            <label for="txtHasta" class="form-label">Hasta</label>
            <input type="date" id="txtHasta" class="form-control" />
        </div>
        <div class="col-md-4">
            <button type="button" id="btnBuscar" class="btn btn-primary">Buscar</button>
            <button type="button" id="btnLimpiarFiltro" class="btn btn-outline-secondary">Limpiar</button>
            <button type="button" id="btnExportar" class="btn btn-success">Exportar a Excel</button>
        </div>
    </div>

    <table id="tblLotes"></table>

    <!-- Modal: alta y edición -->
    <div class="modal fade" id="mdlLote" tabindex="-1" aria-labelledby="lblTituloFormulario" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 id="lblTituloFormulario" class="modal-title">Nuevo lote</h5>
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
                            <label for="cboPieza" class="form-label">Pieza</label>
                            <select id="cboPieza" class="form-select" disabled></select>
                        </div>
                        <div class="col-12">
                            <label for="txtNumeroLote" class="form-label">Número de lote</label>
                            <input type="text" id="txtNumeroLote" class="form-control" maxlength="50" />
                        </div>
                        <div class="col-md-4">
                            <label for="txtCantidad" class="form-label">Cantidad</label>
                            <input type="number" id="txtCantidad" class="form-control" min="1" step="1" />
                        </div>
                        <div class="col-md-4">
                            <label for="txtFecha" class="form-label">Fecha</label>
                            <input type="date" id="txtFecha" class="form-control" />
                        </div>
                        <div class="col-md-4">
                            <label for="cboTurno" class="form-label">Turno</label>
                            <select id="cboTurno" class="form-select">
                                <option value="">Selecciona</option>
                                <option value="1">1</option>
                                <option value="2">2</option>
                                <option value="3">3</option>
                            </select>
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
                    <h5 id="lblTituloConfirmar" class="modal-title">Eliminar lote</h5>
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
    <script src="js/produccion.js"></script>
</asp:Content>
