using UnityEngine;

public class Electricidad : MonoBehaviour
{
    public enum Sentido { Derecha, Izquierda, Arriba, Abajo }

    [Header("Configuración de Movimiento")]
    public float velocidadMovimiento = 3f;
    public Sentido direccionInicial = Sentido.Derecha;

    private Rigidbody2D rb;
    private Vector2 direccionActual;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            // Opcional: Aumentar la masa para que el player no pueda empujarlo fisicamente
            rb.mass = 10000f;
        }

        switch (direccionInicial)
        {
            case Sentido.Derecha: direccionActual = Vector2.right; break;
            case Sentido.Izquierda: direccionActual = Vector2.left; break;
            case Sentido.Arriba: direccionActual = Vector2.up; break;
            case Sentido.Abajo: direccionActual = Vector2.down; break;
        }
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = direccionActual * velocidadMovimiento;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // --- ¡CORRECCIÓN AQUÍ! ---

        // 1. Si chocamos con el Player, NO calculamos rebote.
        // El Player morirá por su propia lógica (su script detecta el tag Obstaculo),
        // así que aquí simplemente no hacemos nada.
        if (collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        // 2. Solo rebotamos si chocamos con paredes, suelo u otros obstáculos
        if (collision.contactCount > 0)
        {
            Vector2 puntoNormal = collision.GetContact(0).normal;

            // Truco matemático: Forzamos que el rebote sea estricto (recto)
            // para evitar diagonales raras si pegamos en una esquina rara del mapa.
            // Si la normal es mayormente horizontal, rebotamos en X. Si es vertical, en Y.
            if (Mathf.Abs(puntoNormal.x) > Mathf.Abs(puntoNormal.y))
            {
                puntoNormal = new Vector2(Mathf.Sign(puntoNormal.x), 0);
            }
            else
            {
                puntoNormal = new Vector2(0, Mathf.Sign(puntoNormal.y));
            }

            direccionActual = Vector2.Reflect(direccionActual, puntoNormal).normalized;
        }
    }
}