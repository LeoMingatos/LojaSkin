namespace PovraRegistra.Models;

public class Usuario
{
    [SQLite.PrimaryKey, SQLite.AutoIncrement] public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string Tipo { get; set; } = "Cliente";
    public DateTime DataNascimento { get; set; } = DateTime.Today;
}
