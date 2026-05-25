using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))] 
public class GameManager : MonoBehaviour
{
    public static GameManager Instancia { get; private set; }

    [Header("Configuración de Muerte")]
    public GameObject prefabExplosion;
    public AudioClip sonidoExplosion;
    public float retrasoReinicio = 1.0f;

    [Header("Configuracion de Nivel")]
    public int llavesTotales = 4;
    private int llavesActuales;
    public int vidasMaximas = 3;
    private int vidasActuales;
    public int VidasActuales { get { return vidasActuales; } }
    public float tiempoLimite = 60f; 
    private float tiempoRestante;
    private bool estaJuegoTerminado = false;
    public string nombreSiguienteNivel;

    [Header("Referencias de Escena")]
    public GameObject pelota;
    public Transform posicionInicio;
    public PuertaSalida puertaSalida;

    private Vector3 vectorRespawnActual;
    private Checkpoint checkpointActivo;
    private string feedbackPendiente = "";

    [Header("Paneles UI")]
    public GameObject panelGameOver;
    public GameObject panelNivelCompletado;
    public TextMeshProUGUI textoTiempoNivel;
    public AudioClip sonidoGameOver;
    public AudioClip sonidoNivelCompletado;

    [Header("UI Nivel Completado")] 
    public UnityEngine.UI.Image[] estrellas; 
    public Color colorEstrellaApagada = Color.black;
    private int preguntasFallidas = 0;

    private AudioSource miAudioSource;
    [Header("Preguntas de ESTE Nivel")] 
    public List<PreguntaSO> preguntasDelNivel;

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

        miAudioSource = GetComponent<AudioSource>();

        estaJuegoTerminado = false;
        Time.timeScale = 1f;
        llavesActuales = 0;
        vidasActuales = vidasMaximas;
        tiempoRestante = tiempoLimite;

