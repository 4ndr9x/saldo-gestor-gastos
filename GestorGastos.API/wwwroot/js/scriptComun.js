

const API_BASE_URL = 'http://localhost:5031/api';

const TOKEN_KEY = 'gestor_gastos_token';
const USER_KEY  = 'gestor_gastos_usuario';


/* ============================================================
   1. GESTIÓN DEL TOKEN JWT
   ============================================================ */

function guardarToken(token, recordar, usuario){
  const storage = recordar ? localStorage : sessionStorage;
  storage.setItem(TOKEN_KEY, token);
  if(usuario) storage.setItem(USER_KEY, JSON.stringify(usuario));
}

function leerToken(){
  return localStorage.getItem(TOKEN_KEY) || sessionStorage.getItem(TOKEN_KEY);
}

function leerUsuario(){
  const crudo = localStorage.getItem(USER_KEY) || sessionStorage.getItem(USER_KEY);
  if(!crudo) return null;
  try{
    return JSON.parse(crudo);
  }catch(e){
    return null;
  }
}

function borrarToken(){
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
  sessionStorage.removeItem(TOKEN_KEY);
  sessionStorage.removeItem(USER_KEY);
}

function decodificarPayloadJWT(token){
  try{
    const payloadBase64 = token.split('.')[1];
    const payloadJson = atob(payloadBase64.replace(/-/g, '+').replace(/_/g, '/'));
    return JSON.parse(payloadJson);
  }catch(e){
    return null;
  }
}

function tokenExpirado(token){
  const payload = decodificarPayloadJWT(token);
  if(!payload?.exp) return false; // si no trae "exp", no podemos saberlo aquí
  const ahoraEnSegundos = Math.floor(Date.now() / 1000);
  return payload.exp < ahoraEnSegundos;
}

function cerrarSesion(){
  borrarToken();
  window.location.href = 'login.html';
}

function exigirSesion(){
  const token = leerToken();
  if(!token || tokenExpirado(token)){
    borrarToken();
    window.location.replace('login.html');
  }
}


/* ============================================================
   2. PETICIÓN AUTENTICADA GENÉRICA
   ============================================================ */
async function hacerPeticionAutenticada(url, options = {}){
  const token = leerToken();

  if(!token || tokenExpirado(token)){
    borrarToken();
    window.location.href = 'login.html';
    throw new Error('Sesión no válida o expirada.');
  }

  const misHeaders = {
    'Authorization': `Bearer ${token}`,
    ...options.headers
  };

  if (!(options.body instanceof FormData) && !misHeaders['Content-Type']) {
    misHeaders['Content-Type'] = 'application/json';
  }

  const respuesta = await fetch(url, {
    ...options,
    headers: misHeaders,
  });

  if(respuesta.status === 401){
    borrarToken();
    window.location.href = 'login.html';
    throw new Error('Sesión expirada. Vuelve a iniciar sesión.');
  }

  return respuesta;
}

/* ============================================================
   3. PARSEO DE ERRORES ESTANDARIZADOS DEL BACKEND
   ============================================================ */
async function parsearErroresBackend(response) {
  const textoCrudo = await response.text();

  if (!textoCrudo || textoCrudo.trim() === '') {
      return [`Error del servidor (código ${response.status}) sin detalles.`];
  }

  let cuerpo = null;
  try {
      cuerpo = JSON.parse(textoCrudo);
      console.log("Respuesta extraida del backend:", cuerpo);
  } catch (e) {
      return [textoCrudo];
  }

  if (cuerpo?.requestId || cuerpo?.traceId) {
    console.error(`[API] ID de Trazabilidad: ${cuerpo.requestId || cuerpo.traceId}`);
  }

  if (Array.isArray(cuerpo?.detalles) && cuerpo.detalles.length > 0) {
    return cuerpo.detalles;
  }
  
  if (typeof cuerpo?.mensaje === 'string' && cuerpo.mensaje.length > 0) {
    return [cuerpo.mensaje];
  }

  if (cuerpo?.errors && typeof cuerpo.errors === 'object') {
    let validacionesNativas = [];
    for (const campo in cuerpo.errors) {
      if (Array.isArray(cuerpo.errors[campo])) {
        validacionesNativas = validacionesNativas.concat(cuerpo.errors[campo]);
      }
    }
    if (validacionesNativas.length > 0) return validacionesNativas;
  }

  if (typeof cuerpo?.title === 'string' && cuerpo.title.length > 0) {
    return [cuerpo.title];
  }

  return [`Ocurrió un error (código ${response.status}). Intenta de nuevo.`];
}


