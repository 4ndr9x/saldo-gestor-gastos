exigirSesion();

let miGrafico = null;
let gastosCache = [];

/* ============================================================
   1. SALUDO Y FECHA DE HOY
   ============================================================ */
(function pintarEncabezado(){
  const usuario = leerUsuario();
  const primerNombre = usuario?.nombreUsuario?.split(' ')[0] ?? '';
  document.getElementById('saludoUsuario').textContent = primerNombre
    ? `Hola, ${primerNombre}`
    : 'Hola ';

  const hoy = new Date();
  document.getElementById('fechaHoy').textContent = hoy.toLocaleDateString('es-DO', {
    weekday: 'long', day: 'numeric', month: 'long',
  });
})();


/* ============================================================
   2. CARGA DE GASTOS (GET /api/gastos)
   ============================================================ */
async function cargarGastos(){
  const contenedor = document.getElementById('listaGastos');
  if(!contenedor) return;

  try{
    const respuesta = await hacerPeticionAutenticada(`${API_BASE_URL}/gastos`);

    if(!respuesta.ok){
      const mensajes = await parsearErroresBackend(respuesta);
      contenedor.innerHTML = `<div class="estado-error">${mensajes[0]}</div>`;
      return;
    }

    const datosCrudos = await respuesta.json();
    
    gastosCache = (Array.isArray(datosCrudos) ? datosCrudos : []).map(normalizarGasto);

    pintarLista(gastosCache);
    actualizarTarjetasSecundarias(gastosCache);
    pintarGrafico(gastosCache);

  }catch(error){
    contenedor.innerHTML = `<div class="estado-error">No se pudo conectar con el servidor.</div>`;
  }
}

/* ============================================================
   3. PINTADO DE INTERFAZ (Tabla, Tarjetas y Gráfico)
   ============================================================ */
function pintarLista(gastos){
  const contenedor = document.getElementById('listaGastos');

  if(!gastos || gastos.length === 0){
    contenedor.innerHTML = `<div class="estado-vacio">Todavía no tienes gastos registrados. ¡Agrega el primero!</div>`;
    return;
  }

  const recientes = [...gastos]
    .sort((a, b) => new Date(b.fecha) - new Date(a.fecha))
    .slice(0, 8);

  contenedor.innerHTML = recientes.map(g => {
    const notaMoneda = g.moneda && g.moneda !== 'DOP'
      ? ` · ${g.montoOriginal.toLocaleString('es-DO', { minimumFractionDigits: 2 })} ${g.moneda}`
      : '';

    const metaLinea = [formatearFecha(g.fecha), g.metodoPago].filter(Boolean).join(' · ') + notaMoneda;

    return `
      <div class="gasto-row">
        <div class="gasto-info">
          <div class="descripcion">${g.concepto}</div>
          <div class="meta">${metaLinea}</div>
        </div>
        <span class="categoria-tag">${g.categoria}</span>
        <span class="gasto-monto">${formatearMonto(g.montoFinal)}</span>
      </div>
    `;
  }).join('');
}

function actualizarTarjetasSecundarias(gastos) {
  const hoy = new Date();
  
  const delMes = gastos.filter(g => {
    if(!g.fecha) return false;
    const f = new Date(g.fecha);
    return f.getMonth() === hoy.getMonth() && f.getFullYear() === hoy.getFullYear();
  });

  const total = delMes.reduce((suma, g) => suma + g.montoFinal, 0);
  const diaDelMes = hoy.getDate();
  const promedio = diaDelMes > 0 ? total / diaDelMes : 0;
  const masAlto = delMes.reduce((max, g) => Math.max(max, g.montoFinal), 0);

  const elPromedio = document.getElementById('valPromedio');
  const elMasAlto = document.getElementById('valMasAlto');
  const elCantidad = document.getElementById('valCantidad');

  if(elPromedio) elPromedio.textContent = formatearMonto(promedio);
  if(elMasAlto) elMasAlto.textContent = formatearMonto(masAlto);
  if(elCantidad) elCantidad.textContent = delMes.length;
}

