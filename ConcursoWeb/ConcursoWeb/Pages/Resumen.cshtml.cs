using ConcursoWeb.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ConcursoWeb.Pages
{
    public class ResumenModel : PageModel
    {
        public ResumenJuego Resumen { get; set; } = new ResumenJuego();

        public void OnGet()
        {
            CargarResumen();
        }

        private void CargarResumen()
        {
            var historialJson = HttpContext.Session.GetString("HistorialRespuestas");
            if (!string.IsNullOrEmpty(historialJson))
            {
                Resumen.Historial = System.Text.Json.JsonSerializer.Deserialize<List<RespuestaHistorial>>(historialJson) ?? new List<RespuestaHistorial>();
            }

            Resumen.NivelAlcanzado = HttpContext.Session.GetString("NivelAlcanzado") ?? "Sin nivel";
            Resumen.TotalChupitos = Resumen.Historial.Sum(h => h.ChupitosGenerados);
            Resumen.PreguntasCorrectas = Resumen.Historial.Count(h => h.FueCorrecta);
            Resumen.PreguntasIncorrectas = Resumen.Historial.Count(h => !h.FueCorrecta);
            Resumen.ComodinesUsados = Resumen.Historial.Count(h => h.ComodinUsado != null);
        }

        public IActionResult OnPostNuevoJuego()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Juego");
        }
    }
}
