
exigirSesion();

/* ============================================================
   1. CONFIGURACIÓN POR CATÁLOGO
   ============================================================ */
const CATALOGOS = {
  categorias: {
    endpoint: `${API_BASE_URL}/categorias`,
    singular: 'Categoría',
    plural: 'Categorías',
    generoFemenino: true, 
    accionAgregar: 'Agregar categoría',
    gastoCampoId: 'categoriaId'
  },
  metodosPago: {
    endpoint: `${API_BASE_URL}/metodospago`, 
    singular: 'Método de pago',
    plural: 'Métodos de pago',
    generoFemenino: false,
    accionAgregar: 'Agregar método de pago',
    gastoCampoId: 'metodoPagoId'
  },
};

let tabActivo = 'categorias';
let tipoEnEdicion = tabActivo; 
let itemsCache = [];

/* ============================================================
   2. TABS
   ============================================================ */
function cambiarTab(tab){
  tabActivo = tab;

  document.querySelectorAll('.tab-catalogo-btn').forEach(btn => {
    btn.classList.toggle('active', btn.dataset.tab === tab);
  });

  const config = CATALOGOS[tab];
  document.getElementById('tituloCatalogo').textContent = config.plural;
  document.getElementById('btnAbrirModal').querySelector('.btn-label').textContent = `+ ${config.accionAgregar}`;

  cargarLista();
}

document.querySelectorAll('.tab-catalogo-btn').forEach(btn => {
  btn.addEventListener('click', () => cambiarTab(btn.dataset.tab));
});

/* ============================================================
   3. LEER (GET) + CRUCE DE DATOS
   ============================================================ */
async function cargarLista(){
  const banner = document.getElementById('catalogosBanner');
  const contenedor = document.getElementById('tablaCatalogoContenedor');
  ocultarBanner(banner);
  contenedor.innerHTML = `<div class="estado-vacio">Cargando...</div>`;

  const config = CATALOGOS[tabActivo];

  try{
    const [respCatalogo, respGastos] = await Promise.all([
      hacerPeticionAutenticada(config.endpoint),
      hacerPeticionAutenticada(`${API_BASE_URL}/gastos`)
    ]);

    if(!respCatalogo.ok){
      const mensajes = await parsearErroresBackend(respCatalogo);
      contenedor.innerHTML = `<div class="estado-error">${mensajes[0]}</div>`;
      return;
    }

    const crudos = await respCatalogo.json();
    const gastos = respGastos.ok ? await respGastos.json() : [];

    const conteos = {};
    gastos.forEach(g => {
      const idVinculado = g[config.gastoCampoId];
      if(idVinculado) {
        conteos[idVinculado] = (conteos[idVinculado] ?? 0) + 1;
      }
    });

    itemsCache = (Array.isArray(crudos) ? crudos : []).map(it => ({ 
      id: it.id, 
      nombre: it.nombre,
      cantidadGastos: conteos[it.id] ?? 0
    }));
    
    pintarTabla();

  }catch(error){
    contenedor.innerHTML = `<div class="estado-error">No se pudo conectar con el servidor.</div>`;
  }
}

function pintarTabla(){
  const contenedor = document.getElementById('tablaCatalogoContenedor');
  const config = CATALOGOS[tabActivo];

  if(itemsCache.length === 0){
    contenedor.innerHTML = `<div class="estado-vacio">Todavía no tienes ${config.plural.toLowerCase()}. ¡Agrega el primero!</div>`;
    return;
  }

  const filas = itemsCache.map(it => `
    <tr data-id="${it.id}">
      <td>
        <div class="nombre-celda">
          <span class="catalogo-punto"></span>
          ${it.nombre}
        </div>
      </td>
      <td class="conteo-celda">${it.cantidadGastos} gasto${it.cantidadGastos === 1 ? '' : 's'}</td>
      <td>
        <div class="acciones-celda">
          <button class="btn-icono" title="Editar" data-accion="editar" data-id="${it.id}">✎</button>
          <button class="btn-icono peligro" title="Eliminar" data-accion="eliminar" data-id="${it.id}">✕</button>
        </div>
      </td>
    </tr>
  `).join('');

  contenedor.innerHTML = `
    <table class="tabla-catalogo">
      <thead>
        <tr>
          <th>Nombre</th>
          <th>Uso</th>
          <th></th>
        </tr>
      </thead>
      <tbody>${filas}</tbody>
    </table>
  `;
}

cargarLista();

/* ============================================================
   4. MODAL: AGREGAR / EDITAR
   ============================================================ */
