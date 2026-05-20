using UnityEngine;

// Exige SpriteRenderer para trocar sprites de animação,
// Rigidbody2D e BoxCollider2D para detecção de colisão 2D.
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Invader : MonoBehaviour
{
    public Sprite[] animationSprites = new Sprite[0];  // Frames de animação do invasor (configurados no Inspector)
    public float animationTime = 1f;                    // Intervalo em segundos entre cada frame
    public int score = 10;                              // Pontos concedidos ao jogador ao abater este invasor

    private SpriteRenderer spriteRenderer;  // Componente responsável por exibir o sprite atual
    private int animationFrame;             // Índice do frame de animação atual

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Inicializa com o primeiro frame para que o invasor apareça corretamente antes de Start().
        spriteRenderer.sprite = animationSprites[0];
    }

    private void Start()
    {
        // Agenda a troca de sprite repetidamente a cada animationTime segundos.
        // InvokeRepeating é mais eficiente do que chamar no Update pois evita verificação a cada frame.
        InvokeRepeating(nameof(AnimateSprite), animationTime, animationTime);
    }

    // Avança para o próximo frame de animação; volta ao início ao atingir o último frame.
    private void AnimateSprite()
    {
        animationFrame++;

        if (animationFrame >= animationSprites.Length) {
            animationFrame = 0;
        }

        spriteRenderer.sprite = animationSprites[animationFrame];
    }

    // Responde a colisões trigger 2D:
    // - Layer "Laser": o invasor foi acertado pelo jogador → notifica o GameManager.
    // - Layer "Boundary": o invasor chegou à borda da tela → avisa que o limite foi atingido.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Laser")) {
            GameManager.Instance.OnInvaderKilled(this);
        } else if (other.gameObject.layer == LayerMask.NameToLayer("Boundary")) {
            GameManager.Instance.OnBoundaryReached();
        }
    }

}
