
exigirSesion();

/* ============================================================
   1. ESTADO Y REFERENCIAS
   ============================================================ */
let presupuestosCache = [];

const filtroMonthYear    = document.getElementById('filtroMonthYear');
const listaPresupuestos  = document.getElementById('listaPresupuestos');
const presupuestosBanner = document.getElementById('presupuestosBanner');
const alertaExcedidos    = document.getElementById('alertaExcedidos');

const modalOverlay    = document.getElementById('modalOverlay');
const formPresupuesto = document.getElementById('formPresupuesto');
const modalTitulo     = document.getElementById('modalTitulo');
const modalBanner     = document.getElementById('modalBanner');
const btnGuardar      = document.getElementById('btnGuardarPresupuesto');
const selectCategoria = document.getElementById('presupuestoCategoria');

/* ============================================================
   2. FILTRO DE MES / AÑO
   ============================================================ */
function inicializarFiltroMonthYear(){
  const ahora = new Date();
  const valor = `${ahora.getFullYear()}-${String(ahora.getMonth() + 1).padStart(2, '0')}`;
  filtroMonthYear.value = valor;
}

function obtenerMonthYearFiltro(){
  const [year, month] = filtroMonthYear.value.split('-').map(Number);
  return { month, year };
}

filtroMonthYear.addEventListener('change', cargarLista);

/* ============================================================
   3. LEER (GET) + NORMALIZACIÓN
   ============================================================ */
async function cargarLista(){
  ocultarBanner(presupuestosBanner);
  listaPresupuestos.innerHTML = `<div class="estado-vacio">Cargando...</div>`;

  const { month, year } = obtenerMonthYearFiltro();

  try{
    const respuesta = await hacerPeticionAutenticada(
      `${API_BASE_URL}/presupuestos?month=${month}&year=${year}`
    );

    if(!respuesta.ok){
      const mensajes = await parsearErroresBackend(respuesta);
      listaPresupuestos.innerHTML = `<div class="estado-error">${mensajes[0]}</div>`;
      ocultarAlertaExcedidos();
      return;
    }

    const crudos = await respuesta.json();
    presupuestosCache = (Array.isArray(crudos) ? crudos : []).map(normalizarPresupuesto);

    pintarLista();

  }catch(error){
    listaPresupuestos.innerHTML = `<div class="estado-error">No se pudo conectar con el servidor.</div>`;
    ocultarAlertaExcedidos();
  }
}

function normalizarPresupuesto(p){
  return {
    id: p.id,
    categoriaId: p.categoriaId,
    nombreCategoria: p.nombreCategoria ?? 'Sin categoría',
    montoPresupuestado: Number(p.montoPresupuestado ?? 0),
    montoGastado: Number(p.montoGastado ?? 0),
    porcentajeConsumido: Number(p.porcentajeConsumido ?? 0),
    nivelAlerta: p.nivelAlerta ?? 'Bajo',
    month: p.month,
    year: p.year
  };
}

/* ============================================================
   4. REGLA DE COLOR DINÁMICA (Basada en el Porcentaje Real)
   ============================================================ */
function estadoPorPorcentaje(porcentaje){
  if(porcentaje >= 100) return { clase: 'nivel-excedido', etiqueta: 'Excedido' };
  if(porcentaje >= 80)  return { clase: 'nivel-alto',      etiqueta: 'Cerca del límite' };
  if(porcentaje >= 50)  return { clase: 'nivel-medio',     etiqueta: 'Mitad consumida' };
  return { clase: 'nivel-bajo', etiqueta: 'Bajo control' };
}

/* ============================================================
   5. PINTADO DE TARJETAS
   ============================================================ */
