using UnityEngine;

// Rigidbody2D é necessário para o sistema de física 2D detectar colisões trigger.
// BoxCollider2D define a área de colisão do projétil.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Projectile : MonoBehaviour
{
    private BoxCollider2D boxCollider;       // Referência ao collider para repassar ao Bunker na verificação de colisão
    public Vector3 direction = Vector3.up;   // Direção do movimento: Vector3.up para laser (jogador), Vector3.down para míssil (invasores)
    public float speed = 20f;               // Velocidade de deslocamento em unidades por segundo

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        // Desloca o projétil na direção configurada a cada frame, independente da taxa de quadros.
        transform.position += speed * Time.deltaTime * direction;
    }

    // OnTriggerEnter2D é chamado quando o collider entra em um trigger pela primeira vez.
    // Necessário para detectar objetos estáticos (ex: bunker) e projéteis rápidos.
    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckCollision(other);
    }

    // OnTriggerStay2D é chamado enquanto o collider permanece dentro de um trigger.
    // Garante que projéteis muito rápidos que atravessem um bunker em um frame ainda causem dano.
    private void OnTriggerStay2D(Collider2D other)
    {
        CheckCollision(other);
    }

    // Verifica se o projétil deve ser destruído ao colidir com um objeto.
    // Se não for um bunker (ou se o bunker confirmar colisão pixel a pixel), destrói o projétil.
    private void CheckCollision(Collider2D other)
    {
        Bunker bunker = other.gameObject.GetComponent<Bunker>();

        // bunker == null: colidiu com algo que não é bunker (jogador, invasor, borda) → destrói.
        // bunker.CheckCollision(...) == true: acertou um pixel sólido do bunker → destrói.
        if (bunker == null || bunker.CheckCollision(boxCollider, transform.position)) {
            Destroy(gameObject);
        }
    }

}
