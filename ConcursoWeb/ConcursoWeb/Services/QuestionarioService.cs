using ConcursoWeb.Shared;
using System.Text.Json;

namespace ConcursoWeb.Services
{
    public class QuestionarioService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<QuestionarioService> _logger;
        private List<Pregunta> _preguntas;

        public QuestionarioService(IWebHostEnvironment env, ILogger<QuestionarioService> logger)
        {
            _env = env;
            _logger = logger;
            CargarPreguntas();
        }

        private void CargarPreguntas()
        {
            try
            {
                var filePath = Path.Combine(_env.ContentRootPath, "preguntas.json");
                _logger.LogInformation("Intentando cargar preguntas desde: {FilePath}", filePath);
                _logger.LogInformation("ContentRootPath: {ContentRoot}", _env.ContentRootPath);
                _logger.LogInformation("WebRootPath: {WebRoot}", _env.WebRootPath);
                _logger.LogInformation("El archivo existe: {Exists}", File.Exists(filePath));

                var json = File.ReadAllText(filePath);
                var questionario = JsonSerializer.Deserialize<Questionario>(json);
                _preguntas = questionario?.preguntas ?? new List<Pregunta>();
                _logger.LogInformation("Preguntas cargadas correctamente: {Count}", _preguntas.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar preguntas.json");
                _preguntas = new List<Pregunta>();
                throw;
            }
            
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
