namespace FilaDeCampo.Models;

public class ConfiguracaoAudioVideo
{
    public int Id { get; set; }
    public int? UltimoOperadorId { get; set; }
    public int? UltimoAjudanteId { get; set; }
    public int CongregacaoId { get; set; }
    public Congregacao Congregacao { get; set; } = null!;
}