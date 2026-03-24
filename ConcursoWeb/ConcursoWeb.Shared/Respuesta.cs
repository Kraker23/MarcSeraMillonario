using System.Text.Json.Serialization;

namespace ConcursoWeb.Shared
{
    public class Respuesta
    {
        public Guid IdRespuesta { get; set; }

        [JsonPropertyName("texto")]
        public string textoRespuesta { get; set; }
        public bool esCorrecta { get; set; }
    }
}
