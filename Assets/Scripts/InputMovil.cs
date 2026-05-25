using UnityEngine;
using UnityEngine.EventSystems;

public class InputMovil : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    public enum TipoBoton { Izquierda, Derecha, Salto }
    public TipoBoton tipo;

    public static bool MoviendoIzquierda;
    public static bool MoviendoDerecha;
    public static bool Saltando;

    public void OnPointerDown(PointerEventData eventData)
    {
        switch (tipo)
        {
            case TipoBoton.Izquierda: MoviendoIzquierda = true; break;
            case TipoBoton.Derecha: MoviendoDerecha = true; break;
            case TipoBoton.Salto: Saltando = true; break;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        switch (tipo)
        {
            case TipoBoton.Izquierda: MoviendoIzquierda = false; break;
            case TipoBoton.Derecha: MoviendoDerecha = false; break;
            case TipoBoton.Salto: Saltando = false; break;
        }
    }

    void OnDisable()
    {
        MoviendoIzquierda = false;
        MoviendoDerecha = false;
        Saltando = false;
    }
}