using OpenTK.Mathematics;
using Tetris2D.UI;

namespace Tetris2D;

public enum TipoPieza { I, O, T, S, Z, J, L }

public class Pieza
{
    private static readonly Random Aleatorio = new();
    private static readonly Dictionary<TipoPieza, (int X, int Y)[]> Formas = new()
    {
        [TipoPieza.I] = [(0, 1), (1, 1), (2, 1), (3, 1)],
        [TipoPieza.O] = [(1, 0), (2, 0), (1, 1), (2, 1)],
        [TipoPieza.T] = [(1, 0), (0, 1), (1, 1), (2, 1)],
        [TipoPieza.S] = [(1, 0), (2, 0), (0, 1), (1, 1)],
        [TipoPieza.Z] = [(0, 0), (1, 0), (1, 1), (2, 1)],
        [TipoPieza.J] = [(0, 0), (0, 1), (1, 1), (2, 1)],
        [TipoPieza.L] = [(2, 0), (0, 1), (1, 1), (2, 1)]
    };
    private static readonly Dictionary<TipoPieza, Vector4> Colores = new()
    {
        [TipoPieza.I] = TemaArcade.Cian, [TipoPieza.O] = TemaArcade.Amarillo,
        [TipoPieza.T] = TemaArcade.Magenta, [TipoPieza.S] = TemaArcade.Verde,
        [TipoPieza.Z] = TemaArcade.Rojo, [TipoPieza.J] = TemaArcade.Azul,
        [TipoPieza.L] = TemaArcade.Naranja
    };

    public TipoPieza Tipo { get; }
    public List<(int X, int Y)> Bloques { get; private set; }
    public Vector4 Color => Colores[Tipo];
    public Pieza(TipoPieza tipo) { Tipo = tipo; Bloques = Formas[tipo].ToList(); }
    public List<(int X, int Y)> BloquesRotados() => Bloques.Select(b => (3 - b.Y, b.X)).ToList();
    public void Rotar() { if (Tipo != TipoPieza.O) Bloques = BloquesRotados(); }
    public static Pieza Aleatoria() => new((TipoPieza)Aleatorio.Next(0, 7));
}
