using PovraRegistra.Data;
using PovraRegistra.Models;
using System.Globalization;
using System.Windows.Input;

namespace PovraRegistra.ViewModels;

public class SkinFormViewModel : BaseNotifyViewModel
{
    private readonly ItemSkin _skin;

    private string _nome = string.Empty;
    private string _arma = string.Empty;
    private string _descricao = "Testada em Campo";
    private string _precoTexto = string.Empty;
    private string _imagemUrl = string.Empty;

    public List<string> Condicoes { get; } = new()
    {
        "Nova de Fábrica",
        "Pouco Usada",
        "Testada em Campo",
        "Bem Desgastada",
        "Veterana de Guerra"
    };

    public string Nome
    {
        get => _nome;
        set
        {
            if (_nome != value)
            {
                _nome = value;
                OnPropertyChanged();
            }
        }
    }

    public string Arma
    {
        get => _arma;
        set
        {
            if (_arma != value)
            {
                _arma = value;
                OnPropertyChanged();
            }
        }
    }

    public string Descricao
    {
        get => _descricao;
        set
        {
            if (_descricao != value)
            {
                _descricao = value;
                OnPropertyChanged();
            }
        }
    }

    public string PrecoTexto
    {
        get => _precoTexto;
        set
        {
            if (_precoTexto != value)
            {
                _precoTexto = value;
                OnPropertyChanged();
            }
        }
    }

    public string ImagemUrl
    {
        get => _imagemUrl;
        set
        {
            if (_imagemUrl != value)
            {
                _imagemUrl = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand SalvarCommand { get; }

    public SkinFormViewModel(ItemSkin? skin = null)
    {
        _skin = skin == null
            ? new ItemSkin()
            : new ItemSkin
            {
                Id = skin.Id,
                Nome = skin.Nome,
                Arma = skin.Arma,
                Descricao = skin.Descricao,
                Preco = skin.Preco,
                ImagemUrl = skin.ImagemUrl
            };

        Nome = _skin.Nome;
        Arma = _skin.Arma;

        Descricao = Condicoes.Contains(_skin.Descricao)
            ? _skin.Descricao
            : "Testada em Campo";

        PrecoTexto = _skin.Preco.ToString(
            "0.00",
            CultureInfo.GetCultureInfo("pt-BR"));

        ImagemUrl = _skin.ImagemUrl;

        SalvarCommand = new Command(async () => await Salvar());
    }

    private async Task Salvar()
    {
        if (string.IsNullOrWhiteSpace(Nome) ||
            string.IsNullOrWhiteSpace(Arma))
        {
            await MostrarAlerta(
                "Atenção",
                "Informe o nome da skin e a arma.");

            return;
        }

        if (!Condicoes.Contains(Descricao))
        {
            await MostrarAlerta(
                "Atenção",
                "Selecione o estado de conservação da skin.");

            return;
        }

        if (!TentarConverterPreco(PrecoTexto, out decimal preco) ||
            preco < 0)
        {
            await MostrarAlerta(
                "Atenção",
                "Informe um preço válido. Exemplo: 1500,50");

            return;
        }

        _skin.Nome = Nome.Trim();
        _skin.Arma = Arma.Trim();
        _skin.Descricao = Descricao;
        _skin.Preco = preco;
        _skin.ImagemUrl = ImagemUrl.Trim();

        await BancoDados.Instancia.InicializarAsync();
        await BancoDados.Instancia.SalvarSkinAsync(_skin);

        await MostrarAlerta(
            "Sucesso",
            "Skin salva com sucesso!");

        var pagina = Application.Current?.MainPage;

        if (pagina?.Navigation.NavigationStack.Count > 1)
            await pagina.Navigation.PopAsync();
    }

    private static bool TentarConverterPreco(
        string? texto,
        out decimal preco)
    {
        preco = 0;

        if (string.IsNullOrWhiteSpace(texto))
            return false;

        texto = texto.Trim()
            .Replace("R$", "", StringComparison.OrdinalIgnoreCase)
            .Replace(" ", "");

        // Quando há ponto e vírgula, considera o último separador
        // como o separador decimal.
        int ultimaVirgula = texto.LastIndexOf(',');
        int ultimoPonto = texto.LastIndexOf('.');

        if (ultimaVirgula >= 0 && ultimoPonto >= 0)
        {
            if (ultimaVirgula > ultimoPonto)
            {
                texto = texto.Replace(".", "");
                texto = texto.Replace(',', '.');
            }
            else
            {
                texto = texto.Replace(",", "");
            }
        }
        else if (ultimaVirgula >= 0)
        {
            texto = texto.Replace(',', '.');
        }

        return decimal.TryParse(
            texto,
            NumberStyles.AllowDecimalPoint |
            NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture,
            out preco);
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