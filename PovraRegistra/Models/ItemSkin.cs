namespace PovraRegistra.Models;

public class ItemSkin
{
    [SQLite.PrimaryKey, SQLite.AutoIncrement] public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Arma { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public string ImagemUrl { get; set; } = string.Empty;
}
