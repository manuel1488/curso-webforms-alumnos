let clienteIdEditando = 0;
let clienteAEliminar = null;
let modalCliente;
let modalConfirmar;

$(function () {
    modalCliente = new bootstrap.Modal('#mdlCliente');
    modalConfirmar = new bootstrap.Modal('#mdlConfirmar');

    $('#btnNuevo').on('click', nuevo);
    $('#btnGuardar').on('click', guardar);
    $('#btnConfirmarEliminar').on('click', confirmarEliminar);
    $('#mdlCliente').on('shown.bs.modal', function () {
        $('#txtNombre').trigger('focus');
    });

    inicializarTabla();
});

function obtenerCliente() {
    return {
        clienteId: clienteIdEditando,
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

function mostrarMensaje(texto, tipo, contenedor = '#divMensaje') {
    $(contenedor)
        .removeClass('d-none alert-success alert-danger')
        .addClass('alert-' + tipo)
        .text(texto);
}

function ocultarMensaje(contenedor = '#divMensaje') {
    $(contenedor).addClass('d-none').empty();
}

function nuevo() {
    limpiarCampos();
    ocultarMensaje();
    ocultarMensaje('#divMensajeModal');
    modalCliente.show();
}

function guardar() {
    ocultarMensaje('#divMensajeModal');
    const cliente = obtenerCliente();
    const errores = validarCliente(cliente);
    if (errores.length > 0) {
        mostrarMensaje(errores.join(' '), 'danger', '#divMensajeModal');
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
                mostrarMensaje(r.mensaje, 'danger', '#divMensajeModal');
                return;
            }
            modalCliente.hide();
            mostrarMensaje(r.mensaje, 'success');
            limpiarCampos();
            $('#tblClientes').bootstrapTable('refresh');
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudo comunicar con el servidor (' + xhr.status + ').', 'danger', '#divMensajeModal');
        })
        .always(function () {
            $('#btnGuardar').prop('disabled', false);
        });
}

function limpiarCampos() {
    $('#txtNombre, #txtRFC, #txtContacto, #txtCorreo').val('');
    clienteIdEditando = 0;
    $('#lblTituloFormulario').text('Nuevo cliente');
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
            { field: 'fechaAlta', title: 'Alta', sortable: true, formatter: formatoFecha },
            { field: 'clienteId', title: 'Acciones', align: 'right', formatter: formatoAcciones, events: eventosAcciones }
        ]
    });
}

function formatoFecha(valor) {
    return valor ? new Date(valor).toLocaleDateString('es-MX') : '';
}

function formatoAcciones() {
    return '<button type="button" class="btn btn-sm btn-outline-primary btn-editar">Editar</button> ' +
        '<button type="button" class="btn btn-sm btn-outline-danger btn-eliminar">Eliminar</button>';
}

const eventosAcciones = {
    'click .btn-editar': function (e, valor, renglon) {
        editar(renglon);
    },
    'click .btn-eliminar': function (e, valor, renglon) {
        eliminar(renglon);
    }
};

function editar(c) {
    clienteIdEditando = c.clienteId;
    $('#txtNombre').val(c.nombre);
    $('#txtRFC').val(c.rfc || '');
    $('#txtContacto').val(c.contacto || '');
    $('#txtCorreo').val(c.correo || '');
    $('#lblTituloFormulario').text('Editar cliente');
    ocultarMensaje();
    ocultarMensaje('#divMensajeModal');
    modalCliente.show();
}

function eliminar(c) {
    clienteAEliminar = c;
    $('#lblConfirmar').text('¿Eliminar al cliente "' + c.nombre + '"?');
    ocultarMensaje();
    modalConfirmar.show();
}

function confirmarEliminar() {
    modalConfirmar.hide();

    $.ajax({
        url: 'Handlers/ClientesHandler.ashx?accion=eliminar',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ clienteId: clienteAEliminar.clienteId }),
        dataType: 'json'
    })
        .done(function (r) {
            mostrarMensaje(r.mensaje, r.ok ? 'success' : 'danger');
            if (r.ok) {
                $('#tblClientes').bootstrapTable('refresh');
            }
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudo comunicar con el servidor (' + xhr.status + ').', 'danger');
        });
}
