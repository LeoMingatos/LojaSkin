namespace PovraRegistra.Models;

public class Historico
{
    [SQLite.PrimaryKey, SQLite.AutoIncrement] public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public int ItemId { get; set; }
    public string ItemNome { get; set; } = string.Empty;
    public DateTime DataHora { get; set; } = DateTime.Now;
}
