using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LeaderboardUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public Transform contenedorLista; 
    public GameObject filaPrefab;     
    public TextMeshProUGUI textoEstado; 

    public void MostrarTabla()
    {
        foreach (Transform hijo in contenedorLista)
        {
            Destroy(hijo.gameObject);
        }

        if (textoEstado != null)
        {
            textoEstado.text = "Cargando datos...";
            textoEstado.gameObject.SetActive(true);
        }

        if (FirebaseManager.Instancia != null)
        {
            FirebaseManager.Instancia.ObtenerLeaderboard(AlRecibirDatos);
        }
    }

    private void AlRecibirDatos(List<FirebaseManager.PerfilJugador> listaJugadores)
    {
        if (textoEstado != null) textoEstado.gameObject.SetActive(false);

        int puesto = 1;

        foreach (var jugador in listaJugadores)
        {
            GameObject nuevaFila = Instantiate(filaPrefab, contenedorLista);

            TextMeshProUGUI[] textos = nuevaFila.GetComponentsInChildren<TextMeshProUGUI>();

            if (textos.Length >= 3)
            {
                textos[0].text = "#" + puesto;
                textos[1].text = jugador.username;
                textos[2].text = jugador.estrellas.ToString();
            }

            puesto++;
        }
    }
}