const modalOverlay = document.getElementById('modalOverlay');
const formCatalogo = document.getElementById('formCatalogo');
const modalTitulo = document.getElementById('modalTitulo');
const btnGuardar = document.getElementById('btnGuardarCatalogo');

function abrirModal(item){
  tipoEnEdicion = tabActivo;
  const config = CATALOGOS[tipoEnEdicion];

  modalOverlay.classList.add('show');

  if(item){
    modalTitulo.textContent = `Editar ${config.singular.toLowerCase()}`;
    btnGuardar.querySelector('.btn-label').textContent = 'Guardar cambios';
    document.getElementById('catalogoId').value = item.id;
    document.getElementById('catalogoNombre').value = item.nombre;
  } else {
    modalTitulo.textContent = config.accionAgregar;
    btnGuardar.querySelector('.btn-label').textContent = 'Guardar';
    document.getElementById('catalogoId').value = '';
  }

  document.getElementById('catalogoNombre').focus();
}

function cerrarModal(){
  modalOverlay.classList.remove('show');
  formCatalogo.reset();
  document.getElementById('catalogoId').value = '';
  ocultarBanner(document.getElementById('modalBanner'));
  limpiarErroresDeCampos(formCatalogo);
}

document.getElementById('btnAbrirModal').addEventListener('click', () => abrirModal());
document.getElementById('btnCancelarModal').addEventListener('click', cerrarModal);
modalOverlay.addEventListener('click', (e) => {
  if(e.target === modalOverlay) cerrarModal();
});

/* ============================================================
   5. CREAR (POST) / EDITAR (PUT)
   ============================================================ */
formCatalogo.addEventListener('submit', async (e) => {
  e.preventDefault();

  const banner = document.getElementById('modalBanner');
  ocultarBanner(banner);
  limpiarErroresDeCampos(formCatalogo);

  const id = document.getElementById('catalogoId').value;
  const nombre = document.getElementById('catalogoNombre').value.trim();

  if(!nombre){
    marcarCampoInvalido('catalogoNombre', 'errCatalogoNombre', 'El nombre es obligatorio.');
    mostrarErrores(banner, ['El nombre es obligatorio.']);
    return;
  }

  const config = CATALOGOS[tipoEnEdicion];
  const esEdicion = Boolean(id);

  ponerBotonCargando(btnGuardar, true);

  try{
    const respuesta = await hacerPeticionAutenticada(
      esEdicion ? `${config.endpoint}/${id}` : config.endpoint,
      {
        method: esEdicion ? 'PUT' : 'POST',
        body: JSON.stringify({ nombre }),
      }
    );

    if(!respuesta.ok){
      const mensajes = await parsearErroresBackend(respuesta);
      mostrarErrores(banner, mensajes);
      resaltarCamposPorMensaje(mensajes, [
        { palabras: ['nombre'], inputId: 'catalogoNombre', errorId: 'errCatalogoNombre' },
      ]);
      return;
    }

    cerrarModal();

    const terminacion = config.generoFemenino ? 'a' : 'o';
    mostrarExito(
      document.getElementById('catalogosBanner'),
      `${config.singular} guardad${terminacion} correctamente.`
    );

    if(tipoEnEdicion === tabActivo){
      cargarLista();
    }

  }catch(error){
    mostrarErrores(banner, ['No se pudo conectar con el servidor. Verifica tu conexión.']);
  }finally{
    ponerBotonCargando(btnGuardar, false);
  }
});

/* ============================================================
   6. ELIMINAR (DELETE) — Con advertencia inteligente
   ============================================================ */
document.getElementById('tablaCatalogoContenedor').addEventListener('click', async (e) => {
  const boton = e.target.closest('button[data-accion]');
  if(!boton) return;

  const id = boton.dataset.id;
  const item = itemsCache.find(it => String(it.id) === id);
  if(!item) return;

  if(boton.dataset.accion === 'editar'){
    abrirModal(item);
    return;
  }

  if(boton.dataset.accion === 'eliminar'){
    const config = CATALOGOS[tabActivo];
    
    const mensaje = item.cantidadGastos > 0
      ? `"${item.nombre}" tiene ${item.cantidadGastos} gasto(s) asociados. ¿Eliminar ${config.generoFemenino ? 'esta' : 'este'} ${config.singular.toLowerCase()} de todas formas?`
      : `¿Eliminar "${item.nombre}"? Esta acción no se puede deshacer.`;

    const confirmado = confirm(mensaje);
    if(!confirmado) return;

    boton.disabled = true;
    try{
      const respuesta = await hacerPeticionAutenticada(`${config.endpoint}/${id}`, {
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
   7. CERRAR SESIÓN
   ============================================================ */
document.getElementById('btnLogout').addEventListener('click', cerrarSesion);