function pintarLista(){
  pintarAlertaExcedidos();

  if(presupuestosCache.length === 0){
    listaPresupuestos.innerHTML = `<div class="estado-vacio">No tienes presupuestos configurados para este mes. ¡Agrega el primero!</div>`;
    return;
  }

  listaPresupuestos.innerHTML = presupuestosCache.map(p => {
    const estado = estadoPorPorcentaje(p.porcentajeConsumido);
    
    const anchoBarra = Math.min(p.porcentajeConsumido, 100).toFixed(1);
    const restante = p.montoPresupuestado - p.montoGastado;

    return `
      <article class="presupuesto-card ${estado.clase}" data-id="${p.id}">
        <div class="presupuesto-card-header">
          <div class="presupuesto-nombre">
            <span class="catalogo-punto"></span>
            ${p.nombreCategoria}
          </div>
          <div class="acciones-celda">
            <button class="btn-icono" title="Editar" data-accion="editar" data-id="${p.id}">✎</button>
            <button class="btn-icono peligro" title="Eliminar" data-accion="eliminar" data-id="${p.id}">✕</button>
          </div>
        </div>

        <div class="presupuesto-montos">
          <span class="monto-gastado">${formatearMonto(p.montoGastado)}</span>
          <span class="monto-separador">de</span>
          <span class="monto-maximo">${formatearMonto(p.montoPresupuestado)}</span>
        </div>

        <div class="barra-progreso-track">
          <div class="barra-progreso-fill" style="width:${anchoBarra}%"></div>
        </div>

        <div class="presupuesto-card-footer">
          <span class="presupuesto-porcentaje">${p.porcentajeConsumido.toFixed(0)}% usado</span>
          <span class="presupuesto-estado-tag">${estado.etiqueta}</span>
        </div>

        ${restante < 0
          ? `<div class="presupuesto-excedido-nota">Te excediste por ${formatearMonto(Math.abs(restante))}</div>`
          : ''}
      </article>
    `;
  }).join('');
}

/* ============================================================
   6. ALERTA RESUMEN: CATEGORÍAS EXCEDIDAS
   ============================================================ */
function pintarAlertaExcedidos(){
  const excedidos = presupuestosCache.filter(p => p.porcentajeConsumido >= 100);

  if(excedidos.length === 0){
    ocultarAlertaExcedidos();
    return;
  }

  alertaExcedidos.classList.add('show');
  alertaExcedidos.textContent = `⚠ Tienes ${excedidos.length} ${excedidos.length === 1 ? 'categoría excediendo' : 'categorías excediendo'} el presupuesto.`;
}

function ocultarAlertaExcedidos(){
  alertaExcedidos.classList.remove('show');
  alertaExcedidos.textContent = '';
}

/* ============================================================
   7. MODAL: AGREGAR / EDITAR
   ============================================================ */
function abrirModal(presupuesto){
  modalOverlay.classList.add('show');
  ocultarBanner(modalBanner);
  limpiarErroresDeCampos(formPresupuesto);

  if(presupuesto){
    modalTitulo.textContent = 'Editar presupuesto';
    btnGuardar.querySelector('.btn-label').textContent = 'Guardar cambios';
    document.getElementById('presupuestoId').value = presupuesto.id;
    document.getElementById('presupuestoCategoria').value = presupuesto.categoriaId ?? '';
    document.getElementById('presupuestoMonto').value = presupuesto.montoPresupuestado;
    
    const monthStr = String(presupuesto.month).padStart(2, '0');
    document.getElementById('presupuestoMonthYear').value = `${presupuesto.year}-${monthStr}`;
  } else {
    modalTitulo.textContent = 'Agregar presupuesto';
    btnGuardar.querySelector('.btn-label').textContent = 'Guardar';
    document.getElementById('presupuestoId').value = '';
    document.getElementById('presupuestoCategoria').value = '';
    document.getElementById('presupuestoMonto').value = '';
    document.getElementById('presupuestoMonthYear').value = filtroMonthYear.value;
  }

  document.getElementById('presupuestoCategoria').focus();
}

function cerrarModal(){
  modalOverlay.classList.remove('show');
  formPresupuesto.reset();
  document.getElementById('presupuestoId').value = '';
  ocultarBanner(modalBanner);
  limpiarErroresDeCampos(formPresupuesto);
}

document.getElementById('btnAbrirModal').addEventListener('click', () => abrirModal());
document.getElementById('btnCancelarModal').addEventListener('click', cerrarModal);
modalOverlay.addEventListener('click', (e) => {
  if(e.target === modalOverlay) cerrarModal();
});

/* ============================================================
   8. CREAR (POST) / EDITAR (PATCH)
   ============================================================ */
