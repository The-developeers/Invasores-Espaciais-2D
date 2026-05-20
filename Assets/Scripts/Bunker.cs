using UnityEngine;

// SpriteRenderer exibe e permite modificar a textura do bunker pixel a pixel.
// BoxCollider2D define a área de trigger para detectar projéteis e invasores.
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class Bunker : MonoBehaviour
{
    public Texture2D splat;          // Textura em forma de "explosão" usada como máscara de destruição
    private Texture2D originalTexture; // Cópia da textura original para restaurar no reset
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();

        // Salva a textura original antes de qualquer modificação.
        originalTexture = spriteRenderer.sprite.texture;

        ResetBunker();
    }

    // Restaura o bunker ao estado inicial copiando a textura original.
    // Chamado no início do jogo e a cada novo round.
    public void ResetBunker()
    {
        // Cria uma cópia exclusiva da textura para este bunker.
        // Necessário porque modificamos os pixels diretamente na textura;
        // sem a cópia, todos os bunkers compartilhariam a mesma textura e um dano afetaria todos.
        CopyTexture(originalTexture);

        gameObject.SetActive(true);
    }

    // Cria uma textura independente com as mesmas propriedades e pixels da fonte.
    private void CopyTexture(Texture2D source)
    {
        Texture2D copy = new Texture2D(source.width, source.height, source.format, false)
        {
            filterMode = source.filterMode,
            anisoLevel = source.anisoLevel,
            wrapMode = source.wrapMode
        };

        // GetPixels32/SetPixels32 transferem todos os pixels de uma vez (mais eficiente que pixel a pixel).
        copy.SetPixels32(source.GetPixels32());
        copy.Apply();

        // Cria um novo Sprite a partir da textura copiada, mantendo as mesmas dimensões e pivot.
        Sprite sprite = Sprite.Create(copy, spriteRenderer.sprite.rect, new Vector2(0.5f, 0.5f), spriteRenderer.sprite.pixelsPerUnit);
        spriteRenderer.sprite = sprite;
    }

    // Verifica colisão pixel-precisa entre o projétil e o bunker.
    // Testa o centro e as quatro extremidades do collider do projétil para cobrir colisões parciais.
    // Retorna true se ao menos um ponto colidiu com um pixel sólido (e causou dano).
    public bool CheckCollision(BoxCollider2D other, Vector3 hitPoint)
    {
        Vector2 offset = other.size / 2;

        return Splat(hitPoint) ||
               Splat(hitPoint + (Vector3.down * offset.y)) ||
               Splat(hitPoint + (Vector3.up * offset.y)) ||
               Splat(hitPoint + (Vector3.left * offset.x)) ||
               Splat(hitPoint + (Vector3.right * offset.x));
    }

    // Aplica a textura de "splat" (explosão) ao ponto de impacto no bunker.
    // Multiplica o canal alpha de cada pixel do bunker pelo alpha do splat,
    // tornando os pixels transparentes onde o splat é opaco (simula destruição).
    // Retorna true se o ponto de impacto era um pixel sólido (não transparente).
    private bool Splat(Vector3 hitPoint)
    {
        // Converte o ponto de mundo para coordenadas de pixel na textura;
        // retorna false se o ponto estiver fora da textura ou em pixel transparente.
        if (!CheckPoint(hitPoint, out int px, out int py)) {
            return false;
        }

        Texture2D texture = spriteRenderer.sprite.texture;

        // Centraliza o splat em torno do ponto de impacto.
        px -= splat.width / 2;
        py -= splat.height / 2;

        int startX = px;

        // Percorre toda a textura do splat e combina cada pixel com o bunker.
        for (int y = 0; y < splat.height; y++)
        {
            px = startX;

            for (int x = 0; x < splat.width; x++)
            {
                // A multiplicação do alpha faz a "destruição" gradual:
                // pixels onde o splat é totalmente opaco (a=1) ficam igual,
                // pixels onde o splat é totalmente transparente (a=0) somem do bunker.
                Color pixel = texture.GetPixel(px, py);
                pixel.a *= splat.GetPixel(x, y).a;
                texture.SetPixel(px, py, pixel);
                px++;
            }

            py++;
        }

        // Apply() envia as modificações de pixel para a GPU; necessário para que a mudança apareça.
        texture.Apply();

        return true;
    }

    // Converte um ponto em coordenadas de mundo para coordenadas de pixel na textura do bunker.
    // Retorna false se o pixel for transparente (vazio), indicando que não há material para colidir.
    private bool CheckPoint(Vector3 hitPoint, out int px, out int py)
    {
        // Converte de espaço de mundo para espaço local do bunker.
        Vector3 localPoint = transform.InverseTransformPoint(hitPoint);

        // Desloca da origem do centro para o canto inferior-esquerdo do collider
        // para facilitar a conversão para coordenadas UV (0 a 1).
        localPoint.x += boxCollider.size.x / 2;
        localPoint.y += boxCollider.size.y / 2;

        Texture2D texture = spriteRenderer.sprite.texture;

        // Converte as coordenadas locais normalizadas (0–1) para índices de pixel (0–width/height).
        px = (int)(localPoint.x / boxCollider.size.x * texture.width);
        py = (int)(localPoint.y / boxCollider.size.y * texture.height);

        // Retorna true somente se o pixel for sólido (alpha > 0), indicando material presente.
        return texture.GetPixel(px, py).a != 0f;
    }

    // Se um invasor encostar diretamente no bunker, desativa-o completamente
    // (o invasor o destrói instantaneamente ao alcançar essa posição).
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Invader")) {
            gameObject.SetActive(false);
        }
    }

}
