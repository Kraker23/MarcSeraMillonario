using ConcursoWeb.Services;
using ConcursoWeb.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ConcursoWeb.Pages
{
    public class JuegoModel : PageModel
    {
        private readonly QuestionarioService _questionarioService;

        public JuegoModel(QuestionarioService questionarioService)
        {
            _questionarioService = questionarioService;
        }

        public List<Pregunta> Preguntas { get; set; } = new List<Pregunta>();
        public int PreguntaActual { get; set; }
        public int PuntosAcumulados { get; set; }
        public bool JuegoTerminado { get; set; }
        public bool RespuestaCorrecta { get; set; }
        public string Mensaje { get; set; } = string.Empty;

        private readonly Dictionary<int, int> PremiosPorNivel = new()
        {
            { 0, 100 },
            { 1, 500 },
            { 2, 1000 },
            { 3, 2000 },
            { 4, 5000 },
            { 5, 10000 },
            { 6, 20000 },
            { 7, 50000 },
            { 8, 100000 },
            { 9, 1000000 }
        };

        public void OnGet()
        {
            IniciarJuego();
        }

        public IActionResult OnPost(Guid respuestaId)
        {
            CargarEstadoJuego();

            if (JuegoTerminado)
            {
                return Page();
            }

            var preguntaActual = Preguntas[PreguntaActual];
            var respuestaSeleccionada = preguntaActual.respuestas.FirstOrDefault(r => r.IdRespuesta == respuestaId);

            if (respuestaSeleccionada != null && respuestaSeleccionada.esCorrecta)
            {
                RespuestaCorrecta = true;
                PuntosAcumulados = PremiosPorNivel.GetValueOrDefault(PreguntaActual, 0);
                PreguntaActual++;

                if (PreguntaActual >= Preguntas.Count)
                {
                    JuegoTerminado = true;
                    Mensaje = $"¡FELICIDADES! ¡Has ganado {PuntosAcumulados:N0}€!";
                }
                else
                {
                    Mensaje = $"¡Correcto! Has ganado {PuntosAcumulados:N0}€";
                }
            }
            else
            {
                RespuestaCorrecta = false;
                JuegoTerminado = true;
                Mensaje = $"Respuesta incorrecta. Has ganado {PuntosAcumulados:N0}€";
            }

            GuardarEstadoJuego();
            return Page();
        }

        public IActionResult OnPostRetirarse()
        {
            CargarEstadoJuego();
            JuegoTerminado = true;
            Mensaje = $"Te has retirado con {PuntosAcumulados:N0}€. ¡Bien jugado!";
            GuardarEstadoJuego();
            return Page();
        }

        public IActionResult OnPostNuevoJuego()
        {
            HttpContext.Session.Clear();
            return RedirectToPage();
        }

        private void IniciarJuego()
        {
            Preguntas = _questionarioService.ObtenerPreguntasAleatorias(10);
            PreguntaActual = 0;
            PuntosAcumulados = 0;
            JuegoTerminado = false;
            RespuestaCorrecta = false;
            GuardarEstadoJuego();
        }

        private void GuardarEstadoJuego()
        {
            HttpContext.Session.SetString("Preguntas", System.Text.Json.JsonSerializer.Serialize(Preguntas));
            HttpContext.Session.SetInt32("PreguntaActual", PreguntaActual);
            HttpContext.Session.SetInt32("PuntosAcumulados", PuntosAcumulados);
            HttpContext.Session.SetString("JuegoTerminado", JuegoTerminado.ToString());
        }

        private void CargarEstadoJuego()
        {
            var preguntasJson = HttpContext.Session.GetString("Preguntas");
            if (!string.IsNullOrEmpty(preguntasJson))
            {
                Preguntas = System.Text.Json.JsonSerializer.Deserialize<List<Pregunta>>(preguntasJson) ?? new List<Pregunta>();
            }

            PreguntaActual = HttpContext.Session.GetInt32("PreguntaActual") ?? 0;
            PuntosAcumulados = HttpContext.Session.GetInt32("PuntosAcumulados") ?? 0;
            var juegoTerminadoStr = HttpContext.Session.GetString("JuegoTerminado");
            JuegoTerminado = !string.IsNullOrEmpty(juegoTerminadoStr) && bool.Parse(juegoTerminadoStr);
        }

        public int ObtenerPremio(int nivel)
        {
            return PremiosPorNivel.GetValueOrDefault(nivel, 0);
        }
    }
}
