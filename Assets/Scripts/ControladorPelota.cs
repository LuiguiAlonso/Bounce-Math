    using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(AudioSource))] 
public class ControladorPelota : MonoBehaviour
{
    public float velocidad = 6f;
    public float salto = 8f;
    public LayerMask capaSuelo;
    public float distanciaRaycast = 0.6f;

    [Header("Sonidos")]
    public AudioClip sonidoSalto; 

    private Rigidbody2D rb;
    private bool estaEnSuelo;
    private AudioSource miAudioSource; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        miAudioSource = GetComponent<AudioSource>(); 
    }

    void Update()
    {
        Salto();
    }

    void FixedUpdate()
    {
        TocaSuelo();
        Movimiento();
    }

    void Salto()
    {
        bool saltoTeclado = Input.GetButtonDown("Jump");
        bool saltoMovil = InputMovil.Saltando;

        if (estaEnSuelo && (saltoTeclado || saltoMovil))
        {
            rb.AddForce(Vector2.up * salto, ForceMode2D.Impulse);

            if (miAudioSource != null && sonidoSalto != null)
            {
                miAudioSource.PlayOneShot(sonidoSalto);
            }

            InputMovil.Saltando = false;
        }
    }

    void Movimiento()
    {
        // 1. Obtenemos input de teclado (-1 a 1)
        float inputHorizontal = Input.GetAxis("Horizontal");

        // 2. Si es 0 (no toca teclado), comprobamos el móvil
        if (inputHorizontal == 0)
        {
            if (InputMovil.MoviendoDerecha) inputHorizontal = 1;
            if (InputMovil.MoviendoIzquierda) inputHorizontal = -1;
        }

        rb.linearVelocity = new Vector2(inputHorizontal * velocidad, rb.linearVelocity.y);
    }

    void TocaSuelo()
    {
        estaEnSuelo = Physics2D.Raycast(transform.position, Vector2.down, distanciaRaycast, capaSuelo);
        Color colorRayo = estaEnSuelo ? Color.green : Color.red;
        Debug.DrawRay(transform.position, Vector2.down * distanciaRaycast, colorRayo);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstaculo"))
        {
            GameManager.Instancia.PerderJuego(transform.position);
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Llave"))
        {
            Llave llave = other.GetComponent<Llave>();
            if (llave != null)
            {
                llave.Colectar();
            }
        }
        else if (other.CompareTag("Vida"))
        {
            Vida itemVida = other.GetComponent<Vida>();
            if (itemVida != null) itemVida.Colectar();
        }
        else if (other.CompareTag("Salida"))
        {
            GameManager.Instancia.IntentarSalir();
        }
        else if (other.CompareTag("Checkpoint"))
        {
            Checkpoint checkpoint = other.GetComponent<Checkpoint>();
            if (checkpoint != null)
            {
                GameManager.Instancia.ActivarNuevoCheckpoint(checkpoint);
            }
        }
    }

    public void MorirPorQuiz(string feedback) 
    {
        GameManager.Instancia.FallarQuiz(feedback); 
        gameObject.SetActive(false);
    }
}