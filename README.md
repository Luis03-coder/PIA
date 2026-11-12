# Mi proyecto: Café Flujo

## Descripción

Para este proyecto desarrollé una aplicación de escritorio para administrar una cafetería. La hice con C# y Windows Forms, y agregué una segunda interfaz con WinUI 3. Con ellas puedo registrar alimentos y bebidas, controlar existencias, consultar y actualizar productos, y cobrar las ventas.

## Problema que quiero resolver

En una cafetería pequeña es fácil perder de vista los precios, las existencias y las ventas cuando se anotan por separado. Mi aplicación reúne esa información en un solo lugar. Cuando cobro una venta, calcula el total y el cambio, guarda el movimiento y descuenta del inventario las unidades vendidas.

## ¿Qué puedo hacer con mi aplicación?

- Iniciar sesión para entrar al sistema.
- Abrir la versión clásica hecha en Windows Forms o la versión moderna hecha en WinUI 3.
- Registrar productos con nombre, tipo, precio y existencias.
- Consultar el catálogo y buscar por ID, nombre o tipo.
- Actualizar la información de un producto usando su ID.
- Agregar productos a un carrito, indicar cantidades y cobrar.
- Revisar el total, el pago recibido y el cambio.
- Consultar el historial de altas, cambios y ventas.
- Exportar el catálogo o el historial a CSV para abrirlos en Excel.
- Imprimir el catálogo o guardarlo como PDF usando una impresora PDF de Windows.
- Preparar un correo con el programa de correo instalado.

## Cómo puedo iniciar sesión

Para entrar con la cuenta de demostración uso:

- Usuario: `admin`
- Contraseña: `cafe123`

También puedo crear más usuarios con el botón **Crear nuevo usuario** de la pantalla de acceso. Los usuarios se guardan en el mismo archivo JSON y sus contraseñas se almacenan con hash (PBKDF2 con SHA-256 y sal aleatoria), nunca en texto plano.

## Cómo guardo la información

Mi aplicación guarda los productos, las ventas, el historial y los usuarios en un archivo local de este equipo: `%LOCALAPPDATA%\CafeFlujo\datos.json`. Las dos interfaces usan el mismo archivo. Al abrirla por primera vez carga algunos productos de ejemplo.

## Cómo puedo ejecutar el proyecto

Para abrir la versión Windows Forms necesito Windows y .NET 10 SDK con soporte de escritorio. Abro `CafeteriaApp.csproj` en Visual Studio y presiono **Iniciar**, o ejecuto `dotnet run` desde la carpeta del proyecto.

Para abrir mi versión WinUI 3, abro `CafeteriaApp.WinUI\CafeteriaApp.WinUI.csproj` en Visual Studio. Necesito tener instalada la carga de trabajo **Desarrollo de aplicaciones de Windows** y restaurar el paquete Windows App SDK cuando Visual Studio lo solicite. Después presiono **Iniciar**. La versión WinUI 3 también usa el acceso `admin` / `cafe123`.

## Alcance

La aplicación está pensada como caso práctico académico y para el manejo local de una cafetería pequeña. En Windows Forms puedo exportar el catálogo y el historial, imprimir el catálogo a PDF y preparar un correo con el programa instalado; para compartir un reporte, primero exporto el CSV o PDF y lo adjunto al mensaje.
