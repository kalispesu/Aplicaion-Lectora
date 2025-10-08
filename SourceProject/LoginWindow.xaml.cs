using System.Windows;
using System.Windows.Controls;
using System;

namespace LectorPdf
{
    public partial class LoginWindow : Window
    {
        private enum Mode { Login, Register }
        private Mode currentMode;

        public LoginWindow()
        {
            InitializeComponent();
            ShowLoginForm();
        }

        private void ClearPanel() => panelAuth.Children.Clear();

        private void ShowLoginForm()
        {
            currentMode = Mode.Login;
            ClearPanel();
            var lblEmail = new TextBlock { Text = "Correo:", Margin = new Thickness(0,6,0,0) };
            var txtEmail = new TextBox { Name = "txtEmail", Width = 380 };
            var lblPass = new TextBlock { Text = "Contraseña:", Margin = new Thickness(0,6,0,0) };
            var txtPass = new PasswordBox { Name = "txtPass", Width = 380 };
            var btnSubmit = new Button { Content = "Entrar", Width = 100, Margin = new Thickness(0,8,0,0), HorizontalAlignment = HorizontalAlignment.Right };
            btnSubmit.Click += (s,e) => {
                var email = txtEmail.Text.Trim();
                var pass = txtPass.Password;
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass)) { txtStatus.Text = "Rellena todos los campos."; return; }
                var um = new UserManager();
                if (um.VerifyCredentials(email, pass))
                {
                    OpenMainWindow(email);
                }
                else txtStatus.Text = "Credenciales incorrectas.";
            };

            panelAuth.Children.Add(lblEmail);
            panelAuth.Children.Add(txtEmail);
            panelAuth.Children.Add(lblPass);
            panelAuth.Children.Add(txtPass);
            panelAuth.Children.Add(btnSubmit);
        }

        private void ShowRegisterForm()
        {
            currentMode = Mode.Register;
            ClearPanel();
            var lblEmail = new TextBlock { Text = "Correo:", Margin = new Thickness(0,6,0,0) };
            var txtEmail = new TextBox { Name = "txtEmail", Width = 380 };
            var lblPass = new TextBlock { Text = "Contraseña:", Margin = new Thickness(0,6,0,0) };
            var txtPass = new PasswordBox { Name = "txtPass", Width = 380 };
            var lblAge = new TextBlock { Text = "Edad (opcional):", Margin = new Thickness(0,6,0,0) };
            var txtAge = new TextBox { Name = "txtAge", Width = 120 };
            var btnSubmit = new Button { Content = "Registrar", Width = 100, Margin = new Thickness(0,8,0,0), HorizontalAlignment = HorizontalAlignment.Right };
            btnSubmit.Click += (s,e) => {
                var email = txtEmail.Text.Trim();
                var pass = txtPass.Password;
                int? age = null;
                if (!string.IsNullOrWhiteSpace(txtAge.Text)) { if (int.TryParse(txtAge.Text, out int a)) age = a; }
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass)) { txtStatus.Text = "Correo y contraseña obligatorios."; return; }
                var um = new UserManager();
                if (um.UserExists(email)) { txtStatus.Text = "El usuario ya existe."; return; }
                um.CreateUser(email, pass, age);
                txtStatus.Text = "Registrado correctamente. Puedes iniciar sesión.";
                ShowLoginForm();
            };

            panelAuth.Children.Add(lblEmail);
            panelAuth.Children.Add(txtEmail);
            panelAuth.Children.Add(lblPass);
            panelAuth.Children.Add(txtPass);
            panelAuth.Children.Add(lblAge);
            panelAuth.Children.Add(txtAge);
            panelAuth.Children.Add(btnSubmit);
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "";
            ShowLoginForm();
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "";
            ShowRegisterForm();
        }

        private void btnGuest_Click(object sender, RoutedEventArgs e)
        {
            OpenMainWindow("Invitado");
        }

        private void OpenMainWindow(string email)
        {
            var main = new MainWindow(email);
            main.Show();
            this.Close();
        }
    }
}
