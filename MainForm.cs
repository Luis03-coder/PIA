using System.Diagnostics;
using System.Drawing.Printing;
using System.Globalization;
using System.Text;

namespace CafeteriaApp;

public sealed class MainForm : Form
{
    private readonly AppData data = Storage.Load();
    private readonly TabControl tabs = new() { Dock = DockStyle.Fill };
    private readonly DataGridView productsGrid = Grid();
    private readonly DataGridView historyGrid = Grid();
    private readonly TextBox nameBox = new() { Width = 240 };
    private readonly ComboBox typeBox = new() { Width = 180, DropDownStyle = ComboBoxStyle.DropDown };
    private readonly NumericUpDown priceBox = new() { Width = 140, DecimalPlaces = 2, Maximum = 100000, Minimum = 0 };
    private readonly NumericUpDown stockBox = new() { Width = 140, Maximum = 100000 };
    private readonly CheckBox trackedBox = new() { Text = "Controlar inventario", Checked = true, AutoSize = true };
    private readonly TextBox updateNameBox = new() { Width = 240 };
    private readonly ComboBox updateTypeBox = new() { Width = 180, DropDownStyle = ComboBoxStyle.DropDown };
    private readonly NumericUpDown updatePriceBox = new() { Width = 140, DecimalPlaces = 2, Maximum = 100000, Minimum = 0 };
    private readonly NumericUpDown updateStockBox = new() { Width = 140, Maximum = 100000 };
    private readonly CheckBox updateTrackedBox = new() { Text = "Controlar inventario", AutoSize = true };
    private readonly TextBox searchId = new() { Width = 110 };
    private readonly Label status = new() { AutoSize = true, ForeColor = Color.DimGray };
    private readonly ComboBox saleProduct = new() { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown saleQuantity = new() { Width = 100, Minimum = 1, Maximum = 1000, Value = 1 };
    private readonly NumericUpDown paymentBox = new() { Width = 160, DecimalPlaces = 2, Maximum = 1000000 };
    private readonly DataGridView cartGrid = Grid();
    private readonly List<PartidaVenta> cart = [];
    private readonly Label totalLabel = new() { AutoSize = true, Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.FromArgb(91, 55, 37) };
    private readonly Label changeLabel = new() { AutoSize = true, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(40, 110, 70), Text = "Cambio: $0.00" };

    public MainForm()
    {
        Text = "Café Flujo | Productos e inventario"; WindowState = FormWindowState.Maximized; MinimumSize = new(900, 600); BackColor = Color.FromArgb(247, 243, 236);
        var header = new Panel { Dock = DockStyle.Top, Height = 74, BackColor = Color.FromArgb(91, 55, 37), Padding = new Padding(24, 10, 20, 10) };
        header.Controls.Add(new Label { Text = "☕  CAFÉ FLUJO", Dock = DockStyle.Left, Width = 350, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.White, Font = new Font("Segoe UI", 20, FontStyle.Bold) });
        header.Controls.Add(new Label { Text = "Gestión de productos e inventario", Dock = DockStyle.Right, Width = 300, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.White, Font = new Font("Segoe UI", 11) });
        Controls.Add(tabs); Controls.Add(header);
        BuildSaleTab(); BuildRegisterTab(); BuildConsultTab(); BuildUpdateTab(); BuildHistoryTab(); RefreshAll();
        FormClosing += (_, _) => Storage.Save(data);
    }

    private void BuildSaleTab()
    {
        var page = new TabPage("Cobrar venta") { BackColor = Color.FromArgb(247, 243, 236), Padding = new Padding(24) };
        page.Controls.Add(Heading("Punto de venta", "Agrego productos al carrito, recibo el pago y calculo el cambio."));
        var row = new FlowLayoutPanel { Location = new(26, 100), Size = new(1000, 48), WrapContents = false };
        row.Controls.Add(new Label { Text = "Producto", AutoSize = true, Padding = new Padding(0, 10, 0, 0) }); row.Controls.Add(saleProduct);
        row.Controls.Add(new Label { Text = "Cantidad", AutoSize = true, Padding = new Padding(8, 10, 0, 0) }); row.Controls.Add(saleQuantity);
        var add = Button("Agregar al carrito", 0, 0, 160); add.Click += (_, _) => AddToCart(); row.Controls.Add(add); page.Controls.Add(row);
        cartGrid.Location = new(26, 158); cartGrid.Size = new(850, 330); page.Controls.Add(cartGrid);
        totalLabel.Location = new(900, 175); page.Controls.Add(totalLabel);
        var payLabel = new Label { Text = "Pago recibido (MXN)", AutoSize = true, Location = new(902, 240), Font = new Font("Segoe UI", 10, FontStyle.Bold) }; page.Controls.Add(payLabel);
        paymentBox.Location = new(902, 270); paymentBox.Font = new Font("Segoe UI", 11); paymentBox.ValueChanged += (_, _) => UpdateSaleSummary(); page.Controls.Add(paymentBox);
        changeLabel.Location = new(902, 320); page.Controls.Add(changeLabel);
        var remove = Button("Quitar producto", 26, 510, 160); remove.Click += (_, _) => RemoveFromCart(); page.Controls.Add(remove);
        var clear = Button("Vaciar carrito", 196, 510, 140); clear.Click += (_, _) => { cart.Clear(); RefreshCart(); }; page.Controls.Add(clear);
        var charge = Button("Cobrar", 902, 380, 150); charge.Height = 48; charge.Font = new Font("Segoe UI", 12, FontStyle.Bold); charge.Click += (_, _) => ChargeSale(); page.Controls.Add(charge);
        tabs.TabPages.Add(page);
    }

    private void BuildRegisterTab()
    {
        var page = new TabPage("Registrar producto") { BackColor = Color.FromArgb(247, 243, 236), Padding = new Padding(24) };
        var title = Heading("Alta de producto", "Registra alimentos y bebidas para mantener actualizado el catálogo y las existencias.");
        var card = Card(22, 100, 660, 370); page.Controls.Add(card); page.Controls.Add(title);
        AddField(card, "Nombre del producto", nameBox, 24, 30); typeBox.Items.AddRange(new object[] { "Alimento", "Bebida", "Postre", "Ingrediente", "Otro" }); AddField(card, "Tipo", typeBox, 350, 30);
        AddField(card, "Precio de venta (MXN)", priceBox, 24, 125); AddField(card, "Existencias", stockBox, 350, 125); trackedBox.Location = new(30, 225); card.Controls.Add(trackedBox);
        var save = Button("Guardar producto", 25, 275, 190); save.Click += (_, _) => AddProduct(); card.Controls.Add(save);
        var note = new Label { Text = "El sistema asigna el ID automáticamente y guarda los cambios en este equipo.", AutoSize = true, Location = new(25, 325), ForeColor = Color.DimGray }; card.Controls.Add(note);
        page.Controls.Add(status); status.Location = new(26, 500); tabs.TabPages.Add(page);
    }

    private void BuildConsultTab()
    {
        var page = new TabPage("Consultar productos") { BackColor = Color.FromArgb(247, 243, 236), Padding = new Padding(24) };
        page.Controls.Add(Heading("Catálogo e inventario", "Consulta, busca y revisa precio y existencias de cada producto."));
        var row = new FlowLayoutPanel { Location = new(26, 100), Size = new(900, 45), BackColor = Color.Transparent, WrapContents = false };
        var filter = new TextBox { Width = 280, PlaceholderText = "Buscar por nombre, ID o tipo" }; var search = Button("Buscar", 0, 0, 100); var all = Button("Mostrar todos", 0, 0, 130);
        row.Controls.Add(filter); row.Controls.Add(search); row.Controls.Add(all); page.Controls.Add(row);
        search.Click += (_, _) => RefreshProducts(filter.Text); all.Click += (_, _) => { filter.Clear(); RefreshProducts(); };
        productsGrid.Location = new(26, 158); productsGrid.Size = new(1050, 440); page.Controls.Add(productsGrid);
        var exportRow = new FlowLayoutPanel { Location = new(26, 610), Size = new(950, 48), WrapContents = false };
        var csv = Button("Exportar Excel / CSV", 0, 0, 190); csv.Click += (_, _) => ExportCsv(); var pdf = Button("Imprimir / Guardar PDF", 0, 0, 190); pdf.Click += (_, _) => ExportPdf(); var mail = Button("Preparar correo", 0, 0, 150); mail.Click += (_, _) => PrepareEmail();
        exportRow.Controls.AddRange([csv, pdf, mail]); page.Controls.Add(exportRow); tabs.TabPages.Add(page);
    }

    private void BuildUpdateTab()
    {
        var page = new TabPage("Actualizar producto") { BackColor = Color.FromArgb(247, 243, 236), Padding = new Padding(24) }; page.Controls.Add(Heading("Actualizar producto", "Escribe el ID para cargar sus datos; guarda los cambios cuando termines."));
        var card = Card(25, 100, 700, 400); page.Controls.Add(card);
        var label = new Label { Text = "ID del producto", AutoSize = true, Location = new(26, 24), Font = new Font("Segoe UI", 10, FontStyle.Bold) }; card.Controls.Add(label); searchId.Location = new(25, 52); card.Controls.Add(searchId);
        var load = Button("Cargar", 155, 48, 100); load.Click += (_, _) => LoadProductForUpdate(); card.Controls.Add(load);
        AddField(card, "Nombre", updateNameBox, 25, 105); updateTypeBox.Items.AddRange(new object[] { "Alimento", "Bebida", "Postre", "Ingrediente", "Otro" }); AddField(card, "Tipo", updateTypeBox, 360, 105); AddField(card, "Precio (MXN)", updatePriceBox, 25, 200); AddField(card, "Existencias", updateStockBox, 360, 200);
        updateTrackedBox.Location = new(30, 290); card.Controls.Add(updateTrackedBox);
        var save = Button("Guardar actualización", 25, 335, 210); save.Click += (_, _) => UpdateProduct(); card.Controls.Add(save);
        tabs.TabPages.Add(page);
    }

    private void BuildHistoryTab()
    {
        var page = new TabPage("Historial") { BackColor = Color.FromArgb(247, 243, 236), Padding = new Padding(24) }; page.Controls.Add(Heading("Historial de movimientos", "Registro automático de altas, actualizaciones y ajustes de existencias."));
        historyGrid.Location = new(26, 100); historyGrid.Size = new(1050, 500); page.Controls.Add(historyGrid);
        var export = Button("Exportar historial CSV", 26, 620, 200); export.Click += (_, _) => ExportHistoryCsv(); page.Controls.Add(export); tabs.TabPages.Add(page);
    }

    private void AddProduct()
    {
        if (string.IsNullOrWhiteSpace(nameBox.Text) || string.IsNullOrWhiteSpace(typeBox.Text)) { Warn("Escribe el nombre y el tipo del producto."); return; }
        var p = new Producto { Id = data.Productos.Count == 0 ? 1 : data.Productos.Max(x => x.Id) + 1, Nombre = nameBox.Text.Trim(), Tipo = typeBox.Text.Trim(), Precio = priceBox.Value, Inventario = (int)stockBox.Value, Inventariable = trackedBox.Checked };
        data.Productos.Add(p); Record(p, "Alta", $"Producto registrado con {p.Inventario} unidades."); Storage.Save(data); RefreshAll(); ClearEditor(); MessageBox.Show($"Producto guardado con ID {p.Id}.", "Café Flujo");
    }

    private void AddToCart()
    {
        if (saleProduct.SelectedItem is not Producto p) { Warn("Selecciona un producto para agregar."); return; }
        var quantity = (int)saleQuantity.Value;
        var alreadyInCart = cart.Where(x => x.ProductoId == p.Id).Sum(x => x.Cantidad);
        if (p.Inventariable && alreadyInCart + quantity > p.Inventario) { Warn($"Solo hay {p.Inventario - alreadyInCart} unidades disponibles de {p.Nombre}."); return; }
        var line = cart.FirstOrDefault(x => x.ProductoId == p.Id);
        if (line is null) cart.Add(new PartidaVenta { ProductoId = p.Id, Producto = p.Nombre, Cantidad = quantity, PrecioUnitario = p.Precio, Importe = p.Precio * quantity });
        else { line.Cantidad += quantity; line.Importe = line.Cantidad * line.PrecioUnitario; }
        RefreshCart();
    }

    private void RemoveFromCart()
    {
        if (cartGrid.CurrentRow?.DataBoundItem is PartidaVenta selected) cart.RemoveAll(x => x.ProductoId == selected.ProductoId);
        else if (cartGrid.CurrentRow?.Index is int index && index >= 0 && index < cart.Count) cart.RemoveAt(index);
        RefreshCart();
    }

    private void ChargeSale()
    {
        var total = cart.Sum(x => x.Importe);
        if (cart.Count == 0) { Warn("Agrega al menos un producto antes de cobrar."); return; }
        if (paymentBox.Value < total) { Warn("El pago recibido no alcanza para cubrir el total."); return; }
        foreach (var line in cart)
        {
            var product = data.Productos.First(p => p.Id == line.ProductoId);
            if (product.Inventariable && product.Inventario < line.Cantidad) { Warn($"Ya no hay suficientes existencias de {product.Nombre}."); RefreshAll(); return; }
        }
        var sale = new Venta { Folio = data.Ventas.Count == 0 ? 1 : data.Ventas.Max(v => v.Folio) + 1, Fecha = DateTime.Now, Partidas = cart.Select(x => new PartidaVenta { ProductoId = x.ProductoId, Producto = x.Producto, Cantidad = x.Cantidad, PrecioUnitario = x.PrecioUnitario, Importe = x.Importe }).ToList(), Total = total, Pago = paymentBox.Value, Cambio = paymentBox.Value - total };
        foreach (var line in sale.Partidas)
        {
            var product = data.Productos.First(p => p.Id == line.ProductoId);
            if (product.Inventariable) product.Inventario -= line.Cantidad;
            data.Historial.Add(new Movimiento { Fecha = sale.Fecha, ProductoId = product.Id, Producto = product.Nombre, Accion = "Venta", Detalle = $"Folio {sale.Folio}: {line.Cantidad} unidad(es); existencias: {product.Inventario}." });
        }
        data.Ventas.Add(sale); Storage.Save(data); RefreshAll();
        MessageBox.Show($"Venta cobrada.\nFolio: {sale.Folio}\nTotal: {sale.Total:C2}\nCambio: {sale.Cambio:C2}", "Cobro realizado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        cart.Clear(); paymentBox.Value = 0; RefreshCart();
    }

    private void LoadProductForUpdate()
    {
        if (!int.TryParse(searchId.Text, out var id) || data.Productos.FirstOrDefault(p => p.Id == id) is not { } p) { Warn("No se encontró un producto con ese ID."); return; }
        updateNameBox.Text = p.Nombre; updateTypeBox.Text = p.Tipo; updatePriceBox.Value = Math.Min(updatePriceBox.Maximum, p.Precio); updateStockBox.Value = Math.Min(updateStockBox.Maximum, p.Inventario); updateTrackedBox.Checked = p.Inventariable;
    }

    private void UpdateProduct()
    {
        if (!int.TryParse(searchId.Text, out var id) || data.Productos.FirstOrDefault(p => p.Id == id) is not { } p) { Warn("Primero carga un producto válido por ID."); return; }
        if (string.IsNullOrWhiteSpace(updateNameBox.Text) || string.IsNullOrWhiteSpace(updateTypeBox.Text)) { Warn("El nombre y tipo son obligatorios."); return; }
        var oldStock = p.Inventario; p.Nombre = updateNameBox.Text.Trim(); p.Tipo = updateTypeBox.Text.Trim(); p.Precio = updatePriceBox.Value; p.Inventario = (int)updateStockBox.Value; p.Inventariable = updateTrackedBox.Checked;
        Record(p, "Actualización", $"Datos editados; existencias {oldStock} → {p.Inventario}."); Storage.Save(data); RefreshAll(); MessageBox.Show("Producto actualizado.", "Café Flujo");
    }

    private void ExportCsv()
    {
        using var dlg = new SaveFileDialog { Filter = "Archivo CSV para Excel (*.csv)|*.csv", FileName = "catalogo_cafeteria.csv" };
        if (dlg.ShowDialog() != DialogResult.OK) return;
        var rows = new List<string> { "ID,Nombre,Tipo,Precio MXN,Existencias,Control de inventario" };
        rows.AddRange(data.Productos.Select(p => $"{p.Id},{Csv(p.Nombre)},{Csv(p.Tipo)},{p.Precio.ToString("0.00", CultureInfo.InvariantCulture)},{p.Inventario},{(p.Inventariable ? "Sí" : "No")}"));
        File.WriteAllText(dlg.FileName, "\uFEFF" + string.Join(Environment.NewLine, rows), Encoding.UTF8); MessageBox.Show("Catálogo exportado. Puedes abrir el CSV en Excel.");
    }

    private void ExportHistoryCsv()
    {
        using var dlg = new SaveFileDialog { Filter = "Archivo CSV (*.csv)|*.csv", FileName = "historial_cafeteria.csv" }; if (dlg.ShowDialog() != DialogResult.OK) return;
        var rows = new List<string> { "Fecha,ID,Producto,Acción,Detalle" }; rows.AddRange(data.Historial.OrderByDescending(h => h.Fecha).Select(h => $"{h.Fecha:yyyy-MM-dd HH:mm:ss},{h.ProductoId},{Csv(h.Producto)},{Csv(h.Accion)},{Csv(h.Detalle)}"));
        File.WriteAllText(dlg.FileName, "\uFEFF" + string.Join(Environment.NewLine, rows), Encoding.UTF8); MessageBox.Show("Historial exportado.");
    }

    private void ExportPdf()
    {
        using var dlg = new PrintDialog { UseEXDialog = true, AllowSomePages = false };
        using var doc = new PrintDocument(); doc.DocumentName = "Catálogo de productos - Café Flujo"; doc.DefaultPageSettings.Landscape = true; doc.PrintPage += (_, e) =>
        {
            var g = e.Graphics!; float y = 55; using var title = new Font("Segoe UI", 18, FontStyle.Bold); using var font = new Font("Segoe UI", 9); using var bold = new Font("Segoe UI", 9, FontStyle.Bold);
            g.DrawString("Café Flujo | Catálogo de productos e inventario", title, Brushes.SaddleBrown, 50, y); y += 42; g.DrawString($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}", font, Brushes.Black, 50, y); y += 30;
            string[] headers = ["ID", "Producto", "Tipo", "Precio", "Existencias", "Inventariable"]; float[] xs = [50, 100, 380, 540, 660, 790];
            for (int i = 0; i < headers.Length; i++) g.DrawString(headers[i], bold, Brushes.Black, xs[i], y); y += 24; g.DrawLine(Pens.SaddleBrown, 50, y, 1000, y); y += 8;
            foreach (var p in data.Productos) { string[] cells = [p.Id.ToString(), p.Nombre, p.Tipo, p.Precio.ToString("C", CultureInfo.GetCultureInfo("es-MX")), p.Inventario.ToString(), p.Inventariable ? "Sí" : "No"]; for (int i = 0; i < cells.Length; i++) g.DrawString(cells[i], font, Brushes.Black, xs[i], y); y += 22; }
            e.HasMorePages = false;
        };
        dlg.Document = doc; if (dlg.ShowDialog() == DialogResult.OK) doc.Print();
    }

    private void PrepareEmail()
    {
        var address = Microsoft.VisualBasic.Interaction.InputBox("Correo destinatario (opcional):", "Preparar correo", "");
        var subject = Uri.EscapeDataString("Catálogo de Café Flujo"); var body = Uri.EscapeDataString("Adjunto el catálogo de productos exportado desde Café Flujo. Recuerda adjuntar el archivo CSV o PDF que generaste.");
        try { Process.Start(new ProcessStartInfo($"mailto:{Uri.EscapeDataString(address)}?subject={subject}&body={body}") { UseShellExecute = true }); }
        catch { MessageBox.Show("No se pudo abrir una aplicación de correo en este equipo."); }
    }

    private void Record(Producto p, string action, string detail) => data.Historial.Add(new Movimiento { Fecha = DateTime.Now, ProductoId = p.Id, Producto = p.Nombre, Accion = action, Detalle = detail });
    private void RefreshAll() { RefreshProducts(); historyGrid.DataSource = null; historyGrid.DataSource = data.Historial.OrderByDescending(h => h.Fecha).Select(h => new { Fecha = h.Fecha.ToString("dd/MM/yyyy HH:mm:ss"), h.ProductoId, h.Producto, h.Accion, h.Detalle }).ToList(); saleProduct.DataSource = null; saleProduct.DataSource = data.Productos.OrderBy(p => p.Nombre).ToList(); saleProduct.DisplayMember = nameof(Producto.Nombre); saleProduct.ValueMember = nameof(Producto.Id); RefreshCart(); }
    private void RefreshCart() { cartGrid.DataSource = null; cartGrid.DataSource = cart.Select(x => new { x.ProductoId, x.Producto, x.Cantidad, Precio = x.PrecioUnitario.ToString("C", CultureInfo.GetCultureInfo("es-MX")), Importe = x.Importe.ToString("C", CultureInfo.GetCultureInfo("es-MX")) }).ToList(); UpdateSaleSummary(); }
    private void UpdateSaleSummary() { var total = cart.Sum(x => x.Importe); totalLabel.Text = $"Total: {total.ToString("C2", CultureInfo.GetCultureInfo("es-MX"))}"; changeLabel.Text = $"Cambio: {Math.Max(0, paymentBox.Value - total).ToString("C2", CultureInfo.GetCultureInfo("es-MX"))}"; }
    private void RefreshProducts(string query = "") { var q = query.Trim(); productsGrid.DataSource = null; productsGrid.DataSource = data.Productos.Where(p => q.Length == 0 || p.Id.ToString().Contains(q) || p.Nombre.Contains(q, StringComparison.OrdinalIgnoreCase) || p.Tipo.Contains(q, StringComparison.OrdinalIgnoreCase)).OrderBy(p => p.Id).Select(p => new { p.Id, p.Nombre, p.Tipo, Precio = p.Precio.ToString("C", CultureInfo.GetCultureInfo("es-MX")), p.Inventario, Controlado = p.Inventariable ? "Sí" : "No" }).ToList(); }
    private void ClearEditor() { nameBox.Clear(); typeBox.SelectedIndex = -1; typeBox.Text = ""; priceBox.Value = 0; stockBox.Value = 0; trackedBox.Checked = true; }
    private static string Csv(string s) => "\"" + s.Replace("\"", "\"\"") + "\"";
    private static DataGridView Grid() => new() { ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false };
    private static Panel Card(int x, int y, int width, int height) => new() { Location = new(x, y), Size = new(width, height), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
    private static Control Heading(string text, string sub) { var panel = new Panel { Location = new(26, 18), Size = new(950, 72) }; panel.Controls.Add(new Label { Text = text, AutoSize = true, Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.FromArgb(91, 55, 37), Location = new(0, 0) }); panel.Controls.Add(new Label { Text = sub, AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = Color.DimGray, Location = new(2, 42) }); return panel; }
    private static void AddField(Control parent, string caption, Control field, int x, int y) { parent.Controls.Add(new Label { Text = caption, AutoSize = true, Location = new(x, y), Font = new Font("Segoe UI", 10, FontStyle.Bold) }); field.Location = new(x, y + 27); field.Font = new Font("Segoe UI", 10); parent.Controls.Add(field); }
    private static Button Button(string text, int x, int y, int width) => new() { Text = text, Location = new(x, y), Size = new(width, 36), BackColor = Color.FromArgb(112, 69, 45), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
    private static void Warn(string text) => MessageBox.Show(text, "Revisar información", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