/* ============================================================
   4. UTILIDADES DE UI: BANNERS DE ERROR/ÉXITO Y CAMPOS INVÁLIDOS
   ============================================================ */

function mostrarErrores(bannerEl, mensajes){
  bannerEl.classList.remove('banner-success');
  bannerEl.classList.add('show');

  if(mensajes.length === 1){
    bannerEl.innerHTML = mensajes[0];
  } else {
    const items = mensajes.map(m => `<li>${m}</li>`).join('');
    bannerEl.innerHTML = `<ul>${items}</ul>`;
  }
}

function mostrarExito(bannerEl, mensaje){
  bannerEl.classList.add('show', 'banner-success');
  bannerEl.textContent = mensaje;
}

function ocultarBanner(bannerEl){
  bannerEl.classList.remove('show', 'banner-success');
  bannerEl.innerHTML = '';
}

function limpiarErroresDeCampos(formEl){
  formEl.querySelectorAll('.field-error').forEach(el => el.textContent = '');
  formEl.querySelectorAll('input[aria-invalid="true"], select[aria-invalid="true"]').forEach(el => el.removeAttribute('aria-invalid'));
}

function marcarCampoInvalido(inputId, errorId, mensaje){
  const input = document.getElementById(inputId);
  const errorEl = document.getElementById(errorId);
  if(input) input.setAttribute('aria-invalid', 'true');
  if(errorEl) errorEl.textContent = mensaje;
}

function resaltarCamposPorMensaje(mensajes, mapeo){
  mensajes.forEach(mensaje => {
    const texto = mensaje.toLowerCase();
    const coincidencia = mapeo.find(m => m.palabras.some(p => texto.includes(p)));
    if(coincidencia){
      marcarCampoInvalido(coincidencia.inputId, coincidencia.errorId, mensaje);
    }
  });
}

function ponerBotonCargando(btnEl, cargando){
  btnEl.disabled = cargando;
  btnEl.classList.toggle('loading', cargando);
}

document.addEventListener('DOMContentLoaded', () => {
  document.querySelectorAll('.field input, .field select').forEach(campo => {
    campo.addEventListener('input', () => {
      campo.removeAttribute('aria-invalid');
      const errorEl = document.getElementById('err' + campo.id.charAt(0).toUpperCase() + campo.id.slice(1));
      if(errorEl) errorEl.textContent = '';
    });
  });
});

/* ============================================================
   5. DOMINIO "GASTOS": NORMALIZACIÓN, FORMATO Y CATÁLOGOS
   ============================================================ */
function normalizarGasto(gastoCrudo){
  return {
    id: gastoCrudo.id,
    concepto: gastoCrudo.concepto ?? 'Sin concepto',
    descripcion: gastoCrudo.descripcion ?? '',
    categoriaId: gastoCrudo.categoriaId ?? null,
    categoria: gastoCrudo.categoria ?? 'Otro',
    metodoPagoId: gastoCrudo.metodoPagoId ?? null,
    metodoPago: gastoCrudo.metodoPago ?? '',
    montoFinal: Number(gastoCrudo.montoFinal ?? 0),
    montoOriginal: Number(gastoCrudo.montoOriginal ?? 0),
    moneda: gastoCrudo.moneda ?? 'DOP',
    fecha: gastoCrudo.fecha ?? null,
  };
}

const SIMBOLOS_MONEDA = {
  'DOP': 'RD$',
  'USD': 'US$',
  'EUR': '€',
  'AUD': 'A$',
  'MXN': 'MX$',
  'COP': 'COL$',
  'ARS': 'AR$',
  'GBP': '£',
  'CAD': 'C$'
};

