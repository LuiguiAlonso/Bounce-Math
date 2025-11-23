using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static System.Net.Mime.MediaTypeNames;

public class LevelButton : MonoBehaviour
{
    [Header("Configuración del Nivel")]
    public string nombreEscena;
    public bool desbloqueadoPorDefecto = false;

    [Header("Referencias de UI")]
    public UnityEngine.UI.Image[] estrellas;
    public Color colorEstrellaGanada = Color.white;
    public Color colorEstrellaPerdida = Color.black;

    private Button miBoton;

    void Awake()
    {
        miBoton = GetComponent<Button>();
    }

    void Start()
    {
        miBoton = GetComponent<Button>();
        ActualizarVisual();

        miBoton.onClick.AddListener(CargarNivel);
    }



    public void ActualizarVisual()
    {
        // --- ¡LÓGICA MODIFICADA! ---
        // El nivel está disponible si:
        // 1. Está marcado como "por defecto" (Nivel 1)
        // 2. O el DataManager dice que ya lo desbloquearon.
        bool estaDisponible = desbloqueadoPorDefecto;

        if (DataManager.Instancia != null && DataManager.Instancia.EsNivelDesbloqueado(nombreEscena))
        {
            estaDisponible = true;
        }

        miBoton.interactable = estaDisponible;

        // Cambiamos el color del botón o su opacidad si está bloqueado (Opcional visualmente)
        // miBoton.image.color = estaDisponible ? Color.white : Color.gray;

        // ----------------------------

        if (estaDisponible)
        {
            int estrellasGuardadas = DataManager.Instancia.GetEstrellas(nombreEscena);

            for (int i = 0; i < estrellas.Length; i++)
            {
                if (i < estrellasGuardadas) estrellas[i].color = colorEstrellaGanada;
                else estrellas[i].color = colorEstrellaPerdida;
            }
        }
        else
        {
            // Si está bloqueado, pinta todas las estrellas de negro
            foreach (var estrella in estrellas) estrella.color = colorEstrellaPerdida;
        }
    }

    void CargarNivel()
    {
        if (!string.IsNullOrEmpty(nombreEscena))
        {
            SceneManager.LoadScene(nombreEscena);
        }
    }

    void OnEnable()
    {
        if (DataManager.Instancia != null)
        {
            ActualizarVisual();
        }
    }
}