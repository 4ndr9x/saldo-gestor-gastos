
exigirSesion();


/* ============================================================
   1. REFERENCIAS Y ESTADO
   ============================================================ */
const dropzone = document.getElementById('dropzone');
const inputArchivo = document.getElementById('archivoExcel');
const bloqueArchivoSeleccionado = document.getElementById('archivoSeleccionado');
const nombreArchivoEl = document.getElementById('nombreArchivo');
const pesoArchivoEl = document.getElementById('pesoArchivo');
const btnQuitarArchivo = document.getElementById('btnQuitarArchivo');
const btnImportar = document.getElementById('btnImportar');
const banner = document.getElementById('importacionBanner');
const contenedorResumen = document.getElementById('resumenImportacion');

const NOMBRE_CAMPO_FORMDATA = 'archivoExcel';

let archivoActual = null;


/* ============================================================
   2. VALIDACIÓN Y SELECCIÓN DE ARCHIVO
   ============================================================ */
function esArchivoXlsxValido(file){
  return Boolean(file) && file.name.toLowerCase().endsWith('.xlsx');
}

function formatearPeso(bytes){
  if(bytes < 1024) return `${bytes} B`;
  if(bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

function seleccionarArchivo(file){
  ocultarBanner(banner);

  if(!esArchivoXlsxValido(file)){
    mostrarErrores(banner, ['Verifica el formato de tu archivo. Solo se aceptan archivos .xlsx']);
    limpiarSeleccion();
    return;
  }

  archivoActual = file;
  nombreArchivoEl.textContent = file.name;
  pesoArchivoEl.textContent = formatearPeso(file.size);
  bloqueArchivoSeleccionado.style.display = 'flex';
  btnImportar.disabled = false;
}

function limpiarSeleccion(){
  archivoActual = null;
  inputArchivo.value = '';
  bloqueArchivoSeleccionado.style.display = 'none';
  btnImportar.disabled = true;
}

btnQuitarArchivo.addEventListener('click', limpiarSeleccion);

/* ============================================================
   3. DRAG & DROP + CLIC PARA BUSCAR ARCHIVO
   ============================================================ */
dropzone.addEventListener('click', () => inputArchivo.click());
dropzone.addEventListener('keydown', (e) => {
  if(e.key === 'Enter' || e.key === ' '){
    e.preventDefault();
    inputArchivo.click();
  }
});

dropzone.addEventListener('dragover', (e) => {
  e.preventDefault();
  dropzone.classList.add('dropzone-activa');
});

dropzone.addEventListener('dragleave', () => {
  dropzone.classList.remove('dropzone-activa');
});

dropzone.addEventListener('drop', (e) => {
  e.preventDefault();
  dropzone.classList.remove('dropzone-activa');

  const files = e.dataTransfer.files;
  if(files.length > 0){
    seleccionarArchivo(files[0]);
  }
});

inputArchivo.addEventListener('change', () => {
  if(inputArchivo.files.length > 0){
    seleccionarArchivo(inputArchivo.files[0]);
  }
});


/* ============================================================
   4. IMPORTAR
   ============================================================ */
btnImportar.addEventListener('click', async () => {
  if(!archivoActual) return;

  ocultarBanner(banner);
  contenedorResumen.style.display = 'none';
  ponerBotonCargando(btnImportar, true);

  const formData = new FormData();
  formData.append(NOMBRE_CAMPO_FORMDATA, archivoActual);

  try{
    const respuesta = await hacerPeticionAutenticada(
        `${API_BASE_URL}/gastos/importar`,
        {
          method: 'POST',
          body: formData,
        }
    );

    if(!respuesta.ok){
      const errorBackend = await respuesta.json();

      if (
          respuesta.status === 400 &&
          Array.isArray(errorBackend.detalles) &&
          errorBackend.detalles.length > 0
      ){
        pintarResumenImportacionFallida(errorBackend);
        return;
      }

      mostrarErrores(
          banner,
          [errorBackend.mensaje ?? 'Ocurrió un error durante la importación.']
      );

      return;
    }

    const resultado = await respuesta.json();

    pintarResumen(resultado);

    limpiarSeleccion();

  }catch(error){
    mostrarErrores(
        banner,
        ['No se pudo conectar con el servidor. Verifica tu conexión.']
    );
  }finally{
    ponerBotonCargando(btnImportar, false);
  }
});

/* ============================================================
   4b. DESCARGAR PLANTILLA DE EXCEL
   ============================================================ */
async function descargarPlantilla(){
  const boton = document.getElementById('btnDescargarPlantilla');
  ocultarBanner(banner);
  ponerBotonCargando(boton, true);

  try{
    const respuesta = await hacerPeticionAutenticada(`${API_BASE_URL}/gastos/plantilla-importacion`);

    if(!respuesta.ok){
      const mensajes = await parsearErroresBackend(respuesta);
      mostrarErrores(banner, mensajes);
      return;
    }

    const blob = await respuesta.blob();
    const urlTemporal = URL.createObjectURL(blob);

    const enlaceDescarga = document.createElement('a');
    enlaceDescarga.href = urlTemporal;
    enlaceDescarga.download = 'Plantilla_Importacion.xlsx';
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

document.getElementById('btnDescargarPlantilla').addEventListener('click', descargarPlantilla);

/* ============================================================
   5. RENDER DEL RESUMEN
   ============================================================ */
function pintarResumen(resultado){
  const hayErrores = resultado.filasConErrores > 0;

  const listaErrores = hayErrores
    ? `
      <div class="resumen-errores">
        <div class="resumen-errores-titulo">Filas con problemas</div>
        <ul class="resumen-errores-lista">
          ${resultado.detallesErrores.map(err => `<li>${err}</li>`).join('')}
        </ul>
      </div>
    `
    : '';

  contenedorResumen.innerHTML = `
    <div class="resumen-header ${hayErrores ? 'resumen-header--alerta' : 'resumen-header--exito'}">
      <span class="resumen-header-icono">${hayErrores ? '⚠' : '✓'}</span>
      <span class="resumen-header-texto">
        ${hayErrores ? 'Importación completada con errores' : 'Importación completada con éxito'}
      </span>
    </div>

    <div class="resumen-stats">
      <div class="resumen-stat">
        <div class="resumen-stat-valor">${resultado.totalFilasProcesadas}</div>
        <div class="resumen-stat-label">Filas procesadas</div>
      </div>
      <div class="resumen-stat resumen-stat--exito">
        <div class="resumen-stat-valor">${resultado.gastosImportadosExitosamente}</div>
        <div class="resumen-stat-label">Importadas con éxito</div>
      </div>
      <div class="resumen-stat ${hayErrores ? 'resumen-stat--error' : ''}">
        <div class="resumen-stat-valor">${resultado.filasConErrores}</div>
        <div class="resumen-stat-label">Con errores</div>
      </div>
    </div>

    ${listaErrores}
  `;

  contenedorResumen.style.display = 'block';
}

/* ============================================================
   5b. PINTAR BIEN EL RESUMEN DE ERRORES EN LA PAGINA
   ============================================================ */
function pintarResumenImportacionFallida(errorBackend){
  const errores = errorBackend.detalles ?? [];

  contenedorResumen.innerHTML = `
    <div class="resumen-header resumen-header--alerta">
      <span class="resumen-header-icono">⚠</span>

      <span class="resumen-header-texto">
        Importación no completada
      </span>
    </div>

    <div class="resumen-stats">
      <div class="resumen-stat">
        <div class="resumen-stat-valor">
          ${errores.length}
        </div>
        <div class="resumen-stat-label">
          Filas procesadas
        </div>
      </div>

      <div class="resumen-stat resumen-stat--error">
        <div class="resumen-stat-valor">
          ${errores.length}
        </div>
        <div class="resumen-stat-label">
          Filas con errores
        </div>
      </div>
    </div>

    <div class="resumen-errores">
      <div class="resumen-errores-titulo">
        Filas con problemas
      </div>

      <ul class="resumen-errores-lista">
        ${errores
      .map(error => `<li>${error}</li>`)
      .join('')}
      </ul>
    </div>
  `;

  contenedorResumen.style.display = 'block';
}

/* ============================================================
   6. CERRAR SESIÓN
   ============================================================ */
document.getElementById('btnLogout').addEventListener('click', cerrarSesion);