
using PovraRegistra.Data;
using PovraRegistra.Models;
using PovraRegistra.Singleton;
using PovraRegistra.Views;
using System.Windows.Input;

namespace PovraRegistra.ViewModels;

public class UsuarioViewModel : BaseNotifyViewModel
{
    private string nome = string.Empty;
    private string cpf = string.Empty;
    private string email = string.Empty;
    private string senha = string.Empty;
    private string tipo = "Cliente";
    private DateTime dataNascimento = DateTime.Today;
    private bool mostrarDados;
    private int usuarioId;

    public string Nome
    {
        get => nome;
        set
        {
            if (nome != value)
            {
                nome = value;
                OnPropertyChanged();
            }
        }
    }

    public string CPF
    {
        get => cpf;
        set
        {
            if (cpf != value)
            {
                cpf = value;
                OnPropertyChanged();
            }
        }
    }

    public string Email
    {
        get => email;
        set
        {
            if (email != value)
            {
                email = value;
                OnPropertyChanged();
            }
        }
    }

    public string Senha
    {
        get => senha;
        set
        {
            if (senha != value)
            {
                senha = value;
                OnPropertyChanged();
            }
        }
    }

    public string Tipo
    {
        get => tipo;
        set
        {
            if (tipo != value)
            {
                tipo = value;
                OnPropertyChanged();
            }
        }
    }

    public List<string> TiposUsuario { get; } = new()
    {
        "Cliente",
        "Administrador"
    };

    public DateTime DataNascimento
    {
        get => dataNascimento;
        set
        {
            if (dataNascimento != value)
            {
                dataNascimento = value;
                OnPropertyChanged();
            }
        }
    }

    public bool MostrarDados
    {
        get => mostrarDados;
        set
        {
            if (mostrarDados != value)
            {
                mostrarDados = value;
                OnPropertyChanged();
            }
        }
    }

    public string NomeLogado =>
        UsuarioSingleton.GetInstance().Usuario?.Nome ?? string.Empty;

    public ICommand EntrarCommand { get; }
    public ICommand RegistrarCommand { get; }
    public ICommand SalvarCommand { get; }
    public ICommand AtualizarCommand { get; }
    public ICommand VisualizarCommand { get; }

    public UsuarioViewModel()
    {
        EntrarCommand = new Command(async () => await Entrar());

        RegistrarCommand = new Command(async () =>
        {
            var pagina = Application.Current?.MainPage;

            if (pagina != null)
                await pagina.Navigation.PushAsync(new CadastroView());
        });

        SalvarCommand = new Command(async () => await Salvar());
        AtualizarCommand = new Command(async () => await EditarUsuario());

        VisualizarCommand = new Command(() =>
        {
            MostrarDados = !MostrarDados;
        });

        CarregarUsuario();
    }

    public void CarregarUsuario()
    {
        var usuario = UsuarioSingleton.GetInstance().Usuario;

        if (usuario == null)
            return;

        usuarioId = usuario.Id;
        Nome = usuario.Nome;
        CPF = usuario.CPF;
        Email = usuario.Email;
        Senha = usuario.Senha;
        Tipo = usuario.Tipo;
        DataNascimento = usuario.DataNascimento;
    }

    private async Task Entrar()
    {
        var pagina = Application.Current?.MainPage;

        if (pagina == null)
            return;

        if (string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Senha))
        {
            await ExibirAlerta(
                "Atenção",
                "Informe o e-mail e a senha.");
            return;
        }

        await BancoDados.Instancia.InicializarAsync();

        string emailNormalizado = Email.Trim().ToLowerInvariant();

        var usuario = await BancoDados.Instancia
            .BuscarUsuarioAsync(emailNormalizado);

        if (usuario == null || usuario.Senha != Senha)
        {
            await ExibirAlerta(
                "Erro",
                "E-mail ou senha inválidos.");
            return;
        }

        UsuarioSingleton.GetInstance().Usuario = usuario;

