using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class Vida : MonoBehaviour
{
    [Header("Sonido")]
    public AudioClip sonidoAlTomar;

    private SpriteRenderer spriteRenderer;
    private Collider2D miCollider;
    private AudioSource miAudioSource;
    private bool yaTomado = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        miCollider = GetComponent<Collider2D>();

        if (sonidoAlTomar != null)
        {
            miAudioSource = gameObject.AddComponent<AudioSource>();
            miAudioSource.clip = sonidoAlTomar;
            miAudioSource.playOnAwake = false;
        }
    }

    public void Colectar()
    {
        if (yaTomado) return;
        yaTomado = true;
        spriteRenderer.enabled = false;
        miCollider.enabled = false;

        if (miAudioSource != null)
        {
            miAudioSource.Play();
        }

        QuizManager.Instancia.SolicitarQuizDeVida();
        Destroy(gameObject, 2f);
    }
}