
using PovraRegistra.Models;
using SQLite;

namespace PovraRegistra.Data;

public sealed class BancoDados
{
    private static readonly Lazy<BancoDados> _instancia =
        new(() => new BancoDados());

    public static BancoDados Instancia => _instancia.Value;

    private readonly SQLiteAsyncConnection _db;
    private bool _inicializado;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private BancoDados()
    {
        var caminho = Path.Combine(
            FileSystem.AppDataDirectory,
            "povraregistra.db3");

        _db = new SQLiteAsyncConnection(caminho);
    }

    public async Task InicializarAsync()
    {
        if (_inicializado)
            return;

        await _lock.WaitAsync();

        try
        {
            if (_inicializado)
                return;

            await _db.CreateTableAsync<Usuario>();
            await _db.CreateTableAsync<ItemSkin>();
            await _db.CreateTableAsync<Historico>();

            // Cria o administrador padrão somente se ele não existir.
            var administrador = await BuscarUsuarioAsync("adm");

            if (administrador == null)
            {
                administrador = new Usuario
                {
                    Nome = "Administrador",
                    CPF = "00000000000",
                    Email = "adm",
                    Senha = "123",
                    Tipo = "Administrador",
                    DataNascimento = DateTime.Today
                };

                await _db.InsertAsync(administrador);
            }

            _inicializado = true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public Task<List<Usuario>> ListarUsuariosAsync()
    {
        return _db.Table<Usuario>().ToListAsync();
    }

    public Task<Usuario?> BuscarUsuarioAsync(string email)
    {
        string emailNormalizado = email.Trim().ToLowerInvariant();

        return _db.Table<Usuario>()
            .Where(u => u.Email == emailNormalizado)
            .FirstOrDefaultAsync();
    }

    public Task<int> SalvarUsuarioAsync(Usuario usuario)
    {
        usuario.Email = usuario.Email.Trim().ToLowerInvariant();

        return usuario.Id == 0
            ? _db.InsertAsync(usuario)
            : _db.UpdateAsync(usuario);
    }

    public Task<int> SalvarSkinAsync(ItemSkin skin)
    {
        return skin.Id == 0
            ? _db.InsertAsync(skin)
            : _db.UpdateAsync(skin);
    }

    public Task<int> ExcluirSkinAsync(ItemSkin skin)
    {
        return _db.DeleteAsync(skin);
    }

    public Task<List<ItemSkin>> ListarSkinsAsync()
    {
        return _db.Table<ItemSkin>()
            .OrderBy(s => s.Nome)
            .ToListAsync();
    }

    public Task<int> RegistrarSelecaoAsync(Historico historico)
    {
        return _db.InsertAsync(historico);
    }

    public Task<List<Historico>> ListarHistoricoAsync()
    {
        return _db.Table<Historico>()
            .OrderByDescending(h => h.DataHora)
            .ToListAsync();
    }

    public Task<List<Historico>> ListarHistoricoUsuarioAsync(int usuarioId)
    {
        return _db.Table<Historico>()
            .Where(h => h.UsuarioId == usuarioId)
            .OrderByDescending(h => h.DataHora)
            .ToListAsync();
    }
}