function formatearMonto(numero, monedaEspecifica = null){
  const usuario = leerUsuario();
  const monedaIso = monedaEspecifica || usuario?.moneda || 'DOP';
  const simbolo = SIMBOLOS_MONEDA[monedaIso] || monedaIso;

  return `${simbolo} ${numero.toLocaleString('es-DO', { minimumFractionDigits: 2 })}`;
}

function formatearFecha(fechaISO){
  if(!fechaISO) return '';
  const fecha = new Date(fechaISO);
  return fecha.toLocaleDateString('es-DO', { day: 'numeric', month: 'short' });
}

function normalizarOpcion(item){
  return { id: item.id, nombre: item.nombre };
}

async function cargarCatalogoEnSelect(url, selectEl, mensajeError){
  if(!selectEl) return;
  try{
    const respuesta = await hacerPeticionAutenticada(url);

    if(!respuesta.ok){
      selectEl.innerHTML = `<option value="">${mensajeError}</option>`;
      return;
    }

    const datosCrudos = await respuesta.json();
    const opciones = (Array.isArray(datosCrudos) ? datosCrudos : []).map(normalizarOpcion);

    if(opciones.length === 0){
      selectEl.innerHTML = `<option value="">Sin opciones disponibles</option>`;
      return;
    }

    selectEl.innerHTML = `<option value="">Selecciona...</option>` + opciones
      .map(op => `<option value="${op.id}">${op.nombre}</option>`)
      .join('');

  }catch(error){
    selectEl.innerHTML = `<option value="">${mensajeError}</option>`;
  }
}

function cargarCatalogosGasto(){
  cargarCatalogoEnSelect(
    `${API_BASE_URL}/categorias`,
    document.getElementById('gastoCategoria'),
    'No se pudieron cargar las categorías'
  );
  cargarCatalogoEnSelect(
    `${API_BASE_URL}/metodospago`,
    document.getElementById('gastoMetodoPago'),
    'No se pudieron cargar los métodos de pago'
  );
}

/* ============================================================
   6. MODAL COMPARTIDO: AGREGAR / EDITAR GASTO
   ============================================================ */
window.alGuardarGastoExitoso = function(){};

function _elementosModalGasto(){
  return {
    overlay: document.getElementById('modalOverlay'),
    form: document.getElementById('formGasto'),
    banner: document.getElementById('gastoBanner'),
    titulo: document.getElementById('modalTitulo'),
    btnGuardar: document.getElementById('btnGuardarGasto'),
  };
}

function abrirModalGasto(gastoParaEditar){
  const el = _elementosModalGasto();
  el.overlay.classList.add('show');

  if(gastoParaEditar){
    el.titulo.textContent = 'Editar gasto';
    el.btnGuardar.querySelector('.btn-label').textContent = 'Guardar cambios';
    document.getElementById('gastoId').value = gastoParaEditar.id;
    document.getElementById('gastoConcepto').value = gastoParaEditar.concepto;
    document.getElementById('gastoDescripcion').value = gastoParaEditar.descripcion;
    document.getElementById('gastoCategoria').value = gastoParaEditar.categoriaId ?? '';
    document.getElementById('gastoMetodoPago').value = gastoParaEditar.metodoPagoId ?? '';
    document.getElementById('gastoMonto').value = gastoParaEditar.montoOriginal;
    document.getElementById('gastoMoneda').value = gastoParaEditar.moneda;
    document.getElementById('gastoFecha').value = (gastoParaEditar.fecha ?? '').slice(0, 16);
  } else {
    el.titulo.textContent = 'Agregar gasto';
    el.btnGuardar.querySelector('.btn-label').textContent = 'Guardar';
    document.getElementById('gastoId').value = '';
    const ahora = new Date();
    ahora.setMinutes(ahora.getMinutes() - ahora.getTimezoneOffset());
    document.getElementById('gastoFecha').value = ahora.toISOString().slice(0, 16);
  }
}

function cerrarModalGasto(){
  const el = _elementosModalGasto();
  el.overlay.classList.remove('show');
  el.form.reset();
  document.getElementById('gastoId').value = '';
  ocultarBanner(el.banner);
  limpiarErroresDeCampos(el.form);
}

