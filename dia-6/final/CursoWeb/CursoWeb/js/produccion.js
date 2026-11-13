let loteIdEditando = 0;
let loteAEliminar = null;
let modalLote;
let modalConfirmar;
let filtroAplicado = ''; // los filtros con los que se pidió la tabla; el Excel usa los mismos

$(function () {
    modalLote = new bootstrap.Modal('#mdlLote');
    modalConfirmar = new bootstrap.Modal('#mdlConfirmar');

    // La fecha de producción no puede ser futura: el calendario no deja elegir días posteriores a hoy
    $('#txtFecha, #txtDesde, #txtHasta').attr('max', hoy());

    $('#btnNuevo').on('click', nuevo);
    $('#btnGuardar').on('click', guardar);
    $('#btnConfirmarEliminar').on('click', confirmarEliminar);
    $('#cboCliente').on('change', cambiarCliente);
    $('#cboFamilia').on('change', cambiarFamilia);
    $('#btnBuscar').on('click', buscar);
    $('#btnLimpiarFiltro').on('click', limpiarFiltro);
    $('#btnExportar').on('click', exportar);
    $('#mdlLote').on('shown.bs.modal', function () {
        $('#cboCliente').trigger('focus');
    });

    cargarClientes();
    cargarPiezasFiltro();
    inicializarTabla();
});

// La fecha de hoy con el formato aaaa-mm-dd que usa <input type="date">
function hoy() {
    const d = new Date();
    return d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0');
}

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

// Llena el combo de piezas del filtro con todas las piezas activas, reutilizando el handler de Piezas
function cargarPiezasFiltro() {
    $.ajax({
        url: 'Handlers/PiezasHandler.ashx?accion=listar',
        type: 'GET',
        dataType: 'json'
    })
        .done(function (r) {
            const $combo = $('#cboFiltroPieza');
            $combo.empty();
            $combo.append($('<option>').val('0').text('Todas las piezas'));
            if (r.ok) {
                r.datos.forEach(function (p) {
                    $combo.append($('<option>').val(p.piezaId).text(p.numeroParte + ' · ' + p.descripcion));
                });
            }
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudieron cargar las piezas del filtro (' + xhr.status + ').', 'danger');
        });
}

// Al cambiar el cliente, se piden al servidor sus familias
function cambiarCliente() {
    cargarFamilias(Number($('#cboCliente').val()) || 0, 0, 0);
}

// Al cambiar la familia, se piden al servidor sus piezas
function cambiarFamilia() {
    cargarPiezas(Number($('#cboFamilia').val()) || 0, 0);
}

// Llena el combo de familias con las del cliente. Al editar llegan también la familia y la pieza que deben quedar seleccionadas
function cargarFamilias(clienteId, familiaId, piezaId) {
    const $combo = $('#cboFamilia');
    $combo.empty();
    $combo.append($('<option>').val('').text('Selecciona una familia'));
    $combo.prop('disabled', true);
    cargarPiezas(0, 0);
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
                cargarPiezas(familiaId, piezaId);
            }
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudieron cargar las familias (' + xhr.status + ').', 'danger', '#divMensajeModal');
        });
}

// Llena el combo de piezas con las de la familia. Al editar llega también la pieza que debe quedar seleccionada
function cargarPiezas(familiaId, piezaId) {
    const $combo = $('#cboPieza');
    $combo.empty();
    $combo.append($('<option>').val('').text('Selecciona una pieza'));
    $combo.prop('disabled', true);
    if (!familiaId) {
        return;
    }

    $.ajax({
        url: 'Handlers/PiezasHandler.ashx?accion=listar&familiaId=' + familiaId,
        type: 'GET',
        dataType: 'json'
    })
        .done(function (r) {
            // Si la familia cambió mientras llegaba la respuesta, esta ya no sirve
            if (familiaId !== (Number($('#cboFamilia').val()) || 0)) {
                return;
            }
            if (!r.ok) {
                mostrarMensaje(r.mensaje, 'danger', '#divMensajeModal');
                return;
            }
            if (r.datos.length === 0) {
                mostrarMensaje('Esta familia no tiene piezas activas.', 'danger', '#divMensajeModal');
                return;
            }
            r.datos.forEach(function (p) {
                $combo.append($('<option>').val(p.piezaId).text(p.numeroParte + ' · ' + p.descripcion));
            });
            $combo.prop('disabled', false);
            if (piezaId) {
                $combo.val(piezaId);
            }
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudieron cargar las piezas (' + xhr.status + ').', 'danger', '#divMensajeModal');
        });
}

// Los filtros que hay en los controles, listos para agregarse a una URL; 0 y vacío significan "sin filtro"
function parametrosFiltro() {
    return '&piezaId=' + (Number($('#cboFiltroPieza').val()) || 0)
        + '&desde=' + $('#txtDesde').val()
        + '&hasta=' + $('#txtHasta').val();
}

// La URL del listado lleva los filtros con los que se pidió la tabla
function urlListado() {
    return 'Handlers/LotesHandler.ashx?accion=listar' + filtroAplicado;
}

// Pide la tabla otra vez, con los filtros que hay en los controles
function recargarTabla() {
    filtroAplicado = parametrosFiltro();
    $('#tblLotes').bootstrapTable('refresh', { url: urlListado() });
}

