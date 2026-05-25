using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // ¡Necesario para cambiar de escena!

public class FeedbackUI : MonoBehaviour
{
    [Header("Input")]
    public TMP_InputField inputComentario;
    public Button botonEnviar;

    [Header("Lista Visual")]
    public Transform contenedorLista;
    public GameObject prefabComentario;
    public GameObject textoEstado; // ¡CAMBIO! Ahora es GameObject para evitar el error

    // Ya no necesitamos "panelMenu" porque estamos en otra escena

    void Start() // Cambiado a Start para que cargue al entrar a la escena
    {
        CargarComentarios();
    }

    public void CargarComentarios()
    {
        foreach (Transform hijo in contenedorLista) Destroy(hijo.gameObject);

        // CORRECCIÓN DEL ERROR:
        if (textoEstado != null) textoEstado.SetActive(true);

        if (FirebaseManager.Instancia != null)
        {
            FirebaseManager.Instancia.ObtenerComentarios((lista) => {
                if (textoEstado != null) textoEstado.SetActive(false);

                foreach (var dato in lista)
                {
                    GameObject fila = Instantiate(prefabComentario, contenedorLista);

                    TextMeshProUGUI[] textos = fila.GetComponentsInChildren<TextMeshProUGUI>();

                    // Asumimos orden en el prefab: 0:Nombre, 1:Fecha, 2:Texto
                    if (textos.Length >= 3)
                    {
                        textos[0].text = dato.nombreUsuario;
                        textos[1].text = dato.fechaString;
                        textos[2].text = dato.texto;
                    }
                }
            });
        }
    }

    public void ClickEnviar()
    {
        string mensaje = inputComentario.text;
        if (string.IsNullOrEmpty(mensaje)) return;

        botonEnviar.interactable = false;
        inputComentario.text = "Enviando...";

        FirebaseManager.Instancia.EnviarComentario(mensaje, () => {
            inputComentario.text = "";
            botonEnviar.interactable = true;
            CargarComentarios();
        });
    }

    public void ClickAtras()
    {
        // ¡CAMBIO! Ahora cargamos la escena del menú
        SceneManager.LoadScene("Menu");
    }
}