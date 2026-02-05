using FilaDeCampo.Models.Enums;

namespace FilaDeCampo.Models;

public class EscalaAudioVideo
{
    public int Id { get; set; }
    public DateTime Data { get; set; }
    public int TecnicoId { get; set; }
    public TecnicoAudioVideo Tecnico { get; set; } = null!;
    public FuncaoAudioVideo Funcao { get; set; }
    public int CongregacaoId { get; set; }
}