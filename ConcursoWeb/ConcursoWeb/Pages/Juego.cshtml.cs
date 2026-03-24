using ConcursoWeb.Services;
using ConcursoWeb.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ConcursoWeb.Pages
{
    public class JuegoModel : PageModel
    {
        private readonly QuestionarioService _questionarioService;
        private readonly PremiosService _premiosService;

        public JuegoModel(QuestionarioService questionarioService, PremiosService premiosService)
        {
            _questionarioService = questionarioService;
            _premiosService = premiosService;
        }

        public List<Pregunta> Preguntas { get; set; } = new List<Pregunta>();
        public int PreguntaActual { get; set; }
        public string NivelAlcanzado { get; set; } = string.Empty;
        public bool JuegoTerminado { get; set; }
        public bool RespuestaCorrecta { get; set; }
        public string Mensaje { get; set; } = string.Empty;

        public bool Comodin5050Usado { get; set; }
        public bool ComodinCambiarPreguntaUsado { get; set; }
        public bool ComodinAmigosUsado { get; set; }
        public List<Guid> RespuestasEliminadas { get; set; } = new List<Guid>();
        public string MensajeAmigos { get; set; } = string.Empty;
        public List<int> NivelesFallados { get; set; } = new List<int>();
        public Dictionary<int, string> ComodinesUsadosPorNivel { get; set; } = new Dictionary<int, string>();
        public List<RespuestaHistorial> HistorialRespuestas { get; set; } = new List<RespuestaHistorial>();

        public void OnGet()
        {
            IniciarJuego();
        }

        public IActionResult OnPostRespuesta(Guid respuestaId)
        {
            CargarEstadoJuego();

            if (JuegoTerminado)
            {
                return Page();
            }

            var preguntaActual = Preguntas[PreguntaActual];
            var respuestaSeleccionada = preguntaActual.respuestas.FirstOrDefault(r => r.IdRespuesta == respuestaId);
            var respuestaCorrecta = preguntaActual.respuestas.FirstOrDefault(r => r.esCorrecta);

            var historial = new RespuestaHistorial
            {
                NumeroPregunta = PreguntaActual + 1,
                TextoPregunta = preguntaActual.pregunta,
                RespuestaDada = respuestaSeleccionada?.textoRespuesta ?? "No se seleccionó respuesta",
                RespuestaCorrecta = respuestaCorrecta?.textoRespuesta ?? "",
                ComodinUsado = ComodinesUsadosPorNivel.ContainsKey(PreguntaActual) ? ComodinesUsadosPorNivel[PreguntaActual] : null
            };

            if (respuestaSeleccionada != null && respuestaSeleccionada.esCorrecta)
            {
                RespuestaCorrecta = true;
                NivelAlcanzado = _premiosService.ObtenerPremio(PreguntaActual);
                Mensaje = $"✅ ¡Correcto!";
                historial.FueCorrecta = true;
                historial.ChupitosGenerados = 2; // 1 tú + 1 el otro
            }
            else
            {
                RespuestaCorrecta = false;
                NivelesFallados.Add(PreguntaActual);
                Mensaje = $"❌ ¡Fallaste! No alcanzaste el nivel: {_premiosService.ObtenerPremio(PreguntaActual)}";
                historial.FueCorrecta = false;

                // Si se usó comodín y falló: 3 chupitos, si no: 2 chupitos
                historial.ChupitosGenerados = historial.ComodinUsado != null ? 3 : 2;
            }

            HistorialRespuestas.Add(historial);
            PreguntaActual++;

            if (PreguntaActual >= Preguntas.Count)
            {
                JuegoTerminado = true;
                if (!string.IsNullOrEmpty(NivelAlcanzado))
                {
                    Mensaje = $"¡JUEGO TERMINADO! Nivel final alcanzado: {NivelAlcanzado}";
                }
                else
                {
                    Mensaje = "¡JUEGO TERMINADO! No alcanzaste ningún nivel.";
                }
            }

            GuardarEstadoJuego();
            return Page();
        }

        public IActionResult OnPostRetirarse()
        {
            CargarEstadoJuego();
            JuegoTerminado = true;
            Mensaje = $"Te has retirado con el nivel: {NivelAlcanzado}. ¡Bien jugado!";
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
            NivelAlcanzado = string.Empty;
            JuegoTerminado = false;
            RespuestaCorrecta = false;
            Comodin5050Usado = false;
            ComodinCambiarPreguntaUsado = false;
            ComodinAmigosUsado = false;
            RespuestasEliminadas = new List<Guid>();
            MensajeAmigos = string.Empty;
            NivelesFallados = new List<int>();
            ComodinesUsadosPorNivel = new Dictionary<int, string>();
            HistorialRespuestas = new List<RespuestaHistorial>();
            GuardarEstadoJuego();
        }

        private void GuardarEstadoJuego()
        {
            HttpContext.Session.SetString("Preguntas", System.Text.Json.JsonSerializer.Serialize(Preguntas));
            HttpContext.Session.SetInt32("PreguntaActual", PreguntaActual);
            HttpContext.Session.SetString("NivelAlcanzado", NivelAlcanzado);
            HttpContext.Session.SetString("JuegoTerminado", JuegoTerminado.ToString());
            HttpContext.Session.SetString("Comodin5050Usado", Comodin5050Usado.ToString());
            HttpContext.Session.SetString("ComodinCambiarPreguntaUsado", ComodinCambiarPreguntaUsado.ToString());
            HttpContext.Session.SetString("ComodinAmigosUsado", ComodinAmigosUsado.ToString());
            HttpContext.Session.SetString("RespuestasEliminadas", System.Text.Json.JsonSerializer.Serialize(RespuestasEliminadas));
            HttpContext.Session.SetString("MensajeAmigos", MensajeAmigos);
            HttpContext.Session.SetString("NivelesFallados", System.Text.Json.JsonSerializer.Serialize(NivelesFallados));
            HttpContext.Session.SetString("ComodinesUsadosPorNivel", System.Text.Json.JsonSerializer.Serialize(ComodinesUsadosPorNivel));
            HttpContext.Session.SetString("HistorialRespuestas", System.Text.Json.JsonSerializer.Serialize(HistorialRespuestas));
        }

        private void CargarEstadoJuego()
        {
            var preguntasJson = HttpContext.Session.GetString("Preguntas");
            if (!string.IsNullOrEmpty(preguntasJson))
            {
                Preguntas = System.Text.Json.JsonSerializer.Deserialize<List<Pregunta>>(preguntasJson) ?? new List<Pregunta>();
            }

            PreguntaActual = HttpContext.Session.GetInt32("PreguntaActual") ?? 0;
            NivelAlcanzado = HttpContext.Session.GetString("NivelAlcanzado") ?? string.Empty;
            var juegoTerminadoStr = HttpContext.Session.GetString("JuegoTerminado");
            JuegoTerminado = !string.IsNullOrEmpty(juegoTerminadoStr) && bool.Parse(juegoTerminadoStr);

            var comodin5050Str = HttpContext.Session.GetString("Comodin5050Usado");
            Comodin5050Usado = !string.IsNullOrEmpty(comodin5050Str) && bool.Parse(comodin5050Str);

            var comodinCambiarStr = HttpContext.Session.GetString("ComodinCambiarPreguntaUsado");
            ComodinCambiarPreguntaUsado = !string.IsNullOrEmpty(comodinCambiarStr) && bool.Parse(comodinCambiarStr);

            var comodinAmigosStr = HttpContext.Session.GetString("ComodinAmigosUsado");
            ComodinAmigosUsado = !string.IsNullOrEmpty(comodinAmigosStr) && bool.Parse(comodinAmigosStr);

            var respuestasEliminadasJson = HttpContext.Session.GetString("RespuestasEliminadas");
            if (!string.IsNullOrEmpty(respuestasEliminadasJson))
            {
                RespuestasEliminadas = System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(respuestasEliminadasJson) ?? new List<Guid>();
            }

            var nivelesFalladosJson = HttpContext.Session.GetString("NivelesFallados");
            if (!string.IsNullOrEmpty(nivelesFalladosJson))
            {
                NivelesFallados = System.Text.Json.JsonSerializer.Deserialize<List<int>>(nivelesFalladosJson) ?? new List<int>();
            }

            var comodinesUsadosJson = HttpContext.Session.GetString("ComodinesUsadosPorNivel");
            if (!string.IsNullOrEmpty(comodinesUsadosJson))
            {
                ComodinesUsadosPorNivel = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, string>>(comodinesUsadosJson) ?? new Dictionary<int, string>();
            }

            var historialRespuestasJson = HttpContext.Session.GetString("HistorialRespuestas");
            if (!string.IsNullOrEmpty(historialRespuestasJson))
            {
                HistorialRespuestas = System.Text.Json.JsonSerializer.Deserialize<List<RespuestaHistorial>>(historialRespuestasJson) ?? new List<RespuestaHistorial>();
            }

            MensajeAmigos = HttpContext.Session.GetString("MensajeAmigos") ?? string.Empty;
        }

        public string ObtenerPremio(int nivel)
        {
            return _premiosService.ObtenerPremio(nivel);
        }

        public IActionResult OnPostUsar5050()
        {
            CargarEstadoJuego();

            if (!Comodin5050Usado && !JuegoTerminado)
            {
                Comodin5050Usado = true;
                ComodinesUsadosPorNivel[PreguntaActual] = "🎯";
                var preguntaActual = Preguntas[PreguntaActual];
                var respuestasIncorrectas = preguntaActual.respuestas
                    .Where(r => !r.esCorrecta)
                    .OrderBy(_ => Guid.NewGuid())
                    .Take(2)
                    .ToList();

                RespuestasEliminadas.AddRange(respuestasIncorrectas.Select(r => r.IdRespuesta));
            }

            GuardarEstadoJuego();
            return Page();
        }

        public IActionResult OnPostCambiarPregunta()
        {
            CargarEstadoJuego();

            if (!ComodinCambiarPreguntaUsado && !JuegoTerminado)
            {
                ComodinCambiarPreguntaUsado = true;
                ComodinesUsadosPorNivel[PreguntaActual] = "🔄";
                //var nuevasPreguntasRestantes = _questionarioService.ObtenerPreguntasAleatorias(Preguntas.Count - PreguntaActual);

                //for (int i = 0; i < nuevasPreguntasRestantes.Count && PreguntaActual + i < Preguntas.Count; i++)
                //{
                //    Preguntas[PreguntaActual + i] = nuevasPreguntasRestantes[i];
                //}

                RespuestasEliminadas.Clear();

                var preguntaActual = Preguntas[PreguntaActual];
                var respuestasIncorrectas = preguntaActual.respuestas
                    .Where(r => !r.esCorrecta)
                    .OrderBy(_ => Guid.NewGuid())
                    .Take(2)
                    .ToList();

                RespuestasEliminadas.AddRange(respuestasIncorrectas.Select(r => r.IdRespuesta));
                MensajeAmigos = string.Empty;

                MensajeAmigos = $"Tus supuestos amigos te van hacer otra pregunta, preparate para sufrir";
            }

            GuardarEstadoJuego();
            return Page();
        }

        public IActionResult OnPostComodinAmigos()
        {
            CargarEstadoJuego();

            if (!ComodinAmigosUsado && !JuegoTerminado)
            {
                ComodinAmigosUsado = true;
                ComodinesUsadosPorNivel[PreguntaActual] = "👥";
                var preguntaActual = Preguntas[PreguntaActual];
                var respuestaCorrecta = preguntaActual.respuestas.FirstOrDefault(r => r.esCorrecta);

                if (respuestaCorrecta != null)
                {
                    var random = new Random();
                    var porcentajeAmigos = random.Next(60, 91);
                    var indiceRespuesta = preguntaActual.respuestas.IndexOf(respuestaCorrecta);
                    var letra = ((char)('A' + indiceRespuesta)).ToString();

                    MensajeAmigos = $"El programa cree que es la {letra}.{Environment.NewLine} Tus amigos pueden darte tambien lo que creen o puede ser te trolean";
                }
            }

            GuardarEstadoJuego();
            return Page();
        }
    }
}
