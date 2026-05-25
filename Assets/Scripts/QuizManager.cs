using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class QuizManager : MonoBehaviour
{
    public static QuizManager Instancia { get; private set; }

    [Header("Referencias de UI")]
    public GameObject panelQuiz;
    public TextMeshProUGUI textoPregunta;
    public Button[] botonesRespuesta = new Button[4];
    public UnityEngine.UI.Image panelFlash;

    [Header("Feedback")]
    public AudioClip sonidoCorrecto;
    public AudioClip sonidoIncorrecto;
    public float duracionFlash = 0.5f;

    private PreguntaSO preguntaActual;
    private AudioSource miAudioSource;
    private Llave llavePendiente;
    private bool esQuizDeVida;

    private List<PreguntaSO> preguntasMaestras = new List<PreguntaSO>();
    private List<PreguntaSO> preguntasDisponibles = new List<PreguntaSO>();

    void Awake()
    {
        if (Instancia != null && Instancia != this) Destroy(this.gameObject);
        else Instancia = this;

        miAudioSource = GetComponent<AudioSource>();
        panelQuiz.SetActive(false);
        if (panelFlash != null)
        {
            panelFlash.color = new Color(1, 1, 1, 0);
            panelFlash.gameObject.SetActive(false);
        }
    }


    public void CargarPreguntasDelNivel(List<PreguntaSO> nuevasPreguntas)
    {
        preguntasMaestras = new List<PreguntaSO>(nuevasPreguntas); 
        ReponerPreguntas(); 
    }

    private void ReponerPreguntas()
    {
        preguntasDisponibles = new List<PreguntaSO>(preguntasMaestras);
    }

    private PreguntaSO ObtenerPreguntaSinRepetir()
    {
        if (preguntasDisponibles.Count == 0)
        {
            if (preguntasMaestras.Count > 0)
            {
                ReponerPreguntas();
            }
            else
            {
                return null; 
            }
        }

        int index = UnityEngine.Random.Range(0, preguntasDisponibles.Count);
        PreguntaSO preguntaSeleccionada = preguntasDisponibles[index];
        preguntasDisponibles.RemoveAt(index);

        return preguntaSeleccionada;
    }

    public void SolicitarQuizDeLlave(Llave llave)
    {
        if (preguntasMaestras.Count == 0)
        {
            Debug.LogWarning("¡No se han cargado preguntas para este nivel!");
            return;
        }

        llavePendiente = llave;
        esQuizDeVida = false;
        IniciarQuiz();
    }

    public void SolicitarQuizDeVida()
    {
        if (preguntasMaestras.Count == 0) return;

        llavePendiente = null;
        esQuizDeVida = true;
        IniciarQuiz();
    }

    private void IniciarQuiz()
    {
        Time.timeScale = 0f;
        preguntaActual = ObtenerPreguntaSinRepetir();

        if (preguntaActual != null)
        {
            MostrarPregunta(preguntaActual);
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    private void MostrarPregunta(PreguntaSO datos)
    {
        textoPregunta.text = datos.pregunta;
        for (int i = 0; i < botonesRespuesta.Length; i++)
        {
            TextMeshProUGUI textoBoton = botonesRespuesta[i].GetComponentInChildren<TextMeshProUGUI>();
            if (textoBoton != null) textoBoton.text = datos.alternativas[i];
        }
        panelQuiz.SetActive(true);
    }

    public void Responder(int indiceRespuesta)
    {
        panelQuiz.SetActive(false);
        bool esCorrecto = (indiceRespuesta == preguntaActual.indiceRespuestaCorrecta);
        StartCoroutine(RutinaFeedback(esCorrecto));
    }

    public void ForzarCierreQuiz()
    {
        StopAllCoroutines();
        panelQuiz.SetActive(false);
        if (panelFlash != null) panelFlash.gameObject.SetActive(false);
        llavePendiente = null;
        esQuizDeVida = false;
    }

    private IEnumerator RutinaFeedback(bool esCorrecto)
    {
        panelFlash.gameObject.SetActive(true);
        Color colorFlash = esCorrecto ? Color.green : Color.red;
        if (esCorrecto) miAudioSource.PlayOneShot(sonidoCorrecto);
        else miAudioSource.PlayOneShot(sonidoIncorrecto);

        float tiempoPasado = 0f;
        float duracionFade = duracionFlash / 2;
        while (tiempoPasado < duracionFade)
        {
            tiempoPasado += Time.unscaledDeltaTime;
            float alfa = Mathf.Lerp(0, 1, tiempoPasado / duracionFade);
            panelFlash.color = new Color(colorFlash.r, colorFlash.g, colorFlash.b, alfa);
            yield return null;
        }
        tiempoPasado = 0f;
        while (tiempoPasado < duracionFade)
        {
            tiempoPasado += Time.unscaledDeltaTime;
            float alfa = Mathf.Lerp(1, 0, tiempoPasado / duracionFade);
            panelFlash.color = new Color(colorFlash.r, colorFlash.g, colorFlash.b, alfa);
            yield return null;
        }
        panelFlash.gameObject.SetActive(false);

        if (esQuizDeVida)
        {
            if (esCorrecto) GameManager.Instancia.AumentarVida();
            else
            {
                string feedback = preguntaActual.feedbackSolucion;
                UIManager.Instancia.MostrarFeedback(feedback);
            }
        }
        else if (llavePendiente != null)
        {
            if (esCorrecto) llavePendiente.CompletarRecoleccion();
            else
            {
                string feedback = preguntaActual.feedbackSolucion;
                if (GameManager.Instancia.pelota != null)
                    GameManager.Instancia.pelota.GetComponent<ControladorPelota>().MorirPorQuiz(feedback);
                GameManager.Instancia.RegistrarFalloEnQuiz();
            }
        }

        llavePendiente = null;
        esQuizDeVida = false;

        if (GameManager.Instancia.VidasActuales > 0) Time.timeScale = 1f;
    }
}

