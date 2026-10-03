using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Tetris2D.Audio;
using Tetris2D.Graficos;
using Tetris2D.UI;

namespace Tetris2D.Pantallas;

public class PantallaJuego : Pantalla
{
    private readonly Tablero _tablero = new();
    private readonly string _nombreJugador;
    private Pieza _piezaActual = null!, _piezaSiguiente = null!;
    private int _posX, _posY, _puntaje, _lineas, _nivel = 1;
    private float _gravedad;
    private bool _gameOver, _pausa;
    private float _tableroX, _tableroY, _tam, _panelX, _panelW;
    public PantallaJuego(GestorShader s, DibujadorCuadros c, RenderizadorTexto t, string nombre) : base(s, c, t) => _nombreJugador = nombre;
    public override void Cargar() => Reiniciar();
    private void Reiniciar() { _tablero.Reiniciar(); _puntaje = _lineas = 0; _nivel = 1; _gameOver = _pausa = false; _piezaActual = Pieza.Aleatoria(); _piezaSiguiente = Pieza.Aleatoria(); PosicionarNueva(); }
    private void PosicionarNueva()
    {
        _posX = 3; _posY = -1;
        if (_tablero.HayColision(_piezaActual, _posX, _posY)) { _gameOver = true; Sonido.Reproducir(TonoJuego.GameOver); }
    }
    public override void Actualizar(float dt, Vector2 raton)
    {
        if (_gameOver || _pausa) return;
        _gravedad += dt; float intervalo = MathF.Max(.12f, .8f - (_nivel - 1) * .06f);
        if (_gravedad >= intervalo) { _gravedad = 0; if (!Mover(0, 1)) FijarYGenerar(); }
    }
    private bool Mover(int dx, int dy)
    {
        if (_tablero.HayColision(_piezaActual, _posX + dx, _posY + dy)) return false;
        _posX += dx; _posY += dy; return true;
    }
    private void FijarYGenerar()
    {
        _tablero.Fijar(_piezaActual, _posX, _posY); Sonido.Reproducir(TonoJuego.Fijar);
        int borradas = _tablero.LimpiarLineas();
        if (borradas > 0) { _lineas += borradas; _puntaje += borradas switch { 1 => 100, 2 => 300, 3 => 500, _ => 800 }; Sonido.Reproducir(TonoJuego.Linea); int nuevoNivel = _lineas / 10 + 1; if (nuevoNivel > _nivel) { _nivel = nuevoNivel; Sonido.Reproducir(TonoJuego.Nivel); } }
        _piezaActual = _piezaSiguiente; _piezaSiguiente = Pieza.Aleatoria(); PosicionarNueva();
    }
    private void Rotar()
    {
        var anterior = _piezaActual.Bloques; _piezaActual.Rotar();
        foreach (int ajuste in new[] { 0, -1, 1, -2, 2 }) if (!_tablero.HayColision(_piezaActual, _posX + ajuste, _posY)) { _posX += ajuste; Sonido.Reproducir(TonoJuego.Rotar); return; }
        _piezaActual = new Pieza(_piezaActual.Tipo); foreach (var bloque in anterior) { } // below restores exact orientation
        while (!_piezaActual.Bloques.SequenceEqual(anterior)) _piezaActual.Rotar();
    }
    public override void AlTecla(Keys tecla)
    {
        if (tecla == Keys.Escape && !_gameOver) { _pausa = !_pausa; return; }
        if (_gameOver) { if (tecla == Keys.Enter || tecla == Keys.R) Reiniciar(); return; }
        if (_pausa) return;
        switch (tecla)
        {
            case Keys.Left: if (Mover(-1, 0)) Sonido.Reproducir(TonoJuego.Mover); break;
            case Keys.Right: if (Mover(1, 0)) Sonido.Reproducir(TonoJuego.Mover); break;
            case Keys.Down: if (Mover(0, 1)) { _puntaje++; Sonido.Reproducir(TonoJuego.Mover); } else FijarYGenerar(); break;
            case Keys.Space: Rotar(); break;
            case Keys.Up: int caidas = 0; while (Mover(0, 1)) caidas++; _puntaje += caidas * 2; FijarYGenerar(); break;
        }
    }
    public override void Renderizar(float ancho, float alto)
    {
        CalcularDisposicion(ancho, alto); _tablero.Renderizar(Cuadros, _tableroX, _tableroY, _tam);
        if (!_gameOver) foreach (var (x, y) in _piezaActual.Bloques) if (_posY + y >= 0) Tablero.DibujarBloque(Cuadros, _piezaActual.Color, _tableroX + (_posX + x) * _tam, _tableroY + (_posY + y) * _tam, _tam);
        Titulo("TETRIS - 2D", ancho * .5f, alto * .03f, alto * .045f, TemaArcade.Cian);
        RenderizarPanel(alto);
        if (_pausa) Overlay(ancho, alto, "P A U S A", "PRESIONA ESC PARA CONTINUAR", TemaArcade.Amarillo);
        if (_gameOver) Overlay(ancho, alto, "GAME OVER", $"PUNTAJE FINAL: {_puntaje}   -   ENTER O R PARA REINICIAR", TemaArcade.Rojo);
    }
    private void CalcularDisposicion(float ancho, float alto) { _tam = MathF.Floor(MathF.Min((alto * .80f) / Tablero.Filas, (ancho * .52f) / Tablero.Columnas)); _tableroX = ancho * .5f - (_tam * Tablero.Columnas + _tam * 1.5f + MathF.Max(150, ancho * .22f)) * .5f; _tableroY = alto * .14f; _panelX = _tableroX + Tablero.Columnas * _tam + _tam * 1.5f; _panelW = MathF.Max(150, ancho * .22f); }
    private void RenderizarPanel(float alto)
    {
        float h = Tablero.Filas * _tam; Cuadros.DibujarRectangulo(TemaArcade.PanelOscuro, _panelX, _tableroY, _panelX + _panelW, _tableroY + h); Cuadros.DibujarBordeRectangulo(TemaArcade.Cian, _panelX, _tableroY, _panelX + _panelW, _tableroY + h, MathF.Max(1, _tam * .06f)); float cx = _panelX + _panelW / 2, y = _tableroY + _tam * .7f;
        Dato("JUGADOR", _nombreJugador.ToUpperInvariant(), cx, ref y, TemaArcade.Amarillo); Dato("PUNTAJE", _puntaje.ToString(), cx, ref y, TemaArcade.Amarillo); Dato("LINEAS", _lineas.ToString(), cx, ref y, TemaArcade.Verde); Dato("NIVEL", _nivel.ToString(), cx, ref y, TemaArcade.Rojo);
        Titulo("SIGUIENTE", cx, y, _tam * .7f, TemaArcade.Magenta); y += _tam; RenderizarPreview(_piezaSiguiente, cx, y); y += _tam * 4.2f; Titulo("FLECHAS: MOVER", cx, y, _tam * .43f, TemaArcade.TextoSuave); Titulo("ESPACIO: ROTAR", cx, y + _tam * .65f, _tam * .43f, TemaArcade.TextoSuave); Titulo("ESC: PAUSA", cx, y + _tam * 1.3f, _tam * .43f, TemaArcade.TextoSuave);
    }
    private void Dato(string etiqueta, string valor, float cx, ref float y, Vector4 color) { Titulo(etiqueta, cx, y, _tam * .65f, TemaArcade.Magenta); Titulo(valor, cx, y + _tam * .65f, _tam * .95f, color); y += _tam * 2.15f; }
    private void RenderizarPreview(Pieza pieza, float cx, float y) { int minX = pieza.Bloques.Min(b => b.X), maxX = pieza.Bloques.Max(b => b.X), minY = pieza.Bloques.Min(b => b.Y), maxY = pieza.Bloques.Max(b => b.Y); float mini = _tam * .55f; foreach (var (x, yy) in pieza.Bloques) Tablero.DibujarBloque(Cuadros, pieza.Color, cx + (x - (minX + maxX + 1) / 2f) * mini, y + (yy - minY) * mini, mini); }
    private void Titulo(string texto, float cx, float y, float tamano, Vector4 color) { float w = Texto.MedirTexto(texto, tamano); Texto.DibujarTexto(texto, cx - w / 2, y, tamano, color); }
    private void Overlay(float ancho, float alto, string titulo, string nota, Vector4 color) { Cuadros.DibujarRectangulo(new Vector4(0, 0, 0, .68f), 0, 0, ancho, alto); float px = ancho * .15f, py = alto * .37f, pw = ancho * .70f, ph = alto * .25f; Cuadros.DibujarRectangulo(TemaArcade.PanelOscuro, px, py, px + pw, py + ph); Cuadros.DibujarBordeRectangulo(color, px, py, px + pw, py + ph, 3); Titulo(titulo, ancho / 2, py + ph * .22f, alto * .07f, color); Titulo(nota, ancho / 2, py + ph * .63f, alto * .026f, TemaArcade.TextoSuave); }
}
