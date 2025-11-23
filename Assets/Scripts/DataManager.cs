
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
        // La clave será "Estrellas_Nivel_1"
        string clave = $"Estrellas_{nombreNivel}";

        // Guardamos solo si el nuevo puntaje es MEJOR que el anterior
        if (estrellas > GetEstrellas(nombreNivel))
        {
            PlayerPrefs.SetInt(clave, estrellas);
            PlayerPrefs.Save();
            Debug.Log($"Datos guardados: {clave} = {estrellas} estrellas");
        }
    }

    public int GetEstrellas(string nombreNivel)
    {
        string clave = $"Estrellas_{nombreNivel}";
        return PlayerPrefs.GetInt(clave, 0);
    }

    public void DesbloquearNivel(string nombreNivel)
    {
        string clave = $"Desbloqueado_{nombreNivel}";
        PlayerPrefs.SetInt(clave, 1);
        PlayerPrefs.Save();
        Debug.Log($"Nivel Desbloqueado: {nombreNivel}");
    }

    public bool EsNivelDesbloqueado(string nombreNivel)
    {
        string clave = $"Desbloqueado_{nombreNivel}";
        return PlayerPrefs.GetInt(clave, 0) == 1;
    }
}