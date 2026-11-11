$(function () {
    $('#btnGuardar').on('click', guardar);
    $('#btnLimpiar').on('click', limpiar);
    inicializarTabla();
});

function obtenerCliente() {
    return {
        clienteId: 0,
        nombre: $('#txtNombre').val().trim(),
        rfc: $('#txtRFC').val().trim(),
        contacto: $('#txtContacto').val().trim(),
        correo: $('#txtCorreo').val().trim()
    };
}

function validarCliente(c) {
    const errores = [];
    if (!c.nombre.trim()) {
        errores.push('El nombre es obligatorio.');
    }
    if (c.nombre.length > 150) {
        errores.push('El nombre no puede pasar de 150 caracteres.');
    }
    if (c.correo && !c.correo.includes('@')) {
        errores.push('El correo no es válido.');
    }
    return errores;
}

function mostrarMensaje(texto, tipo) {
    $('#divMensaje')
        .removeClass('d-none alert-success alert-danger')
        .addClass('alert-' + tipo)
        .text(texto);
}

function ocultarMensaje() {
    $('#divMensaje').addClass('d-none').empty();
}

function guardar() {
    ocultarMensaje();
    const cliente = obtenerCliente();
    const errores = validarCliente(cliente);
    if (errores.length > 0) {
        mostrarMensaje(errores.join(' '), 'danger');
        return;
    }
    enviarCliente(cliente);
}

function enviarCliente(cliente) {
    $('#btnGuardar').prop('disabled', true);

    $.ajax({
        url: 'Handlers/ClientesHandler.ashx?accion=guardar',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(cliente),
        dataType: 'json'
    })
        .done(function (r) {
            if (!r.ok) {
                mostrarMensaje(r.mensaje, 'danger');
                return;
            }
            mostrarMensaje(r.mensaje, 'success');
            limpiarCampos();
            $('#tblClientes').bootstrapTable('refresh');
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudo comunicar con el servidor (' + xhr.status + ').', 'danger');
        })
        .always(function () {
            $('#btnGuardar').prop('disabled', false);
        });
}

function limpiarCampos() {
    $('#txtNombre, #txtRFC, #txtContacto, #txtCorreo').val('');
    $('#txtNombre').trigger('focus');
}

function limpiar() {
    limpiarCampos();
    ocultarMensaje();
}

function inicializarTabla() {
    $('#tblClientes').bootstrapTable({
        url: 'Handlers/ClientesHandler.ashx?accion=listar',
        responseHandler: function (r) {
            return r.ok ? r.datos : [];
        },
        classes: 'table table-striped table-hover',
        escape: true,
        search: true,
        pagination: true,
        pageSize: 10,
        locale: 'es-MX',
        columns: [
            { field: 'nombre', title: 'Nombre', sortable: true },
            { field: 'rfc', title: 'RFC' },
            { field: 'contacto', title: 'Contacto' },
            { field: 'correo', title: 'Correo' },
            { field: 'fechaAlta', title: 'Alta', sortable: true, formatter: formatoFecha }
        ]
    });
}

function formatoFecha(valor) {
    return valor ? new Date(valor).toLocaleDateString('es-MX') : '';
}
