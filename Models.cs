namespace CafeteriaApp;

public sealed class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Tipo { get; set; } = "";
    public decimal Precio { get; set; }
    public int Inventario { get; set; }
    public bool Inventariable { get; set; } = true;
}

public sealed class Movimiento
{
    public DateTime Fecha { get; set; }
    public int ProductoId { get; set; }
    public string Producto { get; set; } = "";
    public string Accion { get; set; } = "";
    public string Detalle { get; set; } = "";
}

// NUEVO: usuario del sistema. La contraseña nunca se guarda en texto plano:
// se guarda un hash (huella) y la sal aleatoria con la que se calculó.
public sealed class Usuario
{
    public string Nombre { get; set; } = "";
    public string Salt { get; set; } = "";
    public string Hash { get; set; } = "";
}

public sealed class AppData
{
    public List<Producto> Productos { get; set; } = [];
    public List<Movimiento> Historial { get; set; } = [];
    public List<Venta> Ventas { get; set; } = [];
    public List<Usuario> Usuarios { get; set; } = [];   // NUEVO
}

public sealed class Venta
{
    public int Folio { get; set; }
    public DateTime Fecha { get; set; }
    public List<PartidaVenta> Partidas { get; set; } = [];
    public decimal Total { get; set; }
    public decimal Pago { get; set; }
    public decimal Cambio { get; set; }
}

public sealed class PartidaVenta
{
    public int ProductoId { get; set; }
    public string Producto { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Importe { get; set; }
}
