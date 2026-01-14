namespace FilaDeCampo.Models
{
    public class Congregacao
    {
        public int Id { get; set; }
        public string Nome { get; set; } = null!;
        public string ChaveAcesso { get; set; } = null!;
        public bool Ativa { get; set; } = true;
        public List<Dirigente> Dirigentes { get; set; } = new();
    }
}