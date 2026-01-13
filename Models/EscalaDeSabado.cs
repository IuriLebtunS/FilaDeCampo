namespace FilaDeCampo.Models;

public class EscalaDeSabado
{
    public int Id { get; set; }
    public DateTime Data { get; set; }
    public int DirigenteId { get; set; }
    public Dirigente Dirigente { get; set; } = null!;
    public string? Observacao { get; set; }
}