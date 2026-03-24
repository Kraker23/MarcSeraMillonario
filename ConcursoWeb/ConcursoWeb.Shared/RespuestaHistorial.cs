namespace ConcursoWeb.Shared
{
    public class RespuestaHistorial
    {
        public int NumeroPregunta { get; set; }
        public string TextoPregunta { get; set; } = string.Empty;
        public string RespuestaDada { get; set; } = string.Empty;
        public string RespuestaCorrecta { get; set; } = string.Empty;
        public bool FueCorrecta { get; set; }
        public string? ComodinUsado { get; set; }
        public int ChupitosGenerados { get; set; }
    }

    public class ResumenJuego
    {
        public List<RespuestaHistorial> Historial { get; set; } = new List<RespuestaHistorial>();
        public string NivelAlcanzado { get; set; } = string.Empty;
        public int TotalChupitos { get; set; }
        public int PreguntasCorrectas { get; set; }
        public int PreguntasIncorrectas { get; set; }
        public int ComodinesUsados { get; set; }
    }
}
