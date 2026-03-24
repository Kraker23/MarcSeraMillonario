namespace ConcursoWeb.Shared
{
    public class PremiosConfig
    {
        public List<Premio> premios { get; set; } = new();
    }

    public class Premio
    {
        public int nivel { get; set; }
        public string texto { get; set; } = string.Empty;
    }
}
