namespace ConcursoWeb.Shared
{
    public class QuestionarioDto
    {
        public List<PreguntaDto> Preguntas { get; set; } = new();
    }

    public class PreguntaDto
    {
        public string Pregunta { get; set; } = string.Empty;
        public List<RespuestaDto> Respuestas { get; set; } = new();
    }

    public class RespuestaDto
    {
        public string Texto { get; set; } = string.Empty;
        public bool EsCorrecta { get; set; }
    }
}
