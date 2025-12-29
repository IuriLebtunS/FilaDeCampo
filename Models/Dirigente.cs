namespace FilaDeCampo.Models;

public class Dirigente
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public int OrdemRodizio { get; set; }
    public bool Ativo { get; set; } = true;
}