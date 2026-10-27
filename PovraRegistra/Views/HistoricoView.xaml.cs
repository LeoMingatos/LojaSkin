using PovraRegistra.Data;
using PovraRegistra.Singleton;

namespace PovraRegistra.Views;

public partial class HistoricoView : ContentPage
{
    public HistoricoView()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await BancoDados.Instancia.InicializarAsync();

        var usuario = UsuarioSingleton.GetInstance().Usuario;

        if (usuario == null)
        {
            await DisplayAlert(
                "Atenção",
                "Nenhum usuário está conectado. Faça login novamente.",
                "OK");

            await Navigation.PopToRootAsync();
            return;
        }

        if (usuario.Tipo == "Administrador")
        {
            // O administrador visualiza todas as seleções.
            var historico =
                await BancoDados.Instancia.ListarHistoricoAsync();

            ListaHistorico.ItemsSource = historico;
        }
        else
        {
            // O cliente visualiza somente o próprio histórico.
            var historico =
                await BancoDados.Instancia.ListarHistoricoUsuarioAsync(
                    usuario.Id);

            ListaHistorico.ItemsSource = historico;
        }
    }
}