formPresupuesto.addEventListener('submit', async (e) => {
  e.preventDefault();

  ocultarBanner(modalBanner);
  limpiarErroresDeCampos(formPresupuesto);

  const id = document.getElementById('presupuestoId').value;
  const categoriaId = document.getElementById('presupuestoCategoria').value;

  const montoIngresado = document.getElementById('presupuestoMonto').value;
  const monthYear = document.getElementById('presupuestoMonthYear').value;

  const erroresCliente = [];
  if(!categoriaId){
    erroresCliente.push('Selecciona una categoría.');
    marcarCampoInvalido('presupuestoCategoria', 'errPresupuestoCategoria', 'Selecciona una categoría.');
  }
  if(!montoIngresado || Number(montoIngresado) <= 0){
    erroresCliente.push('El monto máximo debe ser mayor a 0.');
    marcarCampoInvalido('presupuestoMonto', 'errPresupuestoMonto', 'Ingresa un monto válido.');
  }
  if(!monthYear){
    erroresCliente.push('Selecciona el mes y año.');
    marcarCampoInvalido('presupuestoMonthYear', 'errPresupuestoMonthYear', 'Selecciona el mes y año.');
  }

  if(erroresCliente.length){
    mostrarErrores(modalBanner, erroresCliente);
    return;
  }

  const [year, month] = monthYear.split('-').map(Number);
  const esEdicion = Boolean(id);

  ponerBotonCargando(btnGuardar, true);

  try{

    const url = esEdicion ? `${API_BASE_URL}/Presupuestos/${id}` : `${API_BASE_URL}/presupuestos`;
    const method = esEdicion ? 'PATCH' : 'POST';

    let bodyData = {};
    if (esEdicion) {
      bodyData = { montoMaximo: montoIngresado.toString() };
    } else {
      bodyData = { 
        categoriaId: categoriaId.toString(), 
        month: month.toString(), 
        year: year.toString(), 
        montoMaximo: montoIngresado.toString() 
      };
    }

    const respuesta = await hacerPeticionAutenticada(url, {
      method: method,
      body: JSON.stringify(bodyData),
    });

    if(!respuesta.ok){
      const mensajes = await parsearErroresBackend(respuesta);
      mostrarErrores(modalBanner, mensajes);
      return;
    }

    cerrarModal();
    mostrarExito(presupuestosBanner, `Presupuesto ${esEdicion ? 'actualizado' : 'guardado'} correctamente.`);

    const { month: monthFiltro, year: yearFiltro } = obtenerMonthYearFiltro();
    if(month === monthFiltro && year === yearFiltro){
      cargarLista();
    }

  }catch(error){
    mostrarErrores(modalBanner, ['No se pudo conectar con el servidor. Verifica tu conexión.']);
  }finally{
    ponerBotonCargando(btnGuardar, false);
  }
});

/* ============================================================
   9. ELIMINAR (DELETE)
   ============================================================ */
listaPresupuestos.addEventListener('click', async (e) => {
  const boton = e.target.closest('button[data-accion]');
  if(!boton) return;

  const id = boton.dataset.id;
  const presupuesto = presupuestosCache.find(p => String(p.id) === id);
  if(!presupuesto) return;

  if(boton.dataset.accion === 'editar'){
    abrirModal(presupuesto);
    return;
  }

  if(boton.dataset.accion === 'eliminar'){
    const confirmado = confirm(`¿Eliminar el presupuesto de "${presupuesto.nombreCategoria}"? Esta acción no se puede deshacer.`);
    if(!confirmado) return;

    boton.disabled = true;
    try{
      const respuesta = await hacerPeticionAutenticada(`${API_BASE_URL}/presupuestos/${id}`, {
        method: 'DELETE',
      });

      if(!respuesta.ok){
        const mensajes = await parsearErroresBackend(respuesta);
        alert(mensajes[0]);
        boton.disabled = false;
        return;
      }

      cargarLista();

    }catch(error){
      alert('No se pudo conectar con el servidor.');
      boton.disabled = false;
    }
  }
});

/* ============================================================
   10. INICIALIZACIÓN
   ============================================================ */
inicializarFiltroMonthYear();
cargarCatalogoEnSelect(
  `${API_BASE_URL}/categorias`,
  selectCategoria,
  'No se pudieron cargar las categorías'
);
cargarLista();

/* ============================================================
   11. CERRAR SESIÓN
   ============================================================ */
document.getElementById('btnLogout').addEventListener('click', cerrarSesion);