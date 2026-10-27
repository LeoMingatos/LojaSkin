using PovraRegistra.ViewModels;

namespace PovraRegistra.Views;

public partial class CadastroView : ContentPage
{
    private readonly UsuarioViewModel vm;

    public CadastroView()
    {
        InitializeComponent();

        vm = new UsuarioViewModel();

        BindingContext = vm;
    }
}