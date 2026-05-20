using UnityEngine;

// BoxCollider2D necessário para detectar quando o laser do jogador acerta a nave.
[RequireComponent(typeof(BoxCollider2D))]
public class MysteryShip : MonoBehaviour
{
    public float speed = 5f;       // Velocidade de travessia horizontal da nave
    public float cycleTime = 30f;  // Tempo em segundos entre cada aparição
    public int score = 300;        // Pontos concedidos ao abater a nave misteriosa

    // Pontos de destino calculados em Start() a partir das bordas da câmera.
    private Vector2 leftDestination;
    private Vector2 rightDestination;

    // Controla a direção: 1 = da esquerda para a direita, -1 = da direita para a esquerda.
    // Alternado a cada aparição para variar o lado de entrada.
    private int direction = -1;

    private bool spawned;  // Indica se a nave está visível e se movendo na tela

    private void Start()
    {
        // Calcula os pontos de destino nas bordas da câmera em coordenadas de mundo.
        // Adiciona/subtrai 1 unidade para que a nave saia completamente de vista antes de parar.
        Vector3 leftEdge = Camera.main.ViewportToWorldPoint(Vector3.zero);
        Vector3 rightEdge = Camera.main.ViewportToWorldPoint(Vector3.right);

        leftDestination = new Vector2(leftEdge.x - 1f, transform.position.y);
        rightDestination = new Vector2(rightEdge.x + 1f, transform.position.y);

        // Inicia desativada; o primeiro Spawn será agendado após cycleTime segundos.
        Despawn();
    }

    private void Update()
    {
        // Enquanto não estiver na tela, não faz nada.
        if (!spawned) return;

        if (direction == 1) {
            MoveRight();
        } else {
            MoveLeft();
        }
    }

    // Move para a direita e verifica se atingiu o destino; ao chegar, se despawna.
    private void MoveRight()
    {
        transform.position += speed * Time.deltaTime * Vector3.right;

        if (transform.position.x >= rightDestination.x) {
            Despawn();
        }
    }

    // Move para a esquerda e verifica se atingiu o destino; ao chegar, se despawna.
    private void MoveLeft()
    {
        transform.position += speed * Time.deltaTime * Vector3.left;

        if (transform.position.x <= leftDestination.x) {
            Despawn();
        }
    }

    // Inverte a direção a cada aparição e posiciona a nave na borda oposta à direção atual,
    // de modo que ela entre pelo lado correto da tela.
    private void Spawn()
    {
        direction *= -1;  // Alterna entre 1 (direita) e -1 (esquerda)

        // Se vai para a direita, começa na borda esquerda, e vice-versa.
        if (direction == 1) {
            transform.position = leftDestination;
        } else {
            transform.position = rightDestination;
        }

        spawned = true;
    }

    // Oculta a nave e agenda o próximo Spawn após cycleTime segundos.
    // Posiciona a nave no destino oposto à direção atual (fora da tela).
    private void Despawn()
    {
        spawned = false;

        // Garante que a posição seja válida para o próximo Spawn.
        if (direction == 1) {
            transform.position = rightDestination;
        } else {
            transform.position = leftDestination;
        }

        Invoke(nameof(Spawn), cycleTime);
    }

    // Se o laser do jogador acertar a nave, a despawna imediatamente
    // e notifica o GameManager para contabilizar a pontuação.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Laser"))
        {
            Despawn();
            GameManager.Instance.OnMysteryShipKilled(this);
        }
    }

}
