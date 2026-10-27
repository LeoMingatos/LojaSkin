
using PovraRegistra.Data;
using PovraRegistra.Models;
using PovraRegistra.Singleton;
using PovraRegistra.ViewModels;

namespace PovraRegistra.Views;

public partial class PrincipalView : ContentPage
{
    private readonly ItemViewModel vm = new();

    public string Nome =>
        UsuarioSingleton.GetInstance().Usuario?.Nome ?? string.Empty;

    public string Tipo =>
        UsuarioSingleton.GetInstance().Usuario?.Tipo ?? string.Empty;

    public bool AdministradorLogado =>
        UsuarioSingleton.GetInstance().Usuario?.Tipo == "Administrador";

    public bool ClienteLogado =>
        UsuarioSingleton.GetInstance().Usuario?.Tipo == "Cliente";

    public PrincipalView()
    {
        InitializeComponent();
        BindingContext = this;
        AtualizarPermissoes();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        AtualizarPermissoes();

        await vm.CarregarSkinsAsync();
        ListaSkins.ItemsSource = vm.Skins;
    }

    private void AtualizarPermissoes()
    {
        OnPropertyChanged(nameof(Nome));
        OnPropertyChanged(nameof(Tipo));
        OnPropertyChanged(nameof(AdministradorLogado));
        OnPropertyChanged(nameof(ClienteLogado));

        if (BotaoNova != null)
            BotaoNova.IsVisible = AdministradorLogado;
    }

    private async void AtualizarClicked(object sender, EventArgs e)
    {
        await vm.CarregarSkinsAsync();
        ListaSkins.ItemsSource = vm.Skins;
    }

    private async void NovaSkinClicked(object sender, EventArgs e)
    {
        if (!EhAdministrador())
        {
            await DisplayAlert(
                "Acesso negado",
                "Somente administradores podem cadastrar skins.",
                "OK");
            return;
        }

        await Navigation.PushAsync(new SkinFormView(null));
    }

    private async void EditarClicked(object sender, EventArgs e)
    {
        if (!EhAdministrador())
        {
            await DisplayAlert(
                "Acesso negado",
                "Somente administradores podem editar skins.",
                "OK");
            return;
        }

        if (sender is Button botao &&
            botao.CommandParameter is ItemSkin skin)
        {
            await Navigation.PushAsync(new SkinFormView(skin));
        }
    }

    private async void ExcluirClicked(object sender, EventArgs e)
    {
        if (!EhAdministrador())
        {
            await DisplayAlert(
                "Acesso negado",
                "Somente administradores podem excluir skins.",
                "OK");
            return;
        }

        if (sender is not Button botao ||
            botao.CommandParameter is not ItemSkin skin)
            return;

        bool confirmar = await DisplayAlert(
            "Confirmar exclusão",
            $"Deseja excluir a skin {skin.Nome}?",
            "Excluir",
            "Cancelar");

        if (!confirmar)
            return;

        await BancoDados.Instancia.InicializarAsync();
        await BancoDados.Instancia.ExcluirSkinAsync(skin);

        await vm.CarregarSkinsAsync();
        ListaSkins.ItemsSource = vm.Skins;
    }

    private async void SelecionarClicked(object sender, EventArgs e)
    {
        var usuario = UsuarioSingleton.GetInstance().Usuario;

        if (usuario == null)
        {
            await DisplayAlert(
                "Atenção",
                "Faça login para selecionar uma skin.",
                "OK");
            return;
        }

        if (usuario.Tipo != "Cliente")
        {
            await DisplayAlert(
                "Ação indisponível",
                "A seleção de skins está disponível apenas para clientes.",
                "OK");
            return;
        }

        if (sender is not Button botao ||
            botao.CommandParameter is not ItemSkin skin)
            return;

        bool confirmar = await DisplayAlert(
            "Confirmar seleção",
            $"Deseja selecionar {skin.Nome} por {skin.Preco:C}?",
            "Confirmar",
            "Cancelar");

        if (!confirmar)
            return;

        await BancoDados.Instancia.InicializarAsync();

        await BancoDados.Instancia.RegistrarSelecaoAsync(new Historico
        {
            UsuarioId = usuario.Id,
            UsuarioNome = usuario.Nome,
            ItemId = skin.Id,
            ItemNome = skin.Nome,
            DataHora = DateTime.Now
        });

        await DisplayAlert(
            "Sucesso",
            "Seleção registrada no histórico.",
            "OK");
    }

    private async void HistoricoClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new HistoricoView());
    }

    private bool EhAdministrador() =>
        UsuarioSingleton.GetInstance().Usuario?.Tipo == "Administrador";

    private async void SairClicked(object sender, EventArgs e)
    {
        UsuarioSingleton.GetInstance().Usuario = null;

        await Navigation.PopToRootAsync();
    }
}
