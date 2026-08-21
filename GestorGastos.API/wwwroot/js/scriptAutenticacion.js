/* ============================================================
   1. ANIMACIÓN DECORATIVA DEL LEDGER
   ============================================================ */
(function animarTotal(){
  const el = document.getElementById('ledgerTotal');
  const destino = 2310;
  let actual = 0;
  const paso = Math.ceil(destino / 40);
  const intervalo = setInterval(() => {
    actual = Math.min(actual + paso, destino);
    el.textContent = 'RD$ ' + actual.toLocaleString('es-DO', {minimumFractionDigits: 2});
    if(actual >= destino) clearInterval(intervalo);
  }, 25);
})();

/* ============================================================
   2. TOGGLE ENTRE LOGIN Y REGISTRO
   ============================================================ */
const tabs = document.getElementById('tabs');
const tabLogin = document.getElementById('tabLogin');
const tabRegistro = document.getElementById('tabRegistro');
const viewLogin = document.getElementById('viewLogin');
const viewRegistro = document.getElementById('viewRegistro');
const formLogin = document.getElementById('formLogin');
const formRegistro = document.getElementById('formRegistro');

function mostrarLogin(){
  tabs.classList.remove('mode-registro');
  tabLogin.classList.add('active');
  tabRegistro.classList.remove('active');
  viewLogin.style.display = 'block';
  viewRegistro.style.display = 'none';
  formLogin.classList.add('active');
  formRegistro.classList.remove('active');
}

function mostrarRegistro(){
  tabs.classList.add('mode-registro');
  tabRegistro.classList.add('active');
  tabLogin.classList.remove('active');
  viewRegistro.style.display = 'block';
  viewLogin.style.display = 'none';
  formRegistro.classList.add('active');
  formLogin.classList.remove('active');
}

tabLogin.addEventListener('click', mostrarLogin);
tabRegistro.addEventListener('click', mostrarRegistro);
document.getElementById('linkToRegistro').addEventListener('click', mostrarRegistro);
document.getElementById('linkToLogin').addEventListener('click', mostrarLogin);

/* ============================================================
   3. REDIRECCIÓN AUTOMÁTICA SI YA HAY SESIÓN ACTIVA
   ============================================================ */
(function verificarSesionExistente(){
  const token = leerToken(); // Función proveniente de comun.js
  if(token && !tokenExpirado(token)){
    window.location.href = '../pages/dashboard.html';
  }
})();

/* ============================================================
   4. SUBMIT DEL FORMULARIO DE LOGIN
   ============================================================ */
