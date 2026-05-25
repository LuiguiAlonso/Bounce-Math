using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instancia { get; private set; }

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instancia = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    // --- API de Estrellas ---

    public void GuardarEstrellas(string nombreNivel, int estrellas)
    {
        string clave = $"Estrellas_{nombreNivel}";

        if (estrellas > GetEstrellas(nombreNivel))
        {
            PlayerPrefs.SetInt(clave, estrellas);
            PlayerPrefs.Save();
            UnityEngine.Debug.Log($"Datos guardados LOCAL: {clave} = {estrellas}");

            // INTENTO DE GUARDADO EN NUBE
            if (FirebaseManager.Instancia != null && FirebaseManager.Instancia.estaConectado)
            {
                // 1. Calculamos el TOTAL
                int total = CalcularEstrellasTotales();
                // 2. Enviamos Nivel y Total
                FirebaseManager.Instancia.GuardarEstrellasEnNube(nombreNivel, estrellas, total);
            }
        }
    }

    public int CalcularEstrellasTotales()
    {
        int total = 0;
        // Suma del Nivel 1 al 6 (ajusta si tienes más)
        for (int i = 1; i <= 6; i++)
        {
            total += GetEstrellas($"Nivel_{i}");
        }
        return total;
    }

    public void GuardarEstrellasDesdeNube(string nombreNivel, int estrellas)
    {
        string clave = $"Estrellas_{nombreNivel}";
        if (estrellas > GetEstrellas(nombreNivel))
        {
            PlayerPrefs.SetInt(clave, estrellas);
            PlayerPrefs.Save();
        }
    }

    public int GetEstrellas(string nombreNivel)
    {
        string clave = $"Estrellas_{nombreNivel}";
        return PlayerPrefs.GetInt(clave, 0);
    }

    // --- API de Desbloqueo ---

    public void DesbloquearNivel(string nombreNivel)
    {
        string clave = $"Desbloqueado_{nombreNivel}";
        PlayerPrefs.SetInt(clave, 1);
        PlayerPrefs.Save();

        if (FirebaseManager.Instancia != null && FirebaseManager.Instancia.estaConectado)
        {
            FirebaseManager.Instancia.GuardarDesbloqueoEnNube(nombreNivel);
        }
    }

    public void DesbloquearNivelDesdeNube(string nombreNivel)
    {
        string clave = $"Desbloqueado_{nombreNivel}";
        PlayerPrefs.SetInt(clave, 1);
        PlayerPrefs.Save();
    }

    public bool EsNivelDesbloqueado(string nombreNivel)
    {
        string clave = $"Desbloqueado_{nombreNivel}";
        return PlayerPrefs.GetInt(clave, 0) == 1;
    }

    [ContextMenu("Borrar Todos los Datos")]
    public void BorrarTodosLosDatos()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        UnityEngine.Debug.LogWarning("¡TODOS LOS DATOS LOCALES HAN SIDO BORRADOS!");
    }
}