        vectorRespawnActual = posicionInicio.position;

        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (panelNivelCompletado != null) panelNivelCompletado.SetActive(false);
        if (UIManager.Instancia != null)
        {
            UIManager.Instancia.ActualizarVidas(vidasActuales);
            UIManager.Instancia.IniciarLlaves(llavesTotales);
            UIManager.Instancia.ActualizarTiempo(tiempoRestante);
        }
    }

    void Start()
    {
        if (QuizManager.Instancia != null)
        {
            QuizManager.Instancia.CargarPreguntasDelNivel(preguntasDelNivel);
        }
    }

    void Update()
    {
        if (!estaJuegoTerminado)
        {
            tiempoRestante -= Time.unscaledDeltaTime;

            if (tiempoRestante <= 0)
            {
                tiempoRestante = 0;
                Debug.Log("¡Se acabó el tiempo!");

                if (pelota != null)
                {
                    pelota.SetActive(false);
                }
                MostrarGameOver();
            }
            if (UIManager.Instancia != null)
            {
                UIManager.Instancia.ActualizarTiempo(tiempoRestante);
            }
        }
    }

    public void PerderJuego(Vector3 posicion)
    {
        AudioSource.PlayClipAtPoint(sonidoExplosion, posicion);
        GameObject explosion = Instantiate(prefabExplosion, posicion, Quaternion.identity);
        Destroy(explosion, 0.5f);
        ProcesarMuerte();
    }

    public void FallarQuiz(string feedback) 
    {
        feedbackPendiente = feedback; 
        ProcesarMuerte();
    }

    private IEnumerator RutinaRespawn()
    {
        yield return new WaitForSeconds(retrasoReinicio);

        EjecutarRespawn();
    }

    private void MostrarGameOver()
    {
        if (estaJuegoTerminado) return;
        estaJuegoTerminado = true;
        QuizManager.Instancia.ForzarCierreQuiz();
        if (sonidoGameOver != null) miAudioSource.PlayOneShot(sonidoGameOver);
        if (panelGameOver != null) panelGameOver.SetActive(true);
        Time.timeScale = 0f;
    }

    public void JugarOtraVez()
    {
        Time.timeScale = 1f;
        ReiniciarNivel();
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    private void ReiniciarNivel()
    {
        string escenaActual = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(escenaActual);
    }

    public void RecogerLlave()
    {
        llavesActuales++;
        if (UIManager.Instancia != null) UIManager.Instancia.RemoverIconoLlave();

        if (llavesActuales >= llavesTotales)
        {
            if (puertaSalida != null)
            {
                puertaSalida.AbrirPuerta();
            }
        }
    }

    public void IntentarSalir()
    {
        if (puertaSalida != null && puertaSalida.estaAbierta)
        {
            CompletarNivel();
        }
        else
        {
            int faltantes = llavesTotales - llavesActuales;
            Debug.Log($"¡PUERTA CERRADA! Te faltan {faltantes} llaves.");
        }
    }

    private void CompletarNivel()
    {
        estaJuegoTerminado = true;
        Time.timeScale = 0f;
        pelota.SetActive(false);

        if (sonidoNivelCompletado != null) miAudioSource.PlayOneShot(sonidoNivelCompletado);

        if (panelNivelCompletado != null)
        {
            panelNivelCompletado.SetActive(true);
        }

        if (textoTiempoNivel != null)
        {
            float tiempoUtilizado = tiempoLimite - tiempoRestante;
            TimeSpan t = TimeSpan.FromSeconds(tiempoUtilizado);
            string tiempoFormateado = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
            textoTiempoNivel.text = $"Tiempo: {tiempoFormateado}";
        }
        CalcularYMostrarEstrellas();
    }

    public void SiguienteNivel()
    {
        Time.timeScale = 1f;
        int proximaEscena = SceneManager.GetActiveScene().buildIndex + 1;

        if (proximaEscena < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(proximaEscena);
        }
        else
        {
            Debug.Log("¡No hay más niveles! Volviendo al menú.");
            VolverAlMenu();
        }
    }

    public void ActivarNuevoCheckpoint(Checkpoint nuevoCheckpoint)
    {
        if (checkpointActivo != null)
        {
            checkpointActivo.Desactivar();
        }

        checkpointActivo = nuevoCheckpoint;
        checkpointActivo.Activar();

        vectorRespawnActual = nuevoCheckpoint.transform.position;
    }


    public void AumentarVida()
    {
        vidasActuales++;
        Debug.Log($"Vida extra ganada! Vidas restantes: {vidasActuales}");

        if (UIManager.Instancia != null)
        {
            UIManager.Instancia.ActualizarVidas(vidasActuales);
        }
    }

    public void RegistrarFalloEnQuiz()
    {
        preguntasFallidas++;
    }

    private void ProcesarMuerte()
    {
        vidasActuales--;
        Debug.Log($"Vida perdida! Vidas restantes: {vidasActuales}");

        if (UIManager.Instancia != null)
        {
            UIManager.Instancia.ActualizarVidas(vidasActuales);
        }

        if (vidasActuales > 0)
        {
            StartCoroutine(RutinaRespawn());
        }
        else
        {
            MostrarGameOver();
        }
    }

    private void EjecutarRespawn()
    {
        pelota.transform.position = vectorRespawnActual;
        pelota.SetActive(true);

        Rigidbody2D rb = pelota.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        if (!string.IsNullOrEmpty(feedbackPendiente))
        {
            UIManager.Instancia.MostrarFeedback(feedbackPendiente);
            feedbackPendiente = "";
        }
    }

    private void CalcularYMostrarEstrellas()
    {
        int rating = 0;
        if (preguntasFallidas == 0) rating = 3;
        else if (preguntasFallidas == 1) rating = 2;
        else rating = 1;

        string nombreEscenaActual = SceneManager.GetActiveScene().name;

        if (DataManager.Instancia != null)
        {
            // 1. Guardar Estrellas del nivel actual
            DataManager.Instancia.GuardarEstrellas(nombreEscenaActual, rating);

            // --- ¡NUEVA LÓGICA! ---
            // 2. Desbloquear el SIGUIENTE nivel
            if (!string.IsNullOrEmpty(nombreSiguienteNivel))
            {
                DataManager.Instancia.DesbloquearNivel(nombreSiguienteNivel);
            }
            // ----------------------
        }

        for (int i = 0; i < estrellas.Length; i++)
        {
            if (i < rating) estrellas[i].color = Color.white;
            else estrellas[i].color = colorEstrellaApagada;
        }
    }

}