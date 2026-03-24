using ConcursoWeb.Shared;
using System.Text.Json;

namespace ConcursoWeb.Services
{
    public class PremiosService
    {
        private readonly Dictionary<int, string> _premiosPorNivel;

        public PremiosService(IWebHostEnvironment env)
        {
            _premiosPorNivel = new Dictionary<int, string>();
            CargarPremios(env);
        }

        private void CargarPremios(IWebHostEnvironment env)
        {
            var filePath = Path.Combine(env.ContentRootPath, "premios.json");
            var json = File.ReadAllText(filePath);
            var premiosConfig = JsonSerializer.Deserialize<PremiosConfig>(json);

            if (premiosConfig?.premios != null)
            {
                foreach (var premio in premiosConfig.premios)
                {
                    _premiosPorNivel[premio.nivel - 1] = premio.texto;
                }
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
