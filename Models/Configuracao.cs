namespace FilaDeCampo.Models;

public class Configuracao
{
    public int Id { get; set; }
    public int UltimoDirigenteId { get; set; }
    public int CongregacaoId { get; set; }  
    public Congregacao Congregacao { get; set; } = null!;
}