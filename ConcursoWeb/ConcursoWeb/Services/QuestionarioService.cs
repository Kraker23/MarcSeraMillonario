using ConcursoWeb.Shared;
using System.Text.Json;

namespace ConcursoWeb.Services
{
    public class QuestionarioService
    {
        private readonly IWebHostEnvironment _env;
        private List<Pregunta> _preguntas;

        public QuestionarioService(IWebHostEnvironment env)
        {
            _env = env;
            CargarPreguntas();
        }

        private void CargarPreguntas()
        {
            var filePath = Path.Combine(_env.ContentRootPath, "preguntas.json");
            var json = File.ReadAllText(filePath);
            var questionario = JsonSerializer.Deserialize<Questionario>(json);
            _preguntas = questionario?.preguntas ?? new List<Pregunta>();
            
            // Asignar IDs únicos a preguntas y respuestas
            foreach (var pregunta in _preguntas)
            {
                pregunta.IdPregunta = Guid.NewGuid();
                foreach (var respuesta in pregunta.respuestas)
                {
                    respuesta.IdRespuesta = Guid.NewGuid();
                }
            }
        }

        public List<Pregunta> ObtenerPreguntasAleatorias(int cantidad)
        {
            var random = new Random();
            return _preguntas.OrderBy(x => random.Next()).Take(cantidad).ToList();
        }

        public Pregunta? ObtenerPreguntaPorId(Guid id)
        {
            return _preguntas.FirstOrDefault(p => p.IdPregunta == id);
        }
    }
}
