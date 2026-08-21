# Saldo — Gestor de Gastos

**Saldo** es una aplicación web para la gestión de finanzas personales desarrollada con **ASP.NET Core, SQL Server y JavaScript Vanilla**.

El sistema permite registrar y organizar gastos, administrar presupuestos mensuales, consultar reportes financieros, importar transacciones desde archivos Excel y visualizar información relevante desde un dashboard centralizado.

El proyecto utiliza una arquitectura por capas inspirada en **Onion Architecture**, separando las reglas de negocio, el acceso a datos, la API y la interfaz de usuario.

---

## Características principales

### Dashboard financiero

El dashboard proporciona una vista general de la actividad financiera del usuario, incluyendo:

- Total gastado durante el mes.
- Promedio diario de gastos.
- Gasto más alto registrado.
- Cantidad de registros del período.
- Gastos recientes.
- Distribución de gastos por categoría mediante gráficos interactivos con **Chart.js**.
- Alertas relacionadas con presupuestos.

### Gestión de gastos

Permite administrar los gastos del usuario mediante operaciones de:

- Registro de nuevos gastos.
- Consulta de gastos existentes.
- Edición de transacciones.
- Eliminación lógica mediante **Soft Delete**.
- Restauración de gastos enviados a la papelera.
- Filtrado por:
  - Texto.
  - Categoría.
  - Método de pago.
  - Fecha.
- Manejo de diferentes monedas.

### Catálogos

El sistema permite administrar información reutilizada por los gastos:

- Categorías.
- Métodos de pago.

Cada usuario mantiene sus propios catálogos asociados a sus transacciones.

### Presupuestos

Los usuarios pueden establecer límites de gasto mensuales para diferentes categorías.

El sistema calcula el porcentaje consumido y representa visualmente el estado del presupuesto mediante niveles de alerta:

- Bajo control.
- Consumo medio.
- Cerca del límite.
- Presupuesto excedido.

Cuando el monto gastado supera el presupuesto establecido, la interfaz informa cuánto se ha excedido el usuario.

### Reportes

La aplicación genera reportes financieros por mes y año con información como:

- Total gastado.
- Total del mes anterior.
- Diferencia entre períodos.
- Categoría con mayor gasto.
- Desglose de gastos por categoría.

Los reportes pueden exportarse en diferentes formatos:

- TXT
- Excel
- JSON

### Importación de gastos desde Excel

Saldo permite importar múltiples gastos desde archivos `.xlsx`.

El proceso incluye:

- Plantilla descargable.
- Selección mediante explorador de archivos.
- Drag & Drop.
- Validación de cada fila.
- Validación de fechas.
- Validación de montos.
- Validación de categorías y métodos de pago.
- Detección de registros duplicados.
- Manejo de monedas.
- Información detallada sobre filas con errores.

### Gestión de cuenta

Desde la sección de perfil, el usuario puede:

- Actualizar su nombre.
- Cambiar su moneda principal.
- Cambiar su contraseña.
- Eliminar su cuenta mediante Soft Delete.

### Autenticación y autorización

El sistema utiliza **JSON Web Tokens (JWT)** para proteger los recursos de la API.

Las páginas privadas verifican la existencia y vigencia del token antes de permitir el acceso, mientras que las peticiones protegidas incluyen automáticamente:

```http
Authorization: Bearer <token>
```

Al expirar o invalidarse la sesión, el usuario es redirigido nuevamente a la pantalla de autenticación.

---

## Tecnologías utilizadas

### Backend

- C#
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- LINQ
- JWT Authentication
- Dependency Injection

### Base de datos

- Microsoft SQL Server
- Entity Framework Core Code First
- Migrations

### Frontend

- HTML5
- CSS3
- Vanilla JavaScript
- Fetch API
- Chart.js

### Procesamiento de archivos

- Excel `.xlsx`
- ClosedXML
- FormData

### Documentación y desarrollo

- OpenAPI
- Git
- GitHub

---

## Arquitectura

El backend está organizado mediante una arquitectura por capas inspirada en **Onion Architecture**, buscando mantener separadas las responsabilidades principales del sistema.

La aplicación se divide conceptualmente en las siguientes capas:

### Domain

Contiene los componentes centrales del negocio:

- Entidades.
- Modelos de dominio.
- Interfaces y contratos.

Esta capa representa el núcleo del sistema y evita depender directamente de tecnologías de infraestructura.

### Services

Contiene la lógica de negocio de la aplicación.

Entre sus responsabilidades se encuentran:

- Gestión de gastos.
- Validaciones.
- Conversión de monedas.
- Presupuestos.
- Generación de reportes.
- Importación de archivos Excel.
- Gestión de usuarios.

Los servicios interactúan con la persistencia mediante interfaces.

### Data / Repository

Implementa el acceso a datos utilizando:

- Entity Framework Core.
- SQL Server.
- Patrón Repository.

Esta capa se encarga de consultar y persistir las entidades utilizadas por la aplicación.

### API

La capa de presentación expone los recursos mediante controladores REST.

También contiene la configuración relacionada con:

- Dependency Injection.
- Autenticación JWT.
- CORS.
- Middlewares.
- Manejo global de errores.
- Identificadores de solicitud.
- Trazabilidad de peticiones.
- Archivos estáticos.

### Frontend

La interfaz se encuentra integrada dentro del proyecto ASP.NET Core mediante `wwwroot`.