function buscar() {
    ocultarMensaje();
    const desde = $('#txtDesde').val();
    const hasta = $('#txtHasta').val();
    // Las fechas aaaa-mm-dd se pueden comparar como texto
    if (desde && hasta && desde > hasta) {
        mostrarMensaje('La fecha inicial no puede ser mayor que la final.', 'danger');
        return;
    }
    recargarTabla();
}

function limpiarFiltro() {
    $('#cboFiltroPieza').val('0');
    $('#txtDesde, #txtHasta').val('');
    buscar();
}

// Descarga el Excel con los mismos filtros que tiene la tabla
function exportar() {
    window.location = 'Handlers/LotesHandler.ashx?accion=exportar' + filtroAplicado;
}

function obtenerLote() {
    return {
        loteId: loteIdEditando,
        piezaId: Number($('#cboPieza').val()) || 0,
        numeroLote: $('#txtNumeroLote').val().trim(),
        cantidad: Number($('#txtCantidad').val()) || 0,
        fechaProduccion: $('#txtFecha').val(),
        turno: Number($('#cboTurno').val()) || 0
    };
}

function validarLote(l, clienteId, familiaId) {
    const errores = [];
    if (!clienteId) {
        errores.push('Selecciona un cliente.');
    }
    if (!familiaId) {
        errores.push('Selecciona una familia.');
    }
    if (!l.piezaId) {
        errores.push('Selecciona una pieza.');
    }
    if (!l.numeroLote) {
        errores.push('El número de lote es obligatorio.');
    }
    if (l.cantidad <= 0) {
        errores.push('La cantidad debe ser mayor a cero.');
    }
    if (!l.fechaProduccion) {
        errores.push('La fecha de producción es obligatoria.');
    } else if (l.fechaProduccion > hoy()) {
        errores.push('La fecha de producción no puede ser futura.');
    }
    if (!l.turno) {
        errores.push('Selecciona un turno.');
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
    modalLote.show();
}

function guardar() {
    ocultarMensaje('#divMensajeModal');
    const lote = obtenerLote();
    const errores = validarLote(lote, Number($('#cboCliente').val()) || 0, Number($('#cboFamilia').val()) || 0);
    if (errores.length > 0) {
        mostrarMensaje(errores.join(' '), 'danger', '#divMensajeModal');
        return;
    }
    enviarLote(lote);
}

function enviarLote(lote) {
    $('#btnGuardar').prop('disabled', true);

    $.ajax({
        url: 'Handlers/LotesHandler.ashx?accion=guardar',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(lote),
        dataType: 'json'
    })
        .done(function (r) {
            if (!r.ok) {
                mostrarMensaje(r.mensaje, 'danger', '#divMensajeModal');
                return;
            }
            modalLote.hide();
            mostrarMensaje(r.mensaje, 'success');
            limpiarCampos();
            recargarTabla();
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudo comunicar con el servidor (' + xhr.status + ').', 'danger', '#divMensajeModal');
        })
        .always(function () {
            $('#btnGuardar').prop('disabled', false);
        });
}

function limpiarCampos() {
    $('#cboCliente, #txtNumeroLote, #txtCantidad, #txtFecha, #cboTurno').val('');
    cargarFamilias(0, 0, 0);
    loteIdEditando = 0;
    $('#lblTituloFormulario').text('Nuevo lote');
}

function inicializarTabla() {
    $('#tblLotes').bootstrapTable({
        url: urlListado(),
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
            { field: 'numeroLote', title: 'Lote', sortable: true },
            { field: 'cantidad', title: 'Cantidad', align: 'right', sortable: true },
            { field: 'fechaProduccion', title: 'Fecha', sortable: true, formatter: formatoFecha },
            { field: 'turno', title: 'Turno', align: 'center', sortable: true },
            { field: 'loteId', title: 'Acciones', align: 'right', formatter: formatoAcciones, events: eventosAcciones }
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

function editar(l) {
    loteIdEditando = l.loteId;
    $('#cboCliente').val(l.clienteId);
    cargarFamilias(l.clienteId, l.familiaId, l.piezaId);
    $('#txtNumeroLote').val(l.numeroLote);
    $('#txtCantidad').val(l.cantidad);
    $('#txtFecha').val(l.fechaProduccion.substring(0, 10));
    $('#cboTurno').val(l.turno);
    $('#lblTituloFormulario').text('Editar lote');
    ocultarMensaje();
    ocultarMensaje('#divMensajeModal');
    modalLote.show();
}

function eliminar(l) {
    loteAEliminar = l;
    $('#lblConfirmar').text('¿Eliminar el lote "' + l.numeroLote + '" de la pieza ' + l.numeroParte + '?');
    ocultarMensaje();
    modalConfirmar.show();
}

function confirmarEliminar() {
    modalConfirmar.hide();

    $.ajax({
        url: 'Handlers/LotesHandler.ashx?accion=eliminar',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ loteId: loteAEliminar.loteId }),
        dataType: 'json'
    })
        .done(function (r) {
            mostrarMensaje(r.mensaje, r.ok ? 'success' : 'danger');
            if (r.ok) {
                recargarTabla();
            }
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudo comunicar con el servidor (' + xhr.status + ').', 'danger');
        });
}
