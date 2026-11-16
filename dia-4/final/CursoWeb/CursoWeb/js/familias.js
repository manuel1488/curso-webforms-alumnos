let familiaIdEditando = 0;
let familiaAEliminar = null;
let modalFamilia;
let modalConfirmar;

$(function () {
    modalFamilia = new bootstrap.Modal('#mdlFamilia');
    modalConfirmar = new bootstrap.Modal('#mdlConfirmar');

    $('#btnNuevo').on('click', nuevo);
    $('#btnGuardar').on('click', guardar);
    $('#btnConfirmarEliminar').on('click', confirmarEliminar);
    $('#mdlFamilia').on('shown.bs.modal', function () {
        $('#cboCliente').trigger('focus');
    });

    cargarClientes();
    inicializarTabla();
});

// Llena el combo con los clientes activos, reutilizando el handler de Clientes
function cargarClientes() {
    $.ajax({
        url: 'Handlers/ClientesHandler.ashx?accion=listar',
        type: 'GET',
        dataType: 'json'
    })
        .done(function (r) {
            const $combo = $('#cboCliente');
            $combo.empty();
            $combo.append($('<option>').val('').text('Selecciona un cliente'));
            if (r.ok) {
                r.datos.forEach(function (c) {
                    $combo.append($('<option>').val(c.clienteId).text(c.nombre));
                });
            }
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudieron cargar los clientes (' + xhr.status + ').', 'danger');
        });
}

function obtenerFamilia() {
    return {
        familiaId: familiaIdEditando,
        clienteId: Number($('#cboCliente').val()) || 0,
        nombre: $('#txtNombre').val().trim(),
        descripcion: $('#txtDescripcion').val().trim()
    };
}

function validarFamilia(f) {
    const errores = [];
    if (!f.clienteId) {
        errores.push('Selecciona un cliente.');
    }
    if (!f.nombre) {
        errores.push('El nombre es obligatorio.');
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
    modalFamilia.show();
}

function guardar() {
    ocultarMensaje('#divMensajeModal');
    const familia = obtenerFamilia();
    const errores = validarFamilia(familia);
    if (errores.length > 0) {
        mostrarMensaje(errores.join(' '), 'danger', '#divMensajeModal');
        return;
    }
    enviarFamilia(familia);
}

function enviarFamilia(familia) {
    $('#btnGuardar').prop('disabled', true);

    $.ajax({
        url: 'Handlers/FamiliasHandler.ashx?accion=guardar',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(familia),
        dataType: 'json'
    })
        .done(function (r) {
            if (!r.ok) {
                mostrarMensaje(r.mensaje, 'danger', '#divMensajeModal');
                return;
            }
            modalFamilia.hide();
            mostrarMensaje(r.mensaje, 'success');
            limpiarCampos();
            $('#tblFamilias').bootstrapTable('refresh');
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudo comunicar con el servidor (' + xhr.status + ').', 'danger', '#divMensajeModal');
        })
        .always(function () {
            $('#btnGuardar').prop('disabled', false);
        });
}

function limpiarCampos() {
    $('#cboCliente, #txtNombre, #txtDescripcion').val('');
    familiaIdEditando = 0;
    $('#lblTituloFormulario').text('Nueva familia');
}

function inicializarTabla() {
    $('#tblFamilias').bootstrapTable({
        url: 'Handlers/FamiliasHandler.ashx?accion=listar',
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
            { field: 'clienteNombre', title: 'Cliente', sortable: true },
            { field: 'nombre', title: 'Familia', sortable: true },
            { field: 'descripcion', title: 'Descripción' },
            { field: 'fechaAlta', title: 'Alta', sortable: true, formatter: formatoFecha },
            { field: 'familiaId', title: 'Acciones', align: 'right', formatter: formatoAcciones, events: eventosAcciones }
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

function editar(f) {
    familiaIdEditando = f.familiaId;
    $('#cboCliente').val(f.clienteId);
    $('#txtNombre').val(f.nombre);
    $('#txtDescripcion').val(f.descripcion || '');
    $('#lblTituloFormulario').text('Editar familia');
    ocultarMensaje();
    ocultarMensaje('#divMensajeModal');
    modalFamilia.show();
}

function eliminar(f) {
    familiaAEliminar = f;
    $('#lblConfirmar').text('¿Eliminar la familia "' + f.nombre + '" de ' + f.clienteNombre + '?');
    ocultarMensaje();
    modalConfirmar.show();
}

function confirmarEliminar() {
    modalConfirmar.hide();

    $.ajax({
        url: 'Handlers/FamiliasHandler.ashx?accion=eliminar',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ familiaId: familiaAEliminar.familiaId }),
        dataType: 'json'
    })
        .done(function (r) {
            mostrarMensaje(r.mensaje, r.ok ? 'success' : 'danger');
            if (r.ok) {
                $('#tblFamilias').bootstrapTable('refresh');
            }
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudo comunicar con el servidor (' + xhr.status + ').', 'danger');
        });
}