function pintarGrafico(gastos) {
  const canvas = document.getElementById('graficoCategorias');
  if (!canvas) return;

  const hoy = new Date();
  const delMes = gastos.filter(g => {
    if (!g.fecha) return false;
    const f = new Date(g.fecha);
    return f.getMonth() === hoy.getMonth() && f.getFullYear() === hoy.getFullYear();
  });

  const contenedorPadre = canvas.parentElement;

  if (delMes.length === 0) {
    if (miGrafico) {
      miGrafico.destroy();
      miGrafico = null;
    }
    contenedorPadre.innerHTML = `<canvas id="graficoCategorias"></canvas><div style="position:absolute; inset:0; display:flex; align-items:center; justify-content:center; color:var(--text-muted); font-size:13px; background:var(--paper-card);">Sin datos para graficar este mes</div>`;
    return;
  }

  const totalesPorCategoria = {};
  delMes.forEach(g => {
    const cat = g.categoria || 'Otros';
    totalesPorCategoria[cat] = (totalesPorCategoria[cat] || 0) + g.montoFinal;
  });

  const labels = Object.keys(totalesPorCategoria);
  const data = Object.values(totalesPorCategoria);

  if (miGrafico) miGrafico.destroy();

  miGrafico = new Chart(canvas, {
    type: 'doughnut',
    data: {
      labels: labels,
      datasets: [{
        data: data,
        backgroundColor: ['#2F7A5D', '#B98B2E', '#5B6B62', '#7FBFA0', '#14231C', '#AEBAB1'],
        borderWidth: 0
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { position: 'bottom' } },
      cutout: '70%'
    }
  });
}


/* ============================================================
   4. REPORTE MENSUAL (GET /api/reportes/mensual)
   ============================================================ */
async function cargarReporteMensual() {
  const hoy = new Date();
  const month = hoy.getMonth() + 1;
  const year = hoy.getFullYear();

  try {
    const respuesta = await hacerPeticionAutenticada(`${API_BASE_URL}/reportes/mensual?month=${month}&year=${year}`);
    if (!respuesta.ok) return;

    const reporte = await respuesta.json();

    const totalEl = document.getElementById('valTotalMes');
    const comparacionEl = document.getElementById('valComparacion');

    if (totalEl) totalEl.textContent = formatearMonto(reporte.totalGastado);
    if (comparacionEl) comparacionEl.textContent = reporte.mensajeComparacion;

    document.querySelectorAll('.summary-card .value').forEach(el => el.classList.remove('skeleton'));

  } catch (error) {
    console.error("Error al cargar el reporte mensual", error);
  }
}

/* ============================================================
   5. ALERTAS DE PRESUPUESTOS (GET /api/presupuestos/resumen)
   ============================================================ */
async function cargarAlertasPresupuesto() {
  const hoy = new Date();
  const month = hoy.getMonth() + 1;
  const year = hoy.getFullYear();
  const contenedorAlertas = document.getElementById('contenedorAlertas');

  if (!contenedorAlertas) return;

  try {
    const respuesta = await hacerPeticionAutenticada(`${API_BASE_URL}/presupuestos/resumen?month=${month}&year=${year}&soloExcedidos=true`);
    if (!respuesta.ok) return;

    const presupuestosPeligro = await respuesta.json();

    if (presupuestosPeligro.length === 0) {
       contenedorAlertas.innerHTML = ''; 
       return;
    }

    contenedorAlertas.innerHTML = presupuestosPeligro.map(p => {
      const claseColor = p.porcentajeConsumido >= 100 ? 'alerta-roja' : 'alerta-naranja';
      return `
        <div class="alerta-presupuesto ${claseColor}" style="padding: 10px; margin-bottom: 10px; border-radius: 5px;">
          <strong>¡Atención!</strong> Tu presupuesto de <b>${p.nombreCategoria}</b> está en nivel ${p.nivelAlerta}. 
          Has gastado ${formatearMonto(p.montoGastado)} de ${formatearMonto(p.montoPresupuestado)} (${p.porcentajeConsumido}%).
        </div>
      `;
    }).join('');

  } catch (error) {
    console.error("Error al cargar alertas de presupuesto", error);
  }
}

/* ============================================================
   6. INICIALIZACIÓN Y EVENTOS
   ============================================================ */

cargarCatalogosGasto();

window.alGuardarGastoExitoso = function() {
  cargarGastos();
  cargarReporteMensual();
  cargarAlertasPresupuesto();
};

document.getElementById('btnLogout')?.addEventListener('click', cerrarSesion);

cargarGastos();
cargarReporteMensual();
cargarAlertasPresupuesto();