```text
wwwroot/
├── css/
├── js/
├── favicon.ico
├── login.html
├── dashboard.html
├── gastos.html
├── catalogos.html
├── reportes.html
├── presupuestos.html
├── importacion.html
└── perfil.html
```

ASP.NET Core sirve directamente estos archivos utilizando su middleware de archivos estáticos.

---

## Flujo general

```text
Frontend
   │
   │ HTTP / JSON / JWT
   ▼
ASP.NET Core API
   │
   ▼
Services
   │
   ▼
Repository
   │
   ▼
Entity Framework Core
   │
   ▼
SQL Server
```

Este diseño ayuda a mantener separadas la interfaz, las reglas de negocio y la persistencia.

---

## Requisitos

Antes de ejecutar el proyecto asegúrate de tener instalado:

- .NET SDK compatible con el proyecto.
- SQL Server.
- Entity Framework Core CLI, si utilizarás las migraciones mediante terminal.
- Git.

Para instalar las herramientas de Entity Framework Core:

```bash
dotnet tool install --global dotnet-ef
```

---

## Instalación

### 1. Clonar el repositorio

```bash
git clone https://github.com/4ndr9x/Sistema-Gestor-De-Gastos.git
```

Accede al directorio del proyecto:

```bash
cd Sistema-Gestor-De-Gastos
```

### 2. Restaurar dependencias

```bash
dotnet restore
```

### 3. Configurar la base de datos

Configura la cadena de conexión correspondiente a SQL Server en `appsettings.json`.

Ejemplo:

```json
{
  "ConnectionStrings": {
    "AppConnection": "Server=TU_SERVIDOR;Database=TU_BASE_DE_DATOS;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Adapta los valores según tu entorno.

---

## Configuración de JWT

La aplicación requiere configuración para la generación y validación de tokens JWT.

En `appsettings.json` configura los valores correspondientes:

```json
{
  "JwtSettings": {
    "SecretKey": "TU_CLAVE_SECRETA",
    "Issuer": "TU_ISSUER",
    "Audience": "TU_AUDIENCE"
  }
}
```

> No utilices claves reales de producción dentro de repositorios públicos.

Para entornos reales se recomienda almacenar secretos mediante variables de entorno, Secret Manager u otro sistema de gestión de secretos.

---

## Base de datos

Después de configurar la cadena de conexión, aplica las migraciones de Entity Framework Core:

```bash
dotnet ef database update
```

Esto creará o actualizará la estructura necesaria en SQL Server.

---

## Ejecutar la aplicación

Inicia el proyecto:

```bash
dotnet run
```

ASP.NET Core iniciará la API y servirá también los archivos estáticos almacenados dentro de `wwwroot`.

Dependiendo de la configuración del proyecto, podrás acceder a la aplicación utilizando la dirección mostrada por .NET al iniciar el servidor.

Por ejemplo:

```text
https://localhost:<puerto>
```

---

## Endpoints principales

La API expone recursos relacionados con:

```text
/api/usuario
/api/gastos
/api/categorias
/api/metodospago
/api/presupuestos
/api/reportes
```

Entre las operaciones disponibles se encuentran:

```http
GET
POST
PUT
PATCH
DELETE
```

Los endpoints privados requieren autenticación mediante JWT.

---

## Soft Delete

Algunas operaciones de eliminación utilizan **Soft Delete**.

En lugar de eliminar físicamente el registro de la base de datos:

```text
DELETE definitivo
```

se modifica su estado:

```text
Activo = false
```

Este mecanismo se utiliza, por ejemplo, para permitir que determinados registros puedan ser recuperados posteriormente.

En el caso de los gastos, los registros eliminados pueden visualizarse desde la papelera y restaurarse.

---

## Manejo de errores

La API utiliza un middleware global para centralizar el tratamiento de excepciones.

Las respuestas de error pueden incluir información como:

```json
{
  "codigo": 400,
  "mensaje": "Descripción general del error.",
  "detalles": [
    "Detalle adicional del error."
  ],
  "requestId": "identificador-de-la-peticion"
}
```

El `requestId` facilita la trazabilidad de errores entre el cliente y el servidor.

---

## Seguridad

Entre las medidas implementadas se encuentran:

- Autenticación mediante JWT.
- Autorización de endpoints protegidos.
- Validación de sesión desde el frontend.
- Separación de información por usuario.
- Validaciones en frontend y backend.
- Manejo centralizado de excepciones.
- Soft Delete para determinadas entidades.
- Configuración de CORS.
- Trazabilidad de peticiones mediante identificadores únicos.

---

## Objetivo del proyecto

Saldo fue desarrollado como un sistema completo de gestión de gastos personales con el objetivo de integrar conceptos como:

- Desarrollo de APIs REST.
- Arquitectura por capas.
- Programación orientada a objetos.
- Dependency Injection.
- Patrón Repository.
- Entity Framework Core.
- SQL Server.
- Autenticación y autorización.
- Consumo de APIs desde JavaScript.
- Manejo de archivos Excel.
- Reportes financieros.
- Diseño de interfaces web.

---

## Repositorio

Código fuente:

`https://github.com/4ndr9x/Sistema-Gestor-De-Gastos`

---

## Autor

**Andres Perez**\
Desarrollo de Software\
GitHub: [@4ndr9x](https://github.com/4ndr9x)