document.addEventListener('DOMContentLoaded', () => {
  const el = _elementosModalGasto();
  if(!el.overlay || !el.form) return;

  document.getElementById('btnAbrirModal')?.addEventListener('click', () => abrirModalGasto());
  document.getElementById('btnCancelarModal').addEventListener('click', cerrarModalGasto);
  el.overlay.addEventListener('click', (e) => {
    if(e.target === el.overlay) cerrarModalGasto();
  });

  el.form.addEventListener('submit', async (e) => {
    e.preventDefault();

    ocultarBanner(el.banner);
    limpiarErroresDeCampos(el.form);

    const gastoId = document.getElementById('gastoId').value;
    const concepto = document.getElementById('gastoConcepto').value.trim();
    const descripcion = document.getElementById('gastoDescripcion').value.trim();
    const categoriaId = document.getElementById('gastoCategoria').value;
    const metodoPagoId = document.getElementById('gastoMetodoPago').value;
    const monto = document.getElementById('gastoMonto').value;
    const moneda = document.getElementById('gastoMoneda').value;
    const fechaLocal = document.getElementById('gastoFecha').value; 

    const erroresCliente = [];
    if(!concepto){
      erroresCliente.push('El concepto es obligatorio.');
      marcarCampoInvalido('gastoConcepto', 'errGastoConcepto', 'Ingresa un concepto.');
    }
    if(!categoriaId){
      erroresCliente.push('Selecciona una categoría.');
      marcarCampoInvalido('gastoCategoria', 'errGastoCategoria', 'Selecciona una categoría.');
    }
    if(!metodoPagoId){
      erroresCliente.push('Selecciona un método de pago.');
      marcarCampoInvalido('gastoMetodoPago', 'errGastoMetodoPago', 'Selecciona un método de pago.');
    }
    if(!monto || Number(monto) <= 0){
      erroresCliente.push('El monto debe ser mayor a 0.');
      marcarCampoInvalido('gastoMonto', 'errGastoMonto', 'Ingresa un monto válido.');
    }
    if(!fechaLocal){
      erroresCliente.push('La fecha es obligatoria.');
      marcarCampoInvalido('gastoFecha', 'errGastoFecha', 'Selecciona una fecha.');
    }

    if(erroresCliente.length){
      mostrarErrores(el.banner, erroresCliente);
      return;
    }

    ponerBotonCargando(el.btnGuardar, true);

    try{
      const fechaConSegundos = `${fechaLocal}:00.000`;
      const cuerpo = JSON.stringify({
        concepto, descripcion, montoOriginal: monto, moneda,
        fecha: fechaConSegundos, categoriaId, metodoPagoId,
      });

      const esEdicion = Boolean(gastoId);
      const respuesta = await hacerPeticionAutenticada(
        esEdicion ? `${API_BASE_URL}/gastos/${gastoId}` : `${API_BASE_URL}/gastos`,
        { method: esEdicion ? 'PUT' : 'POST', body: cuerpo }
      );

      if(!respuesta.ok){
        const mensajes = await parsearErroresBackend(respuesta);
        mostrarErrores(el.banner, mensajes);
        resaltarCamposPorMensaje(mensajes, [
          { palabras: ['concepto'], inputId: 'gastoConcepto', errorId: 'errGastoConcepto' },
          { palabras: ['categoria', 'categoría'], inputId: 'gastoCategoria', errorId: 'errGastoCategoria' },
          { palabras: ['metodo', 'método', 'pago'], inputId: 'gastoMetodoPago', errorId: 'errGastoMetodoPago' },
          { palabras: ['monto'], inputId: 'gastoMonto', errorId: 'errGastoMonto' },
          { palabras: ['moneda', 'currency'], inputId: 'gastoMoneda', errorId: 'errGastoMoneda' },
          { palabras: ['fecha', 'mes'], inputId: 'gastoFecha', errorId: 'errGastoFecha' },
        ]);
        return;
      }

      cerrarModalGasto();
      window.alGuardarGastoExitoso();

    }catch(error){
      mostrarErrores(el.banner, ['No se pudo conectar con el servidor. Verifica tu conexión.']);
    }finally{
      ponerBotonCargando(el.btnGuardar, false);
    }
  });
});