formLogin.addEventListener('submit', async (e) => {
  e.preventDefault();

  const banner = document.getElementById('loginBanner');
  const btn = document.getElementById('btnLogin');
  ocultarBanner(banner);
  limpiarErroresDeCampos(formLogin);

  const correo = document.getElementById('loginCorreo').value.trim();
  const password = document.getElementById('loginPassword').value;
  const recordar = document.getElementById('loginRecordar').checked;

  const erroresCliente = [];
  if(!correo){
    erroresCliente.push('El correo es obligatorio.');
    marcarCampoInvalido('loginCorreo', 'errLoginCorreo', 'Ingresa tu correo.');
  }
  if(!password){
    erroresCliente.push('La contraseña es obligatoria.');
    marcarCampoInvalido('loginPassword', 'errLoginPassword', 'Ingresa tu contraseña.');
  }

  if(erroresCliente.length){
    mostrarErrores(banner, erroresCliente);
    return;
  }

  ponerBotonCargando(btn, true);

  try{
    const respuesta = await fetch(`${API_BASE_URL}/usuario/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ Email: correo, password }),
    });

    if(!respuesta.ok){
      const mensajes = await parsearErroresBackend(respuesta);
      mostrarErrores(banner, mensajes);
      resaltarCamposPorMensaje(mensajes, [
        { palabras: ['correo', 'email', 'usuario'], inputId: 'loginCorreo', errorId: 'errLoginCorreo' },
        { palabras: ['contraseña', 'contrasena', 'password', 'clave'], inputId: 'loginPassword', errorId: 'errLoginPassword' },
      ]);
      return;
    }

    const datos = await respuesta.json();
    guardarToken(datos.token, recordar, { nombreUsuario: datos.nombreUsuario, email: datos.email, moneda: datos.monedaUsada });

    mostrarExito(banner, 'Sesión iniciada. Redirigiendo...');
    setTimeout(() => { window.location.href = '../pages/dashboard.html'; }, 600);

  }catch(error){
    mostrarErrores(banner, ['No se pudo conectar con el servidor. Verifica tu conexión.']);
  }finally{
    ponerBotonCargando(btn, false);
  }
});

/* ============================================================
   5. SUBMIT DEL FORMULARIO DE REGISTRO
   ============================================================ */
formRegistro.addEventListener('submit', async (e) => {
  e.preventDefault();

  const banner = document.getElementById('registroBanner');
  const btn = document.getElementById('btnRegistro');
  ocultarBanner(banner);
  limpiarErroresDeCampos(formRegistro);

  const nombre = document.getElementById('regNombre').value.trim();
  const correo = document.getElementById('regCorreo').value.trim();
  const password = document.getElementById('regPassword').value;
  const confirmarPassword = document.getElementById('regConfirmarPassword').value;
  const moneda = document.getElementById('regMoneda').value;

  const erroresCliente = [];

  if(!nombre){
    erroresCliente.push('El nombre es obligatorio.');
    marcarCampoInvalido('regNombre', 'errRegNombre', 'Ingresa tu nombre.');
  }
  if(!correo){
    erroresCliente.push('El correo es obligatorio.');
    marcarCampoInvalido('regCorreo', 'errRegCorreo', 'Ingresa tu correo.');
  }
  if(password.length < 8){
    erroresCliente.push('La contraseña debe tener al menos 8 caracteres.');
    marcarCampoInvalido('regPassword', 'errRegPassword', 'Mínimo 8 caracteres.');
  }
  if(password !== confirmarPassword){
    erroresCliente.push('Las contraseñas no coinciden.');
    marcarCampoInvalido('regConfirmarPassword', 'errRegConfirmarPassword', 'No coincide con la contraseña.');
  }

  if(erroresCliente.length){
    mostrarErrores(banner, erroresCliente);
    return;
  }

  ponerBotonCargando(btn, true);

  try{
    const respuesta = await fetch(`${API_BASE_URL}/usuario/registro`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ nombre, Email: correo, password, confirmarPassword, MonedaUsada: moneda }),
    });

    if(!respuesta.ok){
      const mensajes = await parsearErroresBackend(respuesta);
      mostrarErrores(banner, mensajes);
      resaltarCamposPorMensaje(mensajes, [
        { palabras: ['nombre'], inputId: 'regNombre', errorId: 'errRegNombre' },
        { palabras: ['correo', 'email'], inputId: 'regCorreo', errorId: 'errRegCorreo' },
        { palabras: ['contraseña', 'contrasena', 'password', 'clave'], inputId: 'regPassword', errorId: 'errRegPassword' },
        { palabras: ['moneda', 'currency'], inputId: 'regMoneda', errorId: 'errRegMoneda' },
      ]);
      return;
    }

    const datos = await respuesta.json();

    if(datos.token){

      guardarToken(datos.token, false, { nombreUsuario: datos.nombreUsuario, email: datos.email, moneda: datos.monedaUsada });
      mostrarExito(banner, 'Cuenta creada. Redirigiendo...');
      setTimeout(() => { window.location.href = '../pages/dashboard.html'; }, 600);
      return;
    }

    mostrarExito(banner, 'Cuenta creada con éxito. Ahora inicia sesión.');
    formRegistro.reset();
    setTimeout(mostrarLogin, 900);

  }catch(error){
    mostrarErrores(banner, ['No se pudo conectar con el servidor. Verifica tu conexión.']);
  }finally{
    ponerBotonCargando(btn, false);
  }
});