        await pagina.Navigation.PushAsync(new PrincipalView());
    }

    private async Task Salvar()
    {
        if (!await ValidarCamposAsync())
            return;

        await BancoDados.Instancia.InicializarAsync();

        string emailNormalizado = Email.Trim().ToLowerInvariant();

        var existente = await BancoDados.Instancia
            .BuscarUsuarioAsync(emailNormalizado);

        if (existente != null)
        {
            await ExibirAlerta(
                "Atenção",
                "Já existe um usuário com esse e-mail.");
            return;
        }

        var usuario = new Usuario
        {
            Nome = Nome.Trim(),
            CPF = ApenasNumeros(CPF),
            Email = emailNormalizado,
            Senha = Senha,
            Tipo = Tipo,
            DataNascimento = DataNascimento.Date
        };

        await BancoDados.Instancia.SalvarUsuarioAsync(usuario);

        await ExibirAlerta(
            "Sucesso",
            "Usuário cadastrado com sucesso!");

        LimparCampos();

        var pagina = Application.Current?.MainPage;

        if (pagina?.Navigation.NavigationStack.Count > 1)
            await pagina.Navigation.PopAsync();
    }

    private async Task EditarUsuario()
    {
        if (usuarioId == 0)
        {
            await ExibirAlerta(
                "Atenção",
                "Nenhum usuário foi carregado para edição.");
            return;
        }

        if (!await ValidarCamposAsync())
            return;

        await BancoDados.Instancia.InicializarAsync();

        string emailNormalizado = Email.Trim().ToLowerInvariant();

        var existente = await BancoDados.Instancia
            .BuscarUsuarioAsync(emailNormalizado);

        if (existente != null && existente.Id != usuarioId)
        {
            await ExibirAlerta(
                "Atenção",
                "Esse e-mail já está sendo usado por outro usuário.");
            return;
        }

        var usuarioAtualizado = new Usuario
        {
            Id = usuarioId,
            Nome = Nome.Trim(),
            CPF = ApenasNumeros(CPF),
            Email = emailNormalizado,
            Senha = Senha,
            Tipo = Tipo,
            DataNascimento = DataNascimento.Date
        };

        await BancoDados.Instancia.SalvarUsuarioAsync(usuarioAtualizado);

        var singleton = UsuarioSingleton.GetInstance();

        if (singleton.Usuario?.Id == usuarioId)
        {
            singleton.Usuario = usuarioAtualizado;
            OnPropertyChanged(nameof(NomeLogado));
        }

        await ExibirAlerta(
            "Sucesso",
            "Usuário atualizado com sucesso!");

        var pagina = Application.Current?.MainPage;

        if (pagina?.Navigation.NavigationStack.Count > 1)
            await pagina.Navigation.PopAsync();
    }

    private async Task<bool> ValidarCamposAsync()
    {
        if (string.IsNullOrWhiteSpace(Nome) ||
            string.IsNullOrWhiteSpace(CPF) ||
            string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Senha))
        {
            await ExibirAlerta(
                "Atenção",
                "Preencha todos os campos.");
            return false;
        }

        if (!ValidarTipo())
        {
            await ExibirAlerta(
                "Atenção",
                "Selecione um tipo de usuário válido.");
            return false;
        }

        if (ApenasNumeros(CPF).Length != 11)
        {
            await ExibirAlerta(
                "Atenção",
                "Informe um CPF com exatamente 11 números.");
            return false;
        }

        string emailNormalizado = Email.Trim();

        if (!emailNormalizado.Contains('@') ||
            !emailNormalizado.Contains('.'))
        {
            await ExibirAlerta(
                "Atenção",
                "Informe um e-mail válido.");
            return false;
        }

        if (DataNascimento.Date > DateTime.Today)
        {
            await ExibirAlerta(
                "Atenção",
                "A data de nascimento não pode ser futura.");
            return false;
        }

        return true;
    }

    private bool ValidarTipo() =>
        !string.IsNullOrWhiteSpace(Tipo) &&
        TiposUsuario.Contains(Tipo);

    private static string ApenasNumeros(string valor) =>
        new string(valor.Where(char.IsDigit).ToArray());

    private async Task ExibirAlerta(string titulo, string mensagem)
    {
        var pagina = Application.Current?.MainPage;

        if (pagina != null)
            await pagina.DisplayAlert(titulo, mensagem, "OK");
    }

    private void LimparCampos()
    {
        usuarioId = 0;
        Nome = string.Empty;
        CPF = string.Empty;
        Email = string.Empty;
        Senha = string.Empty;
        Tipo = "Cliente";
        DataNascimento = DateTime.Today;
    }
}
