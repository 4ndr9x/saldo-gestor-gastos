
exigirSesion();


/* ============================================================
   1. SELECTORES DE MES / AÑO (por defecto: mes y año actuales)
   ============================================================ */
const selectorMes = document.getElementById('selectorMes');
const selectorAnio = document.getElementById('selectorAnio');

function inicializarSelectoresDePeriodo(){
  const hoy = new Date();
  const mesActual = hoy.getMonth() + 1;
  const anioActual = hoy.getFullYear();

  selectorMes.value = String(mesActual);

  const anioInicio = anioActual - 3;
  let opcionesAnio = '';
  for(let anio = anioActual; anio >= anioInicio; anio--){
    opcionesAnio += `<option value="${anio}">${anio}</option>`;
  }
  selectorAnio.innerHTML = opcionesAnio;
  selectorAnio.value = String(anioActual);
}

inicializarSelectoresDePeriodo();

const NOMBRES_MESES = [
  'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
  'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre',
];

function actualizarSubtitulo(){
  const mes = Number(selectorMes.value);
  const anio = selectorAnio.value;
  document.getElementById('subtituloPeriodo').textContent = `${NOMBRES_MESES[mes - 1]} ${anio}`;
}


/* ============================================================
   2. FORMA REAL DE GET /api/reportes/mensual
   ============================================================ */
function normalizarReporte(crudo){
  const topCategorias = Array.isArray(crudo?.topCategorias) ? crudo.topCategorias : [];

  return {
    totalGastado: Number(crudo?.totalGastado ?? 0),
    totalMesAnterior: Number(crudo?.totalMesAnterior ?? 0),
    diferencia: Number(crudo?.diferencia ?? 0),
    mensajeComparacion: crudo?.mensajeComparacion ?? '',
    moneda: crudo?.moneda ?? 'DOP', // 👈 1. Rescatamos la moneda del backend
    categoriaMayor: topCategorias.length > 0 ? {
      nombre: topCategorias[0].nombreCategoria,
      monto: Number(topCategorias[0].totalGastado ?? 0),
    } : null,
    desglose: topCategorias.map(c => ({
      nombre: c.nombreCategoria,
      total: Number(c.totalGastado ?? 0),
    })),
  };
}

/* ============================================================
   3. CARGA Y RENDER DEL REPORTE
   ============================================================ */
async function cargarReporte(){
  const banner = document.getElementById('reportesBanner');
  const contenedorDesglose = document.getElementById('desgloseContenedor');
  ocultarBanner(banner);

  actualizarSubtitulo();
  document.querySelectorAll('.summary-card .value').forEach(el => el.classList.add('skeleton'));
  contenedorDesglose.innerHTML = `<div class="estado-vacio">Cargando el reporte...</div>`;

  const mes = selectorMes.value;
  const anio = selectorAnio.value;

  try{
    const respuesta = await hacerPeticionAutenticada(
      `${API_BASE_URL}/Reportes/mensual?month=${mes}&year=${anio}`
    );

    if(!respuesta.ok){
      const mensajes = await parsearErroresBackend(respuesta);
      mostrarErrores(banner, mensajes);
      contenedorDesglose.innerHTML = `<div class="estado-error">${mensajes[0]}</div>`;
      return;
    }

    const crudo = await respuesta.json();

    const reporte = normalizarReporte(crudo);

    pintarTarjetas(reporte);
    pintarDesglose(reporte);

  }catch(error){
    contenedorDesglose.innerHTML = `<div class="estado-error">No se pudo conectar con el servidor.</div>`;
  }
}

