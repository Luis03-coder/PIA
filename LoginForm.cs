namespace CafeteriaApp;

public sealed class LoginForm : Form
{
    private readonly TextBox user = new() { PlaceholderText = "Usuario", Width = 260 };
    private readonly TextBox pass = new() { PlaceholderText = "Contraseña", UseSystemPasswordChar = true, Width = 260 };

    public LoginForm()
    {
        Text = "Café Flujo | Acceso"; StartPosition = FormStartPosition.CenterScreen; ClientSize = new(420, 360); FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; BackColor = Color.FromArgb(247, 243, 236);
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(55, 35, 55, 25) }; Controls.Add(panel);
        var title = new Label { Text = "☕  CAFÉ FLUJO", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Color.FromArgb(91, 55, 37), AutoSize = true, Location = new(58, 24) }; panel.Controls.Add(title);
        panel.Controls.Add(new Label { Text = "Administración de productos e inventario", AutoSize = true, Location = new(36, 75), ForeColor = Color.DimGray });
        user.Location = new(25, 125); pass.Location = new(25, 170); panel.Controls.Add(user); panel.Controls.Add(pass);
        var enter = new Button { Text = "Iniciar sesión", Location = new(25, 220), Size = new(260, 40), BackColor = Color.FromArgb(112, 69, 45), ForeColor = Color.White, FlatStyle = FlatStyle.Flat }; enter.Click += (_, _) => Login(); panel.Controls.Add(enter);
        var register = new Button { Text = "Crear nuevo usuario", Location = new(25, 268), Size = new(260, 32), ForeColor = Color.FromArgb(112, 69, 45), FlatStyle = FlatStyle.Flat }; register.Click += (_, _) => Register(); panel.Controls.Add(register);
        panel.Controls.Add(new Label { Text = "Acceso inicial: admin / cafe123", AutoSize = true, Location = new(36, 312), ForeColor = Color.Gray });
        AcceptButton = enter;
    }

    private void Login()
    {
        var data = Storage.Load();
        var name = user.Text.Trim();
        var found = data.Usuarios.FirstOrDefault(u => u.Nombre.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (found is not null && Seguridad.Verificar(pass.Text, found)) { Hide(); using var main = new MainForm(); main.ShowDialog(); Close(); }
        else MessageBox.Show("Usuario o contraseña incorrectos.", "Acceso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void Register()
    {
        using var dlg = new RegisterDialog();
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        var data = Storage.Load();
        if (data.Usuarios.Any(u => u.Nombre.Equals(dlg.UserName, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show("Ese usuario ya existe.", "Crear usuario", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var (salt, hash) = Seguridad.CrearHash(dlg.Password);
        data.Usuarios.Add(new Usuario { Nombre = dlg.UserName, Salt = salt, Hash = hash });
        Storage.Save(data);
        MessageBox.Show("Usuario creado. Ya puedes iniciar sesión.", "Crear usuario", MessageBoxButtons.OK, MessageBoxIcon.Information);
        user.Text = dlg.UserName; pass.Clear(); pass.Focus();
    }
}

// Ventana pequeña para registrar un usuario nuevo.
internal sealed class RegisterDialog : Form
{
    private readonly TextBox name = new() { PlaceholderText = "Nuevo usuario", Width = 260 };
    private readonly TextBox pass1 = new() { PlaceholderText = "Contraseña (mínimo 6 caracteres)", UseSystemPasswordChar = true, Width = 260 };
    private readonly TextBox pass2 = new() { PlaceholderText = "Confirmar contraseña", UseSystemPasswordChar = true, Width = 260 };

    public string UserName => name.Text.Trim();
    public string Password => pass1.Text;

    public RegisterDialog()
    {
        Text = "Crear usuario"; StartPosition = FormStartPosition.CenterParent; ClientSize = new(320, 235); FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false; BackColor = Color.FromArgb(247, 243, 236);
        name.Location = new(30, 30); pass1.Location = new(30, 75); pass2.Location = new(30, 120);
        var ok = new Button { Text = "Guardar usuario", Location = new(30, 170), Size = new(260, 40), BackColor = Color.FromArgb(112, 69, 45), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        ok.Click += (_, _) => Confirm();
        Controls.AddRange([name, pass1, pass2, ok]);
        AcceptButton = ok;
    }

    private void Confirm()
    {
        if (UserName.Length < 3) { Warn("El usuario debe tener al menos 3 caracteres."); return; }
        if (pass1.Text.Length < 6) { Warn("La contraseña debe tener al menos 6 caracteres."); return; }
        if (pass1.Text != pass2.Text) { Warn("Las contraseñas no coinciden."); return; }
        DialogResult = DialogResult.OK;
    }

    private static void Warn(string text) => MessageBox.Show(text, "Revisar información", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
