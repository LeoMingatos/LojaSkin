using PovraRegistra.Data;
using PovraRegistra.Models;
using PovraRegistra.Singleton;
using PovraRegistra.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PovraRegistra.ViewModels;

public class ItemViewModel : BaseNotifyViewModel
{
    public ObservableCollection<ItemSkin> Skins { get; } = new();

    public ObservableCollection<Historico> Historicos { get; } = new();

    private ItemSkin? skinSelecionada;

    public ItemSkin? SkinSelecionada
    {
        get => skinSelecionada;
        set
        {
            skinSelecionada = value;
            OnPropertyChanged();
        }
    }

    public string Nome { get; set; } = string.Empty;
    public string Arma { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string PrecoTexto { get; set; } = string.Empty;
    public string ImagemUrl { get; set; } = string.Empty;

    public ICommand NovoCommand { get; }
    public ICommand EditarCommand { get; }
    public ICommand ExcluirCommand { get; }
    public ICommand SelecionarCommand { get; }
    public ICommand HistoricoCommand { get; }

    public ItemViewModel()
    {
        NovoCommand = new Command(async () =>
            await AbrirFormulario(null));

        EditarCommand = new Command<ItemSkin>(
            async skin => await AbrirFormulario(skin));

        ExcluirCommand = new Command<ItemSkin>(
            async skin => await Excluir(skin));

        SelecionarCommand = new Command<ItemSkin>(
            async skin => await Selecionar(skin));

        HistoricoCommand = new Command(async () =>
            await AbrirHistorico());
    }

    public async Task CarregarSkinsAsync()
    {
        await BancoDados.Instancia.InicializarAsync();

        var lista = await BancoDados.Instancia.ListarSkinsAsync();

        Skins.Clear();

        foreach (var skin in lista)
            Skins.Add(skin);
    }

    public async Task CarregarHistoricoAsync()
    {
        await BancoDados.Instancia.InicializarAsync();

        var usuario = UsuarioSingleton.GetInstance().Usuario;

        List<Historico> lista;

        if (usuario == null)
        {
            lista = new List<Historico>();
        }
        else if (usuario.Tipo == "Administrador")
        {
            lista = await BancoDados.Instancia.ListarHistoricoAsync();
        }
        else
        {
            lista = await BancoDados.Instancia
                .ListarHistoricoUsuarioAsync(usuario.Id);
        }

        Historicos.Clear();

        foreach (var registro in lista)
            Historicos.Add(registro);
    }

    private async Task AbrirFormulario(ItemSkin? skin)
    {
        await BancoDados.Instancia.InicializarAsync();

        var pagina = Application.Current?.MainPage;

        if (pagina == null)
            return;

        await pagina.Navigation.PushAsync(
            new SkinFormView(skin));
    }

    private async Task Excluir(ItemSkin? skin)
    {
        if (skin == null)
            return;

        var usuario = UsuarioSingleton.GetInstance().Usuario;

        if (usuario?.Tipo != "Administrador")
        {
            await MostrarAlerta(
                "Acesso negado",
                "Somente administradores podem excluir skins.");

            return;
        }

        await BancoDados.Instancia.InicializarAsync();

        var pagina = Application.Current?.MainPage;

        if (pagina == null)
            return;

        bool confirmar = await pagina.DisplayAlert(
            "Confirmar exclusão",
            $"Deseja excluir a skin {skin.Nome}?",
            "Excluir",
            "Cancelar");

        if (!confirmar)
            return;

        await BancoDados.Instancia.ExcluirSkinAsync(skin);

        await CarregarSkinsAsync();

        await MostrarAlerta(
            "Sucesso",
            "Skin excluída com sucesso.");
    }

    private async Task Selecionar(ItemSkin? skin)
    {
        if (skin == null)
            return;

        var usuario = UsuarioSingleton.GetInstance().Usuario;

        if (usuario == null)
        {
            await MostrarAlerta(
                "Atenção",
                "Faça login para selecionar uma skin.");

            return;
        }

        if (usuario.Tipo != "Cliente")
        {
            await MostrarAlerta(
                "Ação indisponível",
                "A seleção de skins está disponível apenas para clientes.");

            return;
        }

        await BancoDados.Instancia.InicializarAsync();

        var pagina = Application.Current?.MainPage;

        if (pagina == null)
            return;

        bool confirmar = await pagina.DisplayAlert(
            "Confirmar seleção",
            $"Deseja selecionar {skin.Nome} por {skin.Preco:C}?",
            "Confirmar",
            "Cancelar");

        if (!confirmar)
            return;

        var historico = new Historico
        {
            UsuarioId = usuario.Id,
            UsuarioNome = usuario.Nome,
            ItemId = skin.Id,
            ItemNome = skin.Nome,
            DataHora = DateTime.Now
        };

        await BancoDados.Instancia.RegistrarSelecaoAsync(historico);

        await MostrarAlerta(
            "Sucesso",
            "Seleção registrada no histórico.");
    }

    private async Task AbrirHistorico()
    {
        await BancoDados.Instancia.InicializarAsync();

        var pagina = Application.Current?.MainPage;

        if (pagina == null)
            return;

        await pagina.Navigation.PushAsync(
            new HistoricoView());
    }

    private static async Task MostrarAlerta(
        string titulo,
        string mensagem)
    {
        var pagina = Application.Current?.MainPage;

        if (pagina != null)
        {
            await pagina.DisplayAlert(
                titulo,
                mensagem,
                "OK");
        }
    }
}