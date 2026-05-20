using UnityEngine;

// Exige que o GameObject tenha Rigidbody2D e BoxCollider2D anexados;
// o Unity os adiciona automaticamente caso não existam.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Player : MonoBehaviour
{
    public float speed = 5f;          // Velocidade de deslocamento horizontal do jogador
    public Projectile laserPrefab;    // Prefab do laser instanciado ao atirar
    private Projectile laser;         // Referência ao laser ativo; nulo quando não há laser na cena

    private void Update()
    {
        Vector3 position = transform.position;

        // Move o jogador para a esquerda com A ou seta esquerda,
        // ou para a direita com D ou seta direita.
        // Multiplica por Time.deltaTime para movimento independente de frame rate.
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
            position.x -= speed * Time.deltaTime;
        } else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
            position.x += speed * Time.deltaTime;
        }

        // Converte as bordas da viewport (0,0) e (1,0) para coordenadas de mundo,
        // então limita a posição x do jogador entre as bordas da câmera.
        Vector3 leftEdge = Camera.main.ViewportToWorldPoint(Vector3.zero);
        Vector3 rightEdge = Camera.main.ViewportToWorldPoint(Vector3.right);
        position.x = Mathf.Clamp(position.x, leftEdge.x, rightEdge.x);

        transform.position = position;

        // Permite apenas um laser ativo por vez: se laser for nulo (foi destruído ou
        // nunca existiu) e o jogador pressionar Espaço ou clique esquerdo, instancia um novo.
        if (laser == null && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))) {
            laser = Instantiate(laserPrefab, transform.position, Quaternion.identity);
        }
    }

    // Detecta colisões com triggers 2D: se for um míssil inimigo ou um invasor,
    // notifica o GameManager para processar a morte do jogador.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Missile") ||
            other.gameObject.layer == LayerMask.NameToLayer("Invader")) {
            GameManager.Instance.OnPlayerKilled(this);
        }
    }

}
