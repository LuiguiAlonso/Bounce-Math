using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instancia { get; private set; }

    [Header("Componentes de Vidas")]
    public TextMeshProUGUI textoVidas;

    [Header("Componentes de Llaves")]
    public List<GameObject> iconosLlave;

    [Header("Componentes de Tiempo")] 
    public TextMeshProUGUI textoTiempo;

    [Header("Componentes de Feedback")] 
    public TextMeshProUGUI textoFeedback;
    public float duracionFeedback = 3.0f;
    public float fadeFeedback = 0.5f;

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instancia = this;
        }
    }

    public void IniciarLlaves(int cantidad)
    {
        for (int i = 0; i < iconosLlave.Count; i++)
        {
            iconosLlave[i].SetActive(i < cantidad);
        }
    }

    public void ActualizarVidas(int vidas)
    {
        if (textoVidas != null)
        {
            textoVidas.text = $"X{vidas}";
        }
    }

    public void RemoverIconoLlave()
    {
        for (int i = iconosLlave.Count - 1; i >= 0; i--)
        {
            if (iconosLlave[i].activeSelf)
            {
                iconosLlave[i].SetActive(false);
                break; 
            }
        }
    }

    public void ActualizarTiempo(float tiempoEnSegundos)
    {
        if (textoTiempo != null)
        {
            if (tiempoEnSegundos < 0)
            {
                tiempoEnSegundos = 0;
            }

            float segundos = Mathf.Ceil(tiempoEnSegundos);

            textoTiempo.text = segundos.ToString("00");
        }
    }

    public void MostrarFeedback(string mensaje)
    {
        if (textoFeedback != null)
        {
            StartCoroutine(RutinaMostrarFeedback(mensaje));
        }
    }

    private IEnumerator RutinaMostrarFeedback(string mensaje)
    {
        textoFeedback.text = mensaje;
        textoFeedback.gameObject.SetActive(true);

        float tiempoPasado = 0;

        while (tiempoPasado < fadeFeedback)
        {
            tiempoPasado += Time.deltaTime;
            float alfa = Mathf.Lerp(0, 1, tiempoPasado / fadeFeedback);
            textoFeedback.color = new Color(textoFeedback.color.r, textoFeedback.color.g, textoFeedback.color.b, alfa);
            yield return null;
        }

        yield return new WaitForSeconds(duracionFeedback);

        tiempoPasado = 0;
        while (tiempoPasado < fadeFeedback)
        {
            tiempoPasado += Time.deltaTime;
            float alfa = Mathf.Lerp(1, 0, tiempoPasado / fadeFeedback);
            textoFeedback.color = new Color(textoFeedback.color.r, textoFeedback.color.g, textoFeedback.color.b, alfa);
            yield return null;
        }

        textoFeedback.gameObject.SetActive(false);
    }
}