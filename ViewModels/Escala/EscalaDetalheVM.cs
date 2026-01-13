namespace FilaDeCampo.ViewModels.Escala;

public class EscalaDetalheVM
{
    public int Mes { get; set; }
    public int Ano { get; set; }
    public List<EscalaDiaVM> Sabados { get; set; } = new();
}

public class EscalaDiaVM
{
    public int Id { get; set; }          // 👈 Id da Escala
    public DateTime Data { get; set; }
    public int DirigenteId { get; set; }
    public string Dirigente { get; set; } = string.Empty;
}