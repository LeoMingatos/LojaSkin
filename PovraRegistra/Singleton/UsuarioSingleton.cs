using PovraRegistra.Models;

namespace PovraRegistra.Singleton;

public sealed class UsuarioSingleton
{
    private static readonly Lazy<UsuarioSingleton> _instancia = new(() => new UsuarioSingleton());
    public static UsuarioSingleton GetInstance() => _instancia.Value;
    public Usuario? Usuario { get; set; }
    private UsuarioSingleton() { }
}
