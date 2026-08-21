exigirSesion();

/* ============================================================
   1. CARGA COMPLETA DE GASTOS
   ============================================================ */
let todosLosGastos = [];

async function cargarTodosLosGastos(){
  const contenedor = document.getElementById('tablaGastosContenedor');
  
  const checkboxEliminados = document.getElementById('filtroEliminados');
  const incluirEliminados = checkboxEliminados ? checkboxEliminados.checked : false;

  try{
    const respuesta = await hacerPeticionAutenticada(`${API_BASE_URL}/gastos?incluirEliminados=${incluirEliminados}`);

    if(!respuesta.ok){
      const mensajes = await parsearErroresBackend(respuesta);
      contenedor.innerHTML = `<div class="estado-error">${mensajes[0]}</div>`;
      return;
    }

    const datosCrudos = await respuesta.json();

    todosLosGastos = (Array.isArray(datosCrudos) ? datosCrudos : []).map(g => {
        return {
           ...normalizarGasto(g),
           activo: g.activo
        };
    });

    aplicarFiltrosYRenderizar();

  }catch(error){
    contenedor.innerHTML = `<div class="estado-error">No se pudo conectar con el servidor.</div>`;
  }
}

const filtroEliminados = document.getElementById('filtroEliminados');
if (filtroEliminados) {
    filtroEliminados.addEventListener('change', cargarTodosLosGastos);
}

window.alGuardarGastoExitoso = cargarTodosLosGastos;

cargarTodosLosGastos();
cargarCatalogosGasto();

/* ============================================================
   2. FILTROS
   ============================================================ */
function aplicarFiltrosYRenderizar(){
  const texto = document.getElementById('filtroTexto').value.trim().toLowerCase();
  const categoriaId = document.getElementById('filtroCategoria').value;
  const metodoPagoId = document.getElementById('filtroMetodoPago').value;
  const desde = document.getElementById('filtroDesde').value;
  const hasta = document.getElementById('filtroHasta').value;

  const checkboxEliminados = document.getElementById('filtroEliminados');
  const mostrarEliminados = checkboxEliminados ? checkboxEliminados.checked : false;

  let filtrados = [...todosLosGastos];

if (mostrarEliminados) {
    filtrados = filtrados.filter(g => g.activo === false);
  } else {
    filtrados = filtrados.filter(g => g.activo !== false);
  }

  // Resto de filtros
  if(texto){
    filtrados = filtrados.filter(g =>
      g.concepto.toLowerCase().includes(texto) || g.descripcion.toLowerCase().includes(texto)
    );
  }
  if(categoriaId){
    filtrados = filtrados.filter(g => String(g.categoriaId) === categoriaId);
  }
  if(metodoPagoId){
    filtrados = filtrados.filter(g => String(g.metodoPagoId) === metodoPagoId);
  }
  if(desde){
    filtrados = filtrados.filter(g => g.fecha && g.fecha.slice(0, 10) >= desde);
  }
  if(hasta){
    filtrados = filtrados.filter(g => g.fecha && g.fecha.slice(0, 10) <= hasta);
  }

  filtrados.sort((a, b) => new Date(b.fecha) - new Date(a.fecha));

  pintarTabla(filtrados);

  const contador = document.getElementById('contadorResultados');
  const totalOriginal = todosLosGastos.length;
  if(contador) {
      contador.textContent = filtrados.length === totalOriginal
        ? `${totalOriginal} gasto${totalOriginal === 1 ? '' : 's'} en total`
        : `${filtrados.length} resultados`;
  }
}

['filtroTexto', 'filtroCategoria', 'filtroMetodoPago', 'filtroDesde', 'filtroHasta'].forEach(id => {
  const el = document.getElementById(id);
  if(el) el.addEventListener('input', aplicarFiltrosYRenderizar);
});

const btnLimpiar = document.getElementById('btnLimpiarFiltros');
if(btnLimpiar) {
    btnLimpiar.addEventListener('click', () => {
      document.getElementById('filtroTexto').value = '';
      document.getElementById('filtroCategoria').value = '';
      document.getElementById('filtroMetodoPago').value = '';
      document.getElementById('filtroDesde').value = '';
      document.getElementById('filtroHasta').value = '';
      
      const chkEliminados = document.getElementById('filtroEliminados');
      if(chkEliminados && chkEliminados.checked) {
        chkEliminados.checked = false;
        if (typeof cargarTodosLosGastos === 'function') {
          cargarTodosLosGastos();
        }
      } else {
        aplicarFiltrosYRenderizar();
      }
    });
}

async function cargarFiltrosDeCatalogo(){
  const selectCategoria = document.getElementById('filtroCategoria');
  const selectMetodo = document.getElementById('filtroMetodoPago');

  try{
    const [respCategorias, respMetodos] = await Promise.all([
      hacerPeticionAutenticada(`${API_BASE_URL}/categorias`),
      hacerPeticionAutenticada(`${API_BASE_URL}/metodospago`),
    ]);

    if(respCategorias.ok && selectCategoria){
      const categorias = (await respCategorias.json()).map(normalizarOpcion);
      selectCategoria.innerHTML = `<option value="">Todas</option>` +
        categorias.map(c => `<option value="${c.id}">${c.nombre}</option>`).join('');
    }

    if(respMetodos.ok && selectMetodo){
      const metodos = (await respMetodos.json()).map(normalizarOpcion);
      selectMetodo.innerHTML = `<option value="">Todos</option>` +
        metodos.map(m => `<option value="${m.id}">${m.nombre}</option>`).join('');
    }
  }catch(error){

  }
}

