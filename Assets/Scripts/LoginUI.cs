using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [Header("Campos de Texto")]
    public TMP_InputField inputUsername; 
    public TMP_InputField inputPassword;

    [Header("Feedback")]
    public TextMeshProUGUI textoMensaje;

    [Header("Paneles")]
    public GameObject panelLogin;
    public GameObject panelMenuPrincipal;
    public GameObject contenedorDecoracion;

    private const string DOMINIO_FALSO = "@mathgame.com";

    void Start()
    {
        if (textoMensaje != null) textoMensaje.text = "";

        if (FirebaseManager.Instancia != null && FirebaseManager.Instancia.estaConectado)
        {
            MostrarMenuPrincipal();
        }
        else
        {
            if (contenedorDecoracion != null) contenedorDecoracion.SetActive(false);
            panelLogin.SetActive(true);
            if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false);
        }
    }

    public void ClickRegistrar()
    {
        string username = inputUsername.text;
        string pass = inputPassword.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(pass))
        {
            textoMensaje.text = "Por favor, llena todos los campos.";
            textoMensaje.color = Color.yellow;
            return;
        }

        string usuarioSinEspacios = username.Replace(" ", "");
        string emailFalso = usuarioSinEspacios + DOMINIO_FALSO;
        textoMensaje.text = "Registrando...";
        textoMensaje.color = Color.white;

        FirebaseManager.Instancia.RegistrarUsuario(emailFalso, pass, username,
            (mensajeExito) => {
                textoMensaje.text = mensajeExito;
                textoMensaje.color = Color.green;
                Invoke("MostrarMenuPrincipal", 1.5f);
            },
            (mensajeError) => {
                if (mensajeError.Contains("email already in use"))
                {
                    textoMensaje.text = "Ese nombre de usuario ya está ocupado.";
                }
                else
                {
                    textoMensaje.text = "Error: " + mensajeError;
                }
                textoMensaje.color = Color.red;
            }
        );
    }

    public void ClickLogin()
    {
        string username = inputUsername.text;
        string pass = inputPassword.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(pass))
        {
            textoMensaje.text = "Ingresa tu usuario y contraseña.";
            textoMensaje.color = Color.yellow;
            return;
        }

        string usuarioSinEspacios = username.Replace(" ", "");
        string emailFalso = usuarioSinEspacios + DOMINIO_FALSO;

        textoMensaje.text = "Conectando...";
        textoMensaje.color = Color.white;

        FirebaseManager.Instancia.LoginUsuario(emailFalso, pass,
            (mensajeExito) => {
                textoMensaje.text = "¡Bienvenido " + username + "!";
                textoMensaje.color = Color.green;
                Invoke("MostrarMenuPrincipal", 1.5f);
            },
            (mensajeError) => {
                textoMensaje.text = "Usuario o contraseña incorrectos.";
                textoMensaje.color = Color.red;
            }
        );
    }

    private void MostrarMenuPrincipal()
    {
        panelLogin.SetActive(false);
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
        if (contenedorDecoracion != null) contenedorDecoracion.SetActive(true);

        LevelButton[] botones = FindObjectsByType<LevelButton>(FindObjectsSortMode.None);
        foreach (var btn in botones) btn.ActualizarVisual();
    }

    public void ClickCerrarSesion()
    {
        if (FirebaseManager.Instancia != null)
        {
            FirebaseManager.Instancia.CerrarSesion();
        }

        panelLogin.SetActive(true);
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false);
        if (contenedorDecoracion != null) contenedorDecoracion.SetActive(false);

        // Limpiamos solo usuario y contraseña
        inputUsername.text = "";
        inputPassword.text = "";
        textoMensaje.text = "";
    }

    public void IrAComentarios()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Comentarios");
    }
}