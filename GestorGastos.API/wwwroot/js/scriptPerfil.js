
exigirSesion();
let nombreOriginalGuardado = '';
let monedaOriginalGuardada = '';

/* ============================================================
   1. CARGA INICIAL DEL PERFIL
   ============================================================ */
function normalizarPerfil(crudo){
  return {
    nombre: crudo?.nombre ?? '',
    moneda: crudo?.monedaUsada ?? 'DOP',
  };
}

async function cargarPerfil(){
  try{
    const respuesta = await hacerPeticionAutenticada(`${API_BASE_URL}/usuario/perfil`);

    if(!respuesta.ok){
      const mensajes = await parsearErroresBackend(respuesta);
      mostrarErrores(document.getElementById('bannerNombre'), mensajes);
      return;
    }

    const crudo = await respuesta.json();
    const perfil = normalizarPerfil(crudo);

    document.getElementById('perfilNombre').value = perfil.nombre;
    document.getElementById('perfilMoneda').value = perfil.moneda;
    nombreOriginalGuardado = perfil.nombre;
    monedaOriginalGuardada = perfil.moneda;

    actualizarMonedaEnStorage(perfil.moneda);

  }catch(error){
    mostrarErrores(document.getElementById('bannerNombre'), ['No se pudo conectar con el servidor.']);
  }
}

cargarPerfil();


/* ============================================================
   2. ACTUALIZAR NOMBRE
   ============================================================ */
document.getElementById('formNombre').addEventListener('submit', async (e) => {
  e.preventDefault();

  const form = e.target;
  const banner = document.getElementById('bannerNombre');
  const btn = document.getElementById('btnActualizarNombre');

  ocultarBanner(banner);
  limpiarErroresDeCampos(form);

  const nuevoNombre = document.getElementById('perfilNombre').value.trim();

  if(!nuevoNombre){
    marcarCampoInvalido(
      'perfilNombre',
      'errPerfilNombre',
      'El nombre es obligatorio.'
    );

    mostrarErrores(banner, ['El nombre es obligatorio.']);
    return;
  }

  if(nuevoNombre === nombreOriginalGuardado){
    marcarCampoInvalido(
      'perfilNombre',
      'errPerfilNombre',
      'Ya estás usando este nombre.'
    );

    mostrarErrores(
      banner,
      ['El nuevo nombre debe ser diferente al actual.']
    );

    return;
  }

  ponerBotonCargando(btn, true);

  try{
    const respuesta = await hacerPeticionAutenticada(
      `${API_BASE_URL}/usuario/perfil`,
      {
        method: 'PUT',
        body: JSON.stringify({
          nombre: nuevoNombre
        }),
      }
    );

    if(!respuesta.ok){
      const mensajes = await parsearErroresBackend(respuesta);

      mostrarErrores(banner, mensajes);

      resaltarCamposPorMensaje(mensajes, [
        {
          palabras: ['nombre'],
          inputId: 'perfilNombre',
          errorId: 'errPerfilNombre'
        },
      ]);

      return;
    }

    nombreOriginalGuardado = nuevoNombre;

    mostrarExito(
      banner,
      'Nombre actualizado correctamente.'
    );

  }catch(error){
    mostrarErrores(
      banner,
      ['No se pudo conectar con el servidor. Verifica tu conexión.']
    );
  }finally{
    ponerBotonCargando(btn, false);
  }
});


/* ============================================================
   3. ACTUALIZAR MONEDA
   ============================================================ */

function actualizarMonedaEnStorage(nuevaMoneda){
  [localStorage, sessionStorage].forEach(storage => {
    const crudo = storage.getItem(USER_KEY);
    if(crudo){
      const usuario = JSON.parse(crudo);
      usuario.moneda = nuevaMoneda;
      storage.setItem(USER_KEY, JSON.stringify(usuario));
    }
  });
}

document.getElementById('formMoneda').addEventListener('submit', async (e) => {
  e.preventDefault();

  const banner = document.getElementById('bannerMoneda');
  const btn = document.getElementById('btnActualizarMoneda');
  ocultarBanner(banner);
  limpiarErroresDeCampos(e.target);

  const monedaUsada = document.getElementById('perfilMoneda').value;

  if (monedaOriginalGuardada === monedaUsada) {
    marcarCampoInvalido('perfilMoneda', 'errPerfilMoneda', 'Ya estás usando esta moneda.');
    mostrarErrores(banner, ['La moneda seleccionada ya es tu moneda actual.']);
    return;
  }

  ponerBotonCargando(btn, true);

  try{
    const respuesta = await hacerPeticionAutenticada(`${API_BASE_URL}/usuario/cambiar-moneda`, {
      method: 'PUT',
      body: JSON.stringify({ monedaUsada }),
    });

    if(!respuesta.ok){
      const mensajes = await parsearErroresBackend(respuesta);
      mostrarErrores(banner, mensajes);
      return;
    }

    monedaOriginalGuardada = monedaUsada;
    actualizarMonedaEnStorage(monedaUsada);

    mostrarExito(banner, 'Moneda actualizada correctamente.');

  }catch(error){
    mostrarErrores(banner, ['No se pudo conectar con el servidor. Verifica tu conexión.']);
  }finally{
    ponerBotonCargando(btn, false);
  }
});


