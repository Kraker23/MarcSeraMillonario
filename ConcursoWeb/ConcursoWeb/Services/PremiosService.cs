using ConcursoWeb.Shared;
using System.Text.Json;

namespace ConcursoWeb.Services
{
    public class PremiosService
    {
        private readonly Dictionary<int, string> _premiosPorNivel;
        private readonly ILogger<PremiosService> _logger;

        public PremiosService(IWebHostEnvironment env, ILogger<PremiosService> logger)
        {
            _premiosPorNivel = new Dictionary<int, string>();
            _logger = logger;
            CargarPremios(env);
        }

        private void CargarPremios(IWebHostEnvironment env)
        {
            try
            {
                var filePath = Path.Combine(env.ContentRootPath, "premios.json");
                _logger.LogInformation("Intentando cargar premios desde: {FilePath}", filePath);
                _logger.LogInformation("El archivo existe: {Exists}", File.Exists(filePath));

                var json = File.ReadAllText(filePath);
                var premiosConfig = JsonSerializer.Deserialize<PremiosConfig>(json);

                if (premiosConfig?.premios != null)
                {
                    foreach (var premio in premiosConfig.premios)
                    {
                        _premiosPorNivel[premio.nivel - 1] = premio.texto;
                    }
                }
                _logger.LogInformation("Premios cargados correctamente: {Count}", _premiosPorNivel.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar premios.json");
                throw;
            }
        }

        public string ObtenerPremio(int nivel)
        {
            return _premiosPorNivel.GetValueOrDefault(nivel, "Sin clasificar");
        }

        public Dictionary<int, string> ObtenerTodosPremios()
        {
            return _premiosPorNivel;
        }
    }
}
