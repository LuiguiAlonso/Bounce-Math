using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;
using System;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instancia { get; private set; }

    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private FirebaseUser usuarioActual;

    [Header("Estado")]
    public bool estaConectado = false;

    void Awake()
    {
        if (Instancia != null && Instancia != this) Destroy(this.gameObject);
        else
        {
            Instancia = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InicializarServicios();
            }
            else
            {
                UnityEngine.Debug.LogError("Error fatal Firebase: " + dependencyStatus);
            }
        });
    }

    void InicializarServicios()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        if (auth.CurrentUser != null)
        {
            usuarioActual = auth.CurrentUser;
            estaConectado = true;
            UnityEngine.Debug.Log("Sesión recuperada: " + usuarioActual.Email);
            CargarDatosDelUsuario();
        }
    }

    // --- AUTENTICACIÓN ---

    // ¡AQUÍ ESTÁ EL CAMBIO! AHORA ACEPTA "string username"
    public void RegistrarUsuario(string email, string password, string username, Action<string> onExito, Action<string> onError)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                onError?.Invoke(task.Exception.GetBaseException().Message);
                return;
            }

            usuarioActual = task.Result.User;
            estaConectado = true;

            // Borramos datos locales del usuario anterior
            if (DataManager.Instancia != null) DataManager.Instancia.BorrarTodosLosDatos();

            // Pasamos el username para guardarlo
            CrearDocumentoInicial(username);

            onExito?.Invoke("¡Registro Exitoso!");
        });
    }

    public void LoginUsuario(string email, string password, Action<string> onExito, Action<string> onError)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                onError?.Invoke("Error: Credenciales incorrectas.");
                return;
            }

            usuarioActual = task.Result.User;
            estaConectado = true;

            // Borramos datos locales antes de cargar los nuevos
            if (DataManager.Instancia != null) DataManager.Instancia.BorrarTodosLosDatos();

            CargarDatosDelUsuario();
            onExito?.Invoke("Bienvenido de nuevo.");
        });
    }

    public void CerrarSesion()
    {
        if (auth != null) auth.SignOut();
        usuarioActual = null;
        estaConectado = false;
        if (DataManager.Instancia != null) DataManager.Instancia.BorrarTodosLosDatos();
        UnityEngine.Debug.Log("Sesión cerrada.");
    }

    // --- BASE DE DATOS ---

    private void CrearDocumentoInicial(string username)
    {
        if (usuarioActual == null) return;

        Dictionary<string, object> datosIniciales = new Dictionary<string, object>
        {
            { "email", usuarioActual.Email },
            { "username", username }, // Guardamos el nombre
            { "totalEstrellas", 0 },
            { "fecha_registro", FieldValue.ServerTimestamp }
        };

        db.Collection("jugadores").Document(usuarioActual.UserId).SetAsync(datosIniciales, SetOptions.MergeAll);
    }

    // Modificado para recibir el TOTAL de estrellas
    public void GuardarEstrellasEnNube(string nombreNivel, int estrellasNivel, int totalEstrellas)
    {
        if (!estaConectado || usuarioActual == null) return;

        Dictionary<string, object> datos = new Dictionary<string, object>
        {
            { nombreNivel, estrellasNivel },
            { "totalEstrellas", totalEstrellas }
        };

        db.Collection("jugadores").Document(usuarioActual.UserId).SetAsync(datos, SetOptions.MergeAll);
        UnityEngine.Debug.Log($"[NUBE] Guardado: {nombreNivel} y Total: {totalEstrellas}");
    }

    public void GuardarDesbloqueoEnNube(string nombreNivel)
    {
        if (!estaConectado || usuarioActual == null) return;

        string clave = $"Desbloqueado_{nombreNivel}";
        Dictionary<string, object> datos = new Dictionary<string, object>
        {
            { clave, 1 }
        };

        db.Collection("jugadores").Document(usuarioActual.UserId).SetAsync(datos, SetOptions.MergeAll);
    }

    public void CargarDatosDelUsuario()
    {
        if (!estaConectado || usuarioActual == null) return;

        db.Collection("jugadores").Document(usuarioActual.UserId).GetSnapshotAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted) return;

            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                Dictionary<string, object> datos = snapshot.ToDictionary();
                foreach (var par in datos)
                {
                    if (par.Key.StartsWith("Nivel_"))
                    {
                        int estrellas = Convert.ToInt32(par.Value);
                        DataManager.Instancia.GuardarEstrellasDesdeNube(par.Key, estrellas);
                    }
                    else if (par.Key.StartsWith("Desbloqueado_"))
                    {
                        string nombreNivel = par.Key.Replace("Desbloqueado_", "");
                        DataManager.Instancia.DesbloquearNivelDesdeNube(nombreNivel);
                    }
                }
                UnityEngine.Debug.Log("[NUBE] Datos sincronizados.");

                // Actualizar botones visualmente
                LevelButton[] botones = UnityEngine.Object.FindObjectsByType<LevelButton>(FindObjectsSortMode.None);
                foreach (var btn in botones) btn.ActualizarVisual();
            }
        });
    }

    // --- LEADERBOARD ---

    public struct PerfilJugador
    {
        public string username;
        public int estrellas;
    }

    public void ObtenerLeaderboard(Action<List<PerfilJugador>> alTerminar)
    {
        // Pedimos los 10 mejores
        Query consulta = db.Collection("jugadores").OrderByDescending("totalEstrellas").Limit(10);

        consulta.GetSnapshotAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                UnityEngine.Debug.LogError("Error al obtener leaderboard");
                return;
            }

            List<PerfilJugador> listaTop = new List<PerfilJugador>();

            foreach (DocumentSnapshot documento in task.Result.Documents)
            {
                Dictionary<string, object> datos = documento.ToDictionary();

                string nombre = datos.ContainsKey("username") ? datos["username"].ToString() : "Anonimo";
                int estrellas = datos.ContainsKey("totalEstrellas") ? Convert.ToInt32(datos["totalEstrellas"]) : 0;

                listaTop.Add(new PerfilJugador { username = nombre, estrellas = estrellas });
            }

            alTerminar?.Invoke(listaTop);
        });
    }

    // --- SECCIÓN FEEDBACK / COMENTARIOS ---

    // Estructura de datos para el comentario
    public struct ComentarioData
    {
        public string nombreUsuario;
        public string texto;
        public string fechaString; // Lo convertiremos a texto para mostrarlo fácil
    }

    public void EnviarComentario(string textoComentario, Action alTerminar)
    {
        if (usuarioActual == null) return;

        // 1. Primero obtenemos el nombre del usuario actual desde su perfil
        db.Collection("jugadores").Document(usuarioActual.UserId).GetSnapshotAsync().ContinueWithOnMainThread(task => {
            if (task.Result.Exists)
            {
                string miNombre = task.Result.GetValue<string>("username");

                // 2. Preparamos el comentario
                Dictionary<string, object> nuevoComentario = new Dictionary<string, object>
                {
                    { "nombre", miNombre },
                    { "texto", textoComentario },
                    { "fecha", FieldValue.ServerTimestamp } // Hora del servidor de Google
                };

                // 3. Lo guardamos en una colección nueva llamada "comentarios"
                db.Collection("comentarios").AddAsync(nuevoComentario).ContinueWithOnMainThread(t => {
                    UnityEngine.Debug.Log("Comentario enviado.");
                    alTerminar?.Invoke();
                });
            }
        });
    }

    public void ObtenerComentarios(Action<List<ComentarioData>> alTerminar)
    {
        // Pedimos los últimos 20 comentarios, ordenados por fecha (el más nuevo arriba)
        Query consulta = db.Collection("comentarios").OrderByDescending("fecha").Limit(20);

        consulta.GetSnapshotAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                UnityEngine.Debug.LogError("Error al cargar comentarios.");
                return;
            }

            List<ComentarioData> listaComentarios = new List<ComentarioData>();

            foreach (DocumentSnapshot doc in task.Result.Documents)
            {
                Dictionary<string, object> datos = doc.ToDictionary();

                string nombre = datos.ContainsKey("nombre") ? datos["nombre"].ToString() : "Anónimo";
                string texto = datos.ContainsKey("texto") ? datos["texto"].ToString() : "";

                // Convertir el Timestamp de Firebase a fecha legible
                string fechaStr = "Reciente";
                if (datos.ContainsKey("fecha") && datos["fecha"] is Timestamp tiempo)
                {
                    fechaStr = tiempo.ToDateTime().ToString("dd/MM/yyyy");
                }

                listaComentarios.Add(new ComentarioData
                {
                    nombreUsuario = nombre,
                    texto = texto,
                    fechaString = fechaStr
                });
            }

            alTerminar?.Invoke(listaComentarios);
        });
    }
}