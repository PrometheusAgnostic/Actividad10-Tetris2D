using System.Media;

namespace Tetris2D.Audio;

public enum TonoJuego { Mover, Rotar, Fijar, Linea, Nivel, GameOver }

/// <summary>Genera tonos WAV PCM en memoria; no requiere archivos de audio externos.</summary>
public static class Sonido
{
    public static void Reproducir(TonoJuego tono)
    {
        try
        {
            (double inicio, double fin, double duracion) = tono switch
            {
                TonoJuego.Mover => (420, 420, .045), TonoJuego.Rotar => (520, 700, .09),
                TonoJuego.Fijar => (170, 110, .08), TonoJuego.Linea => (500, 980, .16),
                TonoJuego.Nivel => (440, 1100, .25), _ => (280, 90, .35)
            };
            // SoundPlayer conserva el stream mientras reproduce de forma asincrona.
            // No se libera aqui para evitar cortar el tono antes de tiempo.
            MemoryStream datos = CrearWav(inicio, fin, duracion);
            new SoundPlayer(datos).Play();
        }
        catch { /* El juego sigue funcionando si el dispositivo de audio no esta disponible. */ }
    }
    private static MemoryStream CrearWav(double inicio, double fin, double duracion)
    {
        const int hz = 22050; int muestras = (int)(hz * duracion); var ms = new MemoryStream(); using var w = new BinaryWriter(ms, System.Text.Encoding.ASCII, true);
        w.Write("RIFF"u8.ToArray()); w.Write(36 + muestras * 2); w.Write("WAVEfmt "u8.ToArray()); w.Write(16); w.Write((short)1); w.Write((short)1); w.Write(hz); w.Write(hz * 2); w.Write((short)2); w.Write((short)16); w.Write("data"u8.ToArray()); w.Write(muestras * 2);
        for (int i = 0; i < muestras; i++) { double t = i / (double)hz, f = inicio + (fin - inicio) * i / muestras; w.Write((short)(Math.Sin(2 * Math.PI * f * t) * 8500 * (1 - i / (double)muestras))); }
        ms.Position = 0; return ms;
    }
}