cargarFiltrosDeCatalogo();

/* ============================================================
   3. RENDER DE LA TABLA 
   ============================================================ */
function pintarTabla(gastos) {
  const contenedor = document.getElementById('tablaGastosContenedor');
  if(!contenedor) return;

  if (gastos.length === 0) {
    contenedor.innerHTML = `<div class="estado-vacio">No hay gastos que coincidan con estos filtros.</div>`;
    return;
  }

  const usuario = leerUsuario();
  const monedaBase = usuario?.moneda || 'DOP';

  const filas = gastos.map(g => {
    const notaMoneda = g.moneda && g.moneda !== monedaBase
      ? `<span class="monto-original">${g.montoOriginal.toLocaleString('es-DO', { minimumFractionDigits: 2 })} ${g.moneda}</span>`
      : '';

    const estaEliminado = g.activo === false;
    const filaClase = estaEliminado ? 'gasto-eliminado' : '';
    const opacidad = estaEliminado ? 'opacity: 0.6;' : '';
    const tachado = estaEliminado ? 'text-decoration: line-through;' : '';
    
    const etiquetaEliminado = estaEliminado 
      ? `<span class="categoria-tag" style="background:#FBEAE8; color:#B3261E; margin-top:4px; display:inline-block;">En Papelera</span>` 
      : '';

    const botones = estaEliminado
      ? `<button class="btn-icono" style="color:var(--accent-strong);" title="Restaurar" data-accion="restaurar" data-id="${g.id}">⟲</button>`
      : `<button class="btn-icono" title="Editar" data-accion="editar" data-id="${g.id}">✎</button>
         <button class="btn-icono peligro" title="Eliminar" data-accion="eliminar" data-id="${g.id}">✕</button>`;

    return `
      <tr data-id="${g.id}" class="${filaClase}" style="${opacidad}">
        <td class="concepto-celda">
          <div class="concepto" style="${tachado}">${g.concepto}</div>
          ${g.descripcion ? `<div class="nota">${g.descripcion}</div>` : ''}
          ${etiquetaEliminado}
        </td>
        <td>${formatearFecha(g.fecha)}</td>
        <td><span class="categoria-tag">${g.categoria}</span></td>
        <td>${g.metodoPago}</td>
        <td class="monto-celda alinear-derecha">
          ${formatearMonto(g.montoFinal)}
          ${notaMoneda}
        </td>
        <td>
          <div class="acciones-celda">
            ${botones}
          </div>
        </td>
      </tr>
    `;
  }).join('');

  contenedor.innerHTML = `
    <table class="tabla-gastos">
      <thead>
        <tr>
          <th>Concepto</th>
          <th>Fecha</th>
          <th>Categoría</th>
          <th>Método de pago</th>
          <th class="alinear-derecha">Monto</th>
          <th></th>
        </tr>
      </thead>
      <tbody>${filas}</tbody>
    </table>
  `;
}

/* ============================================================
   4. EDITAR, ELIMINAR Y RESTAURAR 
   ============================================================ */
const tablaContenedor = document.getElementById('tablaGastosContenedor');
if (tablaContenedor) {
    tablaContenedor.addEventListener('click', async (e) => {
      const boton = e.target.closest('button[data-accion]');
      if(!boton) return;

      const id = boton.dataset.id;
      const gasto = todosLosGastos.find(g => String(g.id) === id);
      if(!gasto) return;

      if(boton.dataset.accion === 'editar'){
        abrirModalGasto(gasto);
        return;
      }

      if(boton.dataset.accion === 'eliminar'){
        const confirmado = confirm(`¿Enviar "${gasto.concepto}" a la papelera?`);
        if(!confirmado) return;

        boton.disabled = true;
        try{
          const respuesta = await hacerPeticionAutenticada(`${API_BASE_URL}/gastos/${id}`, {
            method: 'DELETE',
          });

          if(!respuesta.ok){
            const mensajes = await parsearErroresBackend(respuesta);
            alert(mensajes[0]);
            boton.disabled = false;
            return;
          }

          cargarTodosLosGastos();

        }catch(error){
          alert('No se pudo conectar con el servidor.');
          boton.disabled = false;
        }
      }

      if(boton.dataset.accion === 'restaurar'){
        const confirmado = confirm(`¿Quieres restaurar el gasto "${gasto.concepto}"?`);
        if(!confirmado) return;

        boton.disabled = true;
        try{
          const respuesta = await hacerPeticionAutenticada(`${API_BASE_URL}/gastos/${id}/restaurar`, {
            method: 'PUT', 
          });

          if(!respuesta.ok){
            const mensajes = await parsearErroresBackend(respuesta);
            alert(mensajes[0]);
            boton.disabled = false;
            return;
          }

          cargarTodosLosGastos();

        }catch(error){
          alert('No se pudo conectar con el servidor.');
          boton.disabled = false;
        }
      }
    });
}

/* ============================================================
   5. CERRAR SESIÓN
   ============================================================ */
const btnLogout = document.getElementById('btnLogout');
if(btnLogout) {
    btnLogout.addEventListener('click', cerrarSesion);
}