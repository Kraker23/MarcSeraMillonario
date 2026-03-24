namespace ConcursoWeb.Shared
{
    public class Pregunta
    {
        public Guid IdPregunta { get; set; }
        public string pregunta { get; set; }
        public List<Respuesta> respuestas { get; set; }
    }
}
