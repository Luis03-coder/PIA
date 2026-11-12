using CafeteriaApp;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace CafeteriaApp.WinUI;

public sealed partial class MainWindow : Window
{
    private readonly AppData data = Storage.Load();
    private readonly List<PartidaVenta> cart = [];
    private readonly StackPanel login = new() { Width = 390, Spacing = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
    private readonly TextBox username = new() { PlaceholderText = "Usuario" };
    private readonly PasswordBox password = new() { PlaceholderText = "Contraseña" };
    private readonly ComboBox productPicker = new() { Width = 320, PlaceholderText = "Selecciona un producto" };
    private readonly NumberBox quantity = new() { Value = 1, Minimum = 1, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline };
    private readonly NumberBox payment = new() { Value = 0, Minimum = 0, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline };
    private readonly ListView catalog = new();
    private readonly ListView cartView = new();
    private readonly ListView history = new();
    private readonly TextBox newName = new() { PlaceholderText = "Nombre del producto" };
    private readonly ComboBox newType = new() { ItemsSource = new[] { "Alimento", "Bebida", "Postre", "Ingrediente", "Otro" }, SelectedIndex = 0 };
    private readonly NumberBox newPrice = new() { PlaceholderText = "Precio (MXN)", Minimum = 0, Maximum = 100000 };
    private readonly NumberBox newStock = new() { PlaceholderText = "Existencias", Minimum = 0, Maximum = 100000 };
    private readonly TextBlock totalText = new() { FontSize = 22, FontWeight = Microsoft.UI.Text.FontWeights.Bold };
    private readonly TextBlock changeText = new() { FontSize = 16 };

    public MainWindow()
    {
        InitializeComponent();
        Title = "Café Flujo | WinUI 3";
        ExtendsContentIntoTitleBar = false;
        ShowLogin();
    }

    private void ShowLogin()
    {
        Root.Children.Clear(); login.Children.Clear();
        login.Children.Add(new TextBlock { Text = "☕ CAFÉ FLUJO", FontSize = 30, FontWeight = Microsoft.UI.Text.FontWeights.Bold, Foreground = Brush("#5B3725") });
        login.Children.Add(new TextBlock { Text = "Administración de cafetería", FontSize = 15 });
        login.Children.Add(username); login.Children.Add(password);
        var enter = new Button { Content = "Iniciar sesión", HorizontalAlignment = HorizontalAlignment.Stretch };
        enter.Click += (_, _) => { if (username.Text.Trim().Equals("admin", StringComparison.OrdinalIgnoreCase) && password.Password == "cafe123") ShowWorkspace(); else _ = new ContentDialog { Title = "Acceso", Content = "Usuario o contraseña incorrectos.", CloseButtonText = "Aceptar", XamlRoot = Root.XamlRoot }.ShowAsync(); };
        login.Children.Add(enter); login.Children.Add(new TextBlock { Text = "Demostración: admin / cafe123", Opacity = .65 }); Root.Children.Add(login);
    }

    private void ShowWorkspace()
    {
        Root.Children.Clear();
        var layout = new Grid(); layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); layout.RowDefinitions.Add(new RowDefinition());
        layout.Children.Add(new Border { Background = Brush("#5B3725"), Padding = new Thickness(24, 14, 24, 14), Child = new TextBlock { Text = "☕  CAFÉ FLUJO    |    Ventas e inventario", FontSize = 22, FontWeight = Microsoft.UI.Text.FontWeights.Bold, Foreground = Brush("#FFFFFF") } });
        var tabs = new TabView { IsAddTabButtonVisible = false, Margin = new Thickness(18) }; Grid.SetRow(tabs, 1); layout.Children.Add(tabs);
        tabs.TabItems.Add(new TabViewItem { Header = "Cobrar venta", Content = SalePage() });
        tabs.TabItems.Add(new TabViewItem { Header = "Productos", Content = ProductsPage() });
        tabs.TabItems.Add(new TabViewItem { Header = "Registrar", Content = RegisterPage() });
        tabs.TabItems.Add(new TabViewItem { Header = "Historial", Content = HistoryPage() });
        Root.Children.Add(layout); RefreshAll();
    }

