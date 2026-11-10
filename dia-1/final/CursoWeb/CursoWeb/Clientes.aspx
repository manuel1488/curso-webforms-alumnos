<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Clientes.aspx.cs" Inherits="CursoWeb.Clientes" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1 class="mb-4">Clientes</h1>

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
                    <label for="txtContacto" class="form-label">Conctacto</label>
                    <input type="text" id="txtContacto" class="form-control" maxlength="100" />
                </div>
                <div class="col-md-6">
                    <label for="txtCorreo" class="form-label">Correo</label>
                    <input type="email" id="txtCorreo" class="form-control" maxlength="150" />
                </div>
            </div>
            <div class="mb-3">
                <button type="button" id="btnGuardar" class="btn btn-primary">Guardar</button>
                <button type="button" id="btnLimpiar" class="btn btn-outline-secondary">Limpiar</button>
            </div>
        </div>
    </div>

    <table id="tblClientes" class="table table-striped table-hover">
        <thead>
            <tr>
                <th>Nombre</th>
                <th>RFC</th>
                <th>Contacto</th>
                <th>Correo</th>
                <th class="text-end">Acciones</th>
            </tr>
        </thead>
        <tbody>
            <tr>
                <td>Autorpartes del Bajio</td>
                <td>ABA010101AA1</td>
                <td>Laura Mendez</td>
                <td>laura@ejemplo.com</td>
                <td class="text-end">
                    <button type="button" class="btn btn-sm btn-outline-primary">Editar</button>
                    <button type="button" class="btn btn-sm btn-outline-danger">Eliminar</button>
                </td>
            </tr>
            <tr>
                <td>Componentes Industriales GTO</td>
                <td>CIG020202BB2</td>
                <td>Jorge Ramírez</td>
                <td>jorge@ejemplo.com</td>
                <td class="text-end">
                    <button type="button" class="btn btn-sm btn-outline-primary">Editar</button>
                    <button type="button" class="btn btn-sm btn-outline-danger">Eliminar</button>
                </td>
            </tr>
        </tbody>
    </table>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="Scripts" runat="server">
</asp:Content>
