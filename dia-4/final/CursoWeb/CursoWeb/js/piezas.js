let piezaIdEditando = 0;
let piezaAEliminar = null;
let modalPieza;
let modalConfirmar;

$(function () {
    modalPieza = new bootstrap.Modal('#mdlPieza');
    modalConfirmar = new bootstrap.Modal('#mdlConfirmar');

    $('#btnNuevo').on('click', nuevo);
    $('#btnGuardar').on('click', guardar);
    $('#btnConfirmarEliminar').on('click', confirmarEliminar);
    $('#cboCliente').on('change', cambiarCliente);
    $('#mdlPieza').on('shown.bs.modal', function () {
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

// Al cambiar el cliente, se piden al servidor sus familias
function cambiarCliente() {
    cargarFamilias(Number($('#cboCliente').val()) || 0, 0);
}

// Llena el combo de familias con las del cliente. Si llega familiaId (al editar), la deja seleccionada
function cargarFamilias(clienteId, familiaId) {
    const $combo = $('#cboFamilia');
    $combo.empty();
    $combo.append($('<option>').val('').text('Selecciona una familia'));
    $combo.prop('disabled', true);
    ocultarMensaje('#divMensajeModal');
    if (!clienteId) {
        return;
    }

    $.ajax({
        url: 'Handlers/FamiliasHandler.ashx?accion=listarPorCliente&clienteId=' + clienteId,
        type: 'GET',
        dataType: 'json'
    })
        .done(function (r) {
            // Si el cliente cambió mientras llegaba la respuesta, esta ya no sirve
            if (clienteId !== (Number($('#cboCliente').val()) || 0)) {
                return;
            }
            if (!r.ok) {
                mostrarMensaje(r.mensaje, 'danger', '#divMensajeModal');
                return;
            }
            if (r.datos.length === 0) {
                mostrarMensaje('Este cliente no tiene familias activas.', 'danger', '#divMensajeModal');
                return;
            }
            r.datos.forEach(function (f) {
                $combo.append($('<option>').val(f.familiaId).text(f.nombre));
            });
            $combo.prop('disabled', false);
            if (familiaId) {
                $combo.val(familiaId);
            }
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudieron cargar las familias (' + xhr.status + ').', 'danger', '#divMensajeModal');
        });
}

function obtenerPieza() {
    return {
        piezaId: piezaIdEditando,
        familiaId: Number($('#cboFamilia').val()) || 0,
        numeroParte: $('#txtNumeroParte').val().trim(),
        descripcion: $('#txtDescripcion').val().trim()
    };
}

function validarPieza(p, clienteId) {
    const errores = [];
    if (!clienteId) {
        errores.push('Selecciona un cliente.');
    }
    if (!p.familiaId) {
        errores.push('Selecciona una familia.');
    }
    if (!p.numeroParte) {
        errores.push('El número de parte es obligatorio.');
    }
    if (!p.descripcion) {
        errores.push('La descripción es obligatoria.');
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
    modalPieza.show();
}

function guardar() {
    ocultarMensaje('#divMensajeModal');
    const pieza = obtenerPieza();
    const errores = validarPieza(pieza, Number($('#cboCliente').val()) || 0);
    if (errores.length > 0) {
        mostrarMensaje(errores.join(' '), 'danger', '#divMensajeModal');
        return;
    }
    enviarPieza(pieza);
}

function enviarPieza(pieza) {
    $('#btnGuardar').prop('disabled', true);

    $.ajax({
        url: 'Handlers/PiezasHandler.ashx?accion=guardar',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(pieza),
        dataType: 'json'
    })
        .done(function (r) {
            if (!r.ok) {
                mostrarMensaje(r.mensaje, 'danger', '#divMensajeModal');
                return;
            }
            modalPieza.hide();
            mostrarMensaje(r.mensaje, 'success');
            limpiarCampos();
            $('#tblPiezas').bootstrapTable('refresh');
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudo comunicar con el servidor (' + xhr.status + ').', 'danger', '#divMensajeModal');
        })
        .always(function () {
            $('#btnGuardar').prop('disabled', false);
        });
}

function limpiarCampos() {
    $('#cboCliente, #txtNumeroParte, #txtDescripcion').val('');
    cargarFamilias(0, 0);
    piezaIdEditando = 0;
    $('#lblTituloFormulario').text('Nueva pieza');
}

function inicializarTabla() {
    $('#tblPiezas').bootstrapTable({
        url: 'Handlers/PiezasHandler.ashx?accion=listar',
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
            { field: 'familiaNombre', title: 'Familia', sortable: true },
            { field: 'numeroParte', title: 'Número de parte', sortable: true },
            { field: 'descripcion', title: 'Descripción' },
            { field: 'fechaAlta', title: 'Alta', sortable: true, formatter: formatoFecha },
            { field: 'piezaId', title: 'Acciones', align: 'right', formatter: formatoAcciones, events: eventosAcciones }
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

function editar(p) {
    piezaIdEditando = p.piezaId;
    $('#cboCliente').val(p.clienteId);
    cargarFamilias(p.clienteId, p.familiaId);
    $('#txtNumeroParte').val(p.numeroParte);
    $('#txtDescripcion').val(p.descripcion);
    $('#lblTituloFormulario').text('Editar pieza');
    ocultarMensaje();
    ocultarMensaje('#divMensajeModal');
    modalPieza.show();
}

function eliminar(p) {
    piezaAEliminar = p;
    $('#lblConfirmar').text('¿Eliminar la pieza "' + p.numeroParte + '" de ' + p.familiaNombre + '?');
    ocultarMensaje();
    modalConfirmar.show();
}

function confirmarEliminar() {
    modalConfirmar.hide();

    $.ajax({
        url: 'Handlers/PiezasHandler.ashx?accion=eliminar',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ piezaId: piezaAEliminar.piezaId }),
        dataType: 'json'
    })
        .done(function (r) {
            mostrarMensaje(r.mensaje, r.ok ? 'success' : 'danger');
            if (r.ok) {
                $('#tblPiezas').bootstrapTable('refresh');
            }
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudo comunicar con el servidor (' + xhr.status + ').', 'danger');
        });
}