/* ============================================================
   4. CAMBIAR CONTRASEÑA
   ============================================================ */
document.getElementById('formPassword').addEventListener('submit', async (e) => {
  e.preventDefault();

  const banner = document.getElementById('bannerPassword');
  const btn = document.getElementById('btnCambiarPassword');
  const form = e.target;
  ocultarBanner(banner);
  limpiarErroresDeCampos(form);

  const passwordActual = document.getElementById('passwordActual').value;
  const passwordNuevo = document.getElementById('passwordNuevo').value;
  const passwordConfirmar = document.getElementById('passwordConfirmar').value;

  const erroresCliente = [];
  if(!passwordActual){
    erroresCliente.push('Ingresa tu contraseña actual.');
    marcarCampoInvalido('passwordActual', 'errPasswordActual', 'Este campo es obligatorio.');
  }
  if(passwordNuevo.length < 8){
    erroresCliente.push('La nueva contraseña debe tener al menos 8 caracteres.');
    marcarCampoInvalido('passwordNuevo', 'errPasswordNuevo', 'Mínimo 8 caracteres.');
  }
  if(passwordNuevo !== passwordConfirmar){
    erroresCliente.push('Las contraseñas no coinciden.');
    marcarCampoInvalido('passwordConfirmar', 'errPasswordConfirmar', 'No coincide con la nueva contraseña.');
  }

  if (passwordNuevo === passwordActual) {
    marcarCampoInvalido('passwordNuevo', 'errPasswordNuevo', 'La nueva contraseña debe ser diferente.');
    mostrarErrores(banner, ['La nueva contraseña no puede ser igual a la actual.']);
    return;
  }

  if(erroresCliente.length){
    mostrarErrores(banner, erroresCliente);
    return;
  }

  ponerBotonCargando(btn, true);

  try{
    const respuesta = await hacerPeticionAutenticada(`${API_BASE_URL}/usuario/cambiar-password`, {
      method: 'PUT',
      body: JSON.stringify({ passwordActual, passwordNuevo }),
    });

    if(!respuesta.ok){
      const mensajes = await parsearErroresBackend(respuesta);
      mostrarErrores(banner, mensajes);
      resaltarCamposPorMensaje(mensajes, [
        { palabras: ['actual'], inputId: 'passwordActual', errorId: 'errPasswordActual' },
        { palabras: ['nueva', 'nuevo'], inputId: 'passwordNuevo', errorId: 'errPasswordNuevo' },
      ]);
      return;
    }

    mostrarExito(banner, 'Contraseña actualizada correctamente.');
    form.reset();

  }catch(error){
    mostrarErrores(banner, ['No se pudo conectar con el servidor. Verifica tu conexión.']);
  }finally{
    ponerBotonCargando(btn, false);
  }
});


/* ============================================================
   5. ELIMINAR CUENTA
   ============================================================ */
document.getElementById('btnEliminarCuenta').addEventListener('click', async () => {
  const confirmado = confirm(
    'Esta acción eliminará tu cuenta y todos tus datos de forma permanente. ' +
    '¿Estás seguro de que deseas continuar? Esto no se puede deshacer.'
  );
  if(!confirmado) return;

  const btn = document.getElementById('btnEliminarCuenta');
  ponerBotonCargando(btn, true);

  try{
    const respuesta = await hacerPeticionAutenticada(`${API_BASE_URL}/usuario/cuenta`, {
      method: 'DELETE',
    });
    
    if(respuesta.ok){
      cerrarSesion();
      return;
    }

    const mensajes = await parsearErroresBackend(respuesta);
    alert(mensajes[0]);

  }catch(error){
    alert('No se pudo conectar con el servidor.');
  }finally{
    ponerBotonCargando(btn, false);
  }
});


/* ============================================================
   6. CERRAR SESIÓN
   ============================================================ */
document.getElementById('btnLogout').addEventListener('click', cerrarSesion);