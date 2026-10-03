using OpenTK.Mathematics;
using Tetris2D.Graficos;
using Tetris2D.UI;

namespace Tetris2D;

public class Tablero
{
    public const int Columnas = 10;
    public const int Filas = 18;
    private readonly int[,] _celdas = new int[Columnas, Filas];
    public bool HayColision(Pieza pieza, int px, int py)
    {
        foreach (var (x, y) in pieza.Bloques)
        {
            int tx = px + x, ty = py + y;
            if (tx < 0 || tx >= Columnas || ty >= Filas || (ty >= 0 && _celdas[tx, ty] != 0)) return true;
        }
        return false;
    }
    public void Fijar(Pieza pieza, int px, int py)
    {
        foreach (var (x, y) in pieza.Bloques)
            if (py + y >= 0 && py + y < Filas) _celdas[px + x, py + y] = (int)pieza.Tipo + 1;
    }
    public int LimpiarLineas()
    {
        int eliminadas = 0;
        for (int y = Filas - 1; y >= 0; y--)
        {
            bool llena = Enumerable.Range(0, Columnas).All(x => _celdas[x, y] != 0);
            if (!llena) continue;
            eliminadas++;
            for (int yy = y; yy > 0; yy--) for (int x = 0; x < Columnas; x++) _celdas[x, yy] = _celdas[x, yy - 1];
            for (int x = 0; x < Columnas; x++) _celdas[x, 0] = 0;
            y++;
        }
        return eliminadas;
    }
    public void Reiniciar() => Array.Clear(_celdas);
    public void Renderizar(DibujadorCuadros cuadros, float x0, float y0, float tam)
    {
        float ancho = Columnas * tam, alto = Filas * tam;
        cuadros.DibujarRectangulo(TemaArcade.PanelOscuro, x0, y0, x0 + ancho, y0 + alto);
        cuadros.DibujarBordeRectangulo(TemaArcade.Cian, x0, y0, x0 + ancho, y0 + alto, MathF.Max(2, tam * .10f));
        Vector4 cuadricula = new(TemaArcade.TextoSuave.X, TemaArcade.TextoSuave.Y, TemaArcade.TextoSuave.Z, .12f);
        for (int x = 1; x < Columnas; x++) cuadros.DibujarLinea(cuadricula, x0 + x * tam, y0, x0 + x * tam, y0 + alto, 1);
        for (int y = 1; y < Filas; y++) cuadros.DibujarLinea(cuadricula, x0, y0 + y * tam, x0 + ancho, y0 + y * tam, 1);
        for (int x = 0; x < Columnas; x++) for (int y = 0; y < Filas; y++) if (_celdas[x, y] != 0) DibujarBloque(cuadros, new Pieza((TipoPieza)(_celdas[x, y] - 1)).Color, x0 + x * tam, y0 + y * tam, tam);
    }
    public static void DibujarBloque(DibujadorCuadros cuadros, Vector4 color, float x, float y, float tam)
    {
        cuadros.DibujarRectangulo(color, x, y, x + tam, y + tam);
        cuadros.DibujarBordeRectangulo(new Vector4(0, 0, 0, .35f), x, y, x + tam, y + tam, MathF.Max(1, tam * .06f));
    }
}