function pintarTarjetas(reporte){
  document.getElementById('valTotalGastado').textContent = formatearMonto(reporte.totalGastado, reporte.moneda);
  document.getElementById('valMesAnterior').textContent = formatearMonto(reporte.totalMesAnterior, reporte.moneda);

  const diferenciaEl = document.getElementById('valDiferencia');
  const signo = reporte.diferencia > 0 ? '+' : '';
  diferenciaEl.textContent = `${signo}${formatearMonto(reporte.diferencia, reporte.moneda)}`;

  diferenciaEl.classList.toggle('valor-negativo', reporte.diferencia > 0);
  diferenciaEl.classList.toggle('valor-positivo', reporte.diferencia <= 0);

  const categoriaEl = document.getElementById('valCategoriaMayor');
  categoriaEl.textContent = reporte.categoriaMayor
    ? `${reporte.categoriaMayor.nombre} (${formatearMonto(reporte.categoriaMayor.monto, reporte.moneda)})`
    : 'Sin datos';

  document.querySelectorAll('.summary-card .value').forEach(el => el.classList.remove('skeleton'));

  const callout = document.getElementById('comparacionCallout');
  if(reporte.mensajeComparacion){
    callout.textContent = reporte.mensajeComparacion;
    callout.style.display = 'block';
  } else {
    callout.style.display = 'none';
  }
}

function pintarDesglose(reporte){
  const contenedor = document.getElementById('desgloseContenedor');

  if(reporte.desglose.length === 0){
    contenedor.innerHTML = `<div class="estado-vacio">No hay gastos registrados en este periodo.</div>`;
    return;
  }

  const montoMaximo = Math.max(...reporte.desglose.map(c => c.total), 1);

  contenedor.innerHTML = reporte.desglose
    .sort((a, b) => b.total - a.total)
    .map(c => {
      const porcentajeAncho = Math.round((c.total / montoMaximo) * 100);
      return `
        <div class="desglose-fila">
          <div class="desglose-encabezado">
            <span class="desglose-nombre">${c.nombre}</span>
            <span class="desglose-monto">${formatearMonto(c.total, reporte.moneda)}</span>
          </div>
          <div class="desglose-barra-fondo">
            <div class="desglose-barra-relleno" style="width:${porcentajeAncho}%;"></div>
          </div>
        </div>
      `;
    }).join('');
}

selectorMes.addEventListener('change', cargarReporte);
selectorAnio.addEventListener('change', cargarReporte);

cargarReporte();


/* ============================================================
   4. EXPORTACIÓN (TXT / Excel) — descarga autenticada vía blob
============================================= */
async function exportarReporte(tipo){
  // tipo: 'txt' | 'excel' | 'json'
  const extensionesPorTipo = { txt: 'txt', excel: 'xlsx', json: 'json' };
  const botonesPorTipo = {
    txt: 'btnExportarTxt',
    excel: 'btnExportarExcel',
    json: 'btnExportarJson',
  };

  const extension = extensionesPorTipo[tipo];
  const boton = document.getElementById(botonesPorTipo[tipo]);

  const mes = selectorMes.value;
  const anio = selectorAnio.value;
  const banner = document.getElementById('reportesBanner');
  ocultarBanner(banner);

  ponerBotonCargando(boton, true);

  try{
    const respuesta = await hacerPeticionAutenticada(
      `${API_BASE_URL}/Reportes/mensual/exportar/${tipo}?month=${mes}&year=${anio}`
    );

    if(!respuesta.ok){
      const mensajes = await parsearErroresBackend(respuesta);
      mostrarErrores(banner, mensajes);
      return;
    }

    const blob = await respuesta.blob();
    const urlTemporal = URL.createObjectURL(blob);

    const nombreArchivo = `Reporte_${NOMBRES_MESES[Number(mes) - 1]}_${anio}.${extension}`;

    const enlaceDescarga = document.createElement('a');
    enlaceDescarga.href = urlTemporal;
    enlaceDescarga.download = nombreArchivo;
    document.body.appendChild(enlaceDescarga);
    enlaceDescarga.click();
    document.body.removeChild(enlaceDescarga);

    URL.revokeObjectURL(urlTemporal);

  }catch(error){
    mostrarErrores(banner, ['No se pudo conectar con el servidor. Verifica tu conexión.']);
  }finally{
    ponerBotonCargando(boton, false);
  }
}

document.getElementById('btnExportarTxt').addEventListener('click', () => exportarReporte('txt'));
document.getElementById('btnExportarExcel').addEventListener('click', () => exportarReporte('excel'));
document.getElementById('btnExportarJson').addEventListener('click', () => exportarReporte('json'));


/* ============================================================
   5. CERRAR SESIÓN
   ============================================================ */
document.getElementById('btnLogout').addEventListener('click', cerrarSesion);