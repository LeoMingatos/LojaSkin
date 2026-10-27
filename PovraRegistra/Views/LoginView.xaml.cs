using PovraRegistra.ViewModels;

namespace PovraRegistra.Views;

public partial class LoginView : ContentPage
{
    public LoginView()
    {
        InitializeComponent();

        BindingContext = new UsuarioViewModel();
    }
}