    private UIElement SalePage()
    {
        var panel = new StackPanel { Spacing = 12, Padding = new Thickness(18) };
        panel.Children.Add(new TextBlock { Text = "Punto de venta", FontSize = 26, FontWeight = Microsoft.UI.Text.FontWeights.Bold });
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        row.Children.Add(productPicker); row.Children.Add(quantity); var add = new Button { Content = "Agregar al carrito" }; add.Click += (_, _) => AddCart(); row.Children.Add(add); panel.Children.Add(row);
        panel.Children.Add(cartView);
        var payrow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 18, VerticalAlignment = VerticalAlignment.Center };
        payrow.Children.Add(totalText); payrow.Children.Add(new TextBlock { Text = "Pago recibido", VerticalAlignment = VerticalAlignment.Center }); payrow.Children.Add(payment); payrow.Children.Add(changeText);
        payment.ValueChanged += (_, _) => UpdateTotal(); panel.Children.Add(payrow);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        var remove = new Button { Content = "Quitar seleccionado" }; remove.Click += (_, _) => { if (cartView.SelectedIndex >= 0 && cartView.SelectedIndex < cart.Count) cart.RemoveAt(cartView.SelectedIndex); RefreshCart(); };
        var charge = new Button { Content = "Cobrar", Background = Brush("#70452D"), Foreground = Brush("#FFFFFF") }; charge.Click += async (_, _) => await Charge();
        buttons.Children.Add(remove); buttons.Children.Add(charge); panel.Children.Add(buttons); return new ScrollViewer { Content = panel };
    }

    private UIElement ProductsPage()
    {
        var panel = new StackPanel { Spacing = 12, Padding = new Thickness(18) };
        panel.Children.Add(new TextBlock { Text = "Catálogo e inventario", FontSize = 26, FontWeight = Microsoft.UI.Text.FontWeights.Bold }); panel.Children.Add(catalog);
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        var id = new NumberBox { PlaceholderText = "ID del producto", Minimum = 1, Maximum = int.MaxValue };
        var name = new TextBox { PlaceholderText = "Nombre" }; var price = new NumberBox { PlaceholderText = "Precio", Minimum = 0 };
        var stock = new NumberBox { PlaceholderText = "Existencias", Minimum = 0 };
        var update = new Button { Content = "Actualizar seleccionado" }; update.Click += async (_, _) =>
        {
            if (catalog.SelectedIndex < 0 || catalog.SelectedIndex >= data.Productos.Count) { await Message("Selecciona un producto del catálogo."); return; }
            var p = data.Productos.OrderBy(x => x.Id).ElementAt(catalog.SelectedIndex); p.Nombre = string.IsNullOrWhiteSpace(name.Text) ? p.Nombre : name.Text.Trim(); if (price.Value >= 0) p.Precio = (decimal)price.Value; if (stock.Value >= 0) p.Inventario = (int)stock.Value;
            data.Historial.Add(new Movimiento { Fecha = DateTime.Now, ProductoId = p.Id, Producto = p.Nombre, Accion = "Actualización", Detalle = "Producto actualizado desde WinUI." }); Storage.Save(data); RefreshAll();
        };
        row.Children.Add(id); row.Children.Add(name); row.Children.Add(price); row.Children.Add(stock); row.Children.Add(update); panel.Children.Add(row);
        var csv = new Button { Content = "Exportar catálogo CSV" }; csv.Click += async (_, _) => await Message("Los productos se guardan en el JSON compartido. Para exportar CSV usa la versión WinForms: pestaña Consultar productos."); panel.Children.Add(csv);
        return new ScrollViewer { Content = panel };
    }

    private UIElement RegisterPage()
    {
        var panel = new StackPanel { Spacing = 12, Padding = new Thickness(18), MaxWidth = 520, HorizontalAlignment = HorizontalAlignment.Left };
        panel.Children.Add(new TextBlock { Text = "Registrar producto", FontSize = 26, FontWeight = Microsoft.UI.Text.FontWeights.Bold });
        panel.Children.Add(newName); panel.Children.Add(newType); panel.Children.Add(newPrice); panel.Children.Add(newStock);
        var tracked = new CheckBox { Content = "Controlar inventario", IsChecked = true }; panel.Children.Add(tracked);
        var save = new Button { Content = "Guardar producto" }; save.Click += async (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(newName.Text) || newType.SelectedItem is null || newPrice.Value < 0) { await Message("Completa el nombre, tipo y precio."); return; }
            var p = new Producto { Id = data.Productos.Count == 0 ? 1 : data.Productos.Max(x => x.Id) + 1, Nombre = newName.Text.Trim(), Tipo = newType.SelectedItem.ToString()!, Precio = (decimal)newPrice.Value, Inventario = newStock.Value < 0 ? 0 : (int)newStock.Value, Inventariable = tracked.IsChecked == true };
            data.Productos.Add(p); data.Historial.Add(new Movimiento { Fecha = DateTime.Now, ProductoId = p.Id, Producto = p.Nombre, Accion = "Alta", Detalle = "Producto registrado desde WinUI." }); Storage.Save(data); newName.Text = ""; newPrice.Value = 0; newStock.Value = 0; RefreshAll(); await Message($"Producto guardado con ID {p.Id}.");
        }; panel.Children.Add(save); return new ScrollViewer { Content = panel };
    }

    private UIElement HistoryPage() => new ScrollViewer { Content = new StackPanel { Padding = new Thickness(18), Spacing = 12, Children = { new TextBlock { Text = "Historial de movimientos", FontSize = 26, FontWeight = Microsoft.UI.Text.FontWeights.Bold }, history } } };

    private void AddCart()
    {
        if (productPicker.SelectedItem is not Producto p) { _ = Message("Selecciona un producto."); return; }
        int count = Math.Max(1, (int)quantity.Value); int current = cart.Where(x => x.ProductoId == p.Id).Sum(x => x.Cantidad);
        if (p.Inventariable && current + count > p.Inventario) { _ = Message($"Existencias disponibles: {Math.Max(0, p.Inventario-current)}."); return; }
        var line = cart.FirstOrDefault(x => x.ProductoId == p.Id);
        if (line is null) cart.Add(new PartidaVenta { ProductoId = p.Id, Producto = p.Nombre, Cantidad = count, PrecioUnitario = p.Precio, Importe = p.Precio * count }); else { line.Cantidad += count; line.Importe = line.Cantidad * line.PrecioUnitario; }
        RefreshCart();
    }

    private async Task Charge()
    {
        var sum = cart.Sum(x => x.Importe); if (cart.Count == 0) { await Message("Agrega productos al carrito."); return; }
        if ((decimal)payment.Value < sum) { await Message("El pago recibido no alcanza para cubrir el total."); return; }
        foreach (var line in cart) { var product = data.Productos.First(x => x.Id == line.ProductoId); if (product.Inventariable && product.Inventario < line.Cantidad) { await Message($"No hay suficientes existencias de {product.Nombre}."); return; } }
        var sale = new Venta { Folio = data.Ventas.Count == 0 ? 1 : data.Ventas.Max(x => x.Folio) + 1, Fecha = DateTime.Now, Partidas = cart.Select(x => new PartidaVenta { ProductoId = x.ProductoId, Producto = x.Producto, Cantidad = x.Cantidad, PrecioUnitario = x.PrecioUnitario, Importe = x.Importe }).ToList(), Total = sum, Pago = (decimal)payment.Value, Cambio = (decimal)payment.Value - sum };
        foreach (var item in sale.Partidas) { var p = data.Productos.First(x => x.Id == item.ProductoId); if (p.Inventariable) p.Inventario -= item.Cantidad; data.Historial.Add(new Movimiento { Fecha = sale.Fecha, ProductoId = p.Id, Producto = p.Nombre, Accion = "Venta", Detalle = $"Folio {sale.Folio}: {item.Cantidad} unidad(es); existencias: {p.Inventario}." }); }
        data.Ventas.Add(sale); Storage.Save(data); cart.Clear(); payment.Value = 0; RefreshAll(); await Message($"Venta cobrada. Folio {sale.Folio}. Total {sum:C2}. Cambio {sale.Cambio:C2}.");
    }

    private void RefreshAll()
    {
        catalog.ItemsSource = null; catalog.ItemsSource = data.Productos.OrderBy(x => x.Id).Select(x => $"#{x.Id}   {x.Nombre}   |   {x.Tipo}   |   {x.Precio:C2}   |   Existencias: {x.Inventario}").ToList();
        productPicker.ItemsSource = null; productPicker.ItemsSource = data.Productos.OrderBy(x => x.Nombre).ToList(); productPicker.DisplayMemberPath = nameof(Producto.Nombre);
        history.ItemsSource = null; history.ItemsSource = data.Historial.OrderByDescending(x => x.Fecha).Select(x => $"{x.Fecha:dd/MM/yyyy HH:mm}  |  {x.Accion}  |  {x.Producto}  |  {x.Detalle}").ToList(); RefreshCart();
    }
    private void RefreshCart() { cartView.ItemsSource = null; cartView.ItemsSource = cart.Select(x => $"{x.Producto}  × {x.Cantidad}  |  {x.PrecioUnitario:C2} c/u  |  {x.Importe:C2}").ToList(); UpdateTotal(); }
    private void UpdateTotal() { var sum = cart.Sum(x => x.Importe); totalText.Text = $"Total: {sum:C2}"; changeText.Text = $"Cambio: {Math.Max(0, (decimal)payment.Value-sum):C2}"; }
    private async Task Message(string message) => await new ContentDialog { Title = "Café Flujo", Content = message, CloseButtonText = "Aceptar", XamlRoot = Root.XamlRoot }.ShowAsync();
    private static SolidColorBrush Brush(string value)
    {
        var hex = value.TrimStart('#');
        if (hex.Length == 6) hex = "FF" + hex;
        return new SolidColorBrush(Color.FromArgb(
            byte.Parse(hex[..2], System.Globalization.NumberStyles.HexNumber),
            byte.Parse(hex[2..4], System.Globalization.NumberStyles.HexNumber),
            byte.Parse(hex[4..6], System.Globalization.NumberStyles.HexNumber),
            byte.Parse(hex[6..8], System.Globalization.NumberStyles.HexNumber)));
    }
}
