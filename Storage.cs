using System.Security.Cryptography;
using System.Text.Json;

namespace CafeteriaApp;

public static class Storage
{
    private static readonly string Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CafeFlujo");
    private static readonly string FilePath = Path.Combine(Folder, "datos.json");
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static AppData Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var data = JsonSerializer.Deserialize<AppData>(File.ReadAllText(FilePath)) ?? Seed();
                
                if (data.Usuarios is not { Count: > 0 }) { data.Usuarios = [AdminInicial()]; Save(data); }
                return data;
            }
        }
        catch { }
        var nuevo = Seed(); Save(nuevo); return nuevo;
    }

    public static void Save(AppData data)
    {
        Directory.CreateDirectory(Folder);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(data, Options));
    }

    private static Usuario AdminInicial()
    {
        var (salt, hash) = Seguridad.CrearHash("cafe123");
        return new Usuario { Nombre = "admin", Salt = salt, Hash = hash };
    }

    private static AppData Seed() => new()
    {
        Productos =
        [
            new() { Id = 1, Nombre = "Café americano", Tipo = "Bebida", Precio = 35, Inventario = 40 },
            new() { Id = 2, Nombre = "Sándwich de pollo", Tipo = "Alimento", Precio = 78, Inventario = 16 },
            new() { Id = 3, Nombre = "Galleta de avena", Tipo = "Postre", Precio = 22, Inventario = 25 },
            new() { Id = 4, Nombre = "Agua embotellada", Tipo = "Bebida", Precio = 20, Inventario = 30 }
        ],
        Historial = [new() { Fecha = DateTime.Now, ProductoId = 1, Producto = "Café americano", Accion = "Inicio", Detalle = "Inventario inicial" }],
        Usuarios = [AdminInicial()]
    };
}


public static class Seguridad
{
    private const int Iteraciones = 100_000;

    public static (string Salt, string Hash) CrearHash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iteraciones, HashAlgorithmName.SHA256, 32);
        return (Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    public static bool Verificar(string password, Usuario usuario)
    {
        try
        {
            var salt = Convert.FromBase64String(usuario.Salt);
            var esperado = Convert.FromBase64String(usuario.Hash);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iteraciones, HashAlgorithmName.SHA256, esperado.Length);
            return CryptographicOperations.FixedTimeEquals(actual, esperado);
        }
        catch { return false; }
    }
}
