using UnityEngine;

public class Invaders : MonoBehaviour
{
    [Header("Invaders")]
    public Invader[] prefabs = new Invader[5];    // Um prefab por linha (5 tipos diferentes de invasores)
    public AnimationCurve speed = new AnimationCurve(); // Curva que define a velocidade em função do % de invasores eliminados
    private Vector3 direction = Vector3.right;    // Direção atual do movimento horizontal da grade
    private Vector3 initialPosition;             // Posição original salva para reset entre rounds

    [Header("Grid")]
    public int rows = 5;       // Número de linhas de invasores
    public int columns = 11;   // Número de colunas de invasores

    [Header("Missiles")]

    public Projectile missilePrefab;    // Prefab do míssil disparado pelos invasores
    public float missileSpawnRate = 1f; // Intervalo em segundos entre tentativas de disparo

    private void Awake()
    {
        // Salva a posição inicial antes de qualquer movimento para poder resetar depois.
        initialPosition = transform.position;

        CreateInvaderGrid();
    }

    // Cria a grade de invasores centralizada na tela.
    // Cada linha usa o prefab correspondente ao seu índice (linha 0 → prefabs[0], etc.).
    private void CreateInvaderGrid()
    {
        for (int i = 0; i < rows; i++)
        {
            // Calcula a largura e altura totais da grade para centralizar o offset.
            float width = 2f * (columns - 1);
            float height = 2f * (rows - 1);

            // centerOffset desloca a grade de forma que fique centralizada na posição do pai.
            Vector2 centerOffset = new Vector2(-width * 0.5f, -height * 0.5f);

            // rowPosition é a posição local da célula mais à esquerda desta linha.
            Vector3 rowPosition = new Vector3(centerOffset.x, (2f * i) + centerOffset.y, 0f);

            for (int j = 0; j < columns; j++)
            {
                // Instancia o invasor como filho deste transform para herdar o movimento da grade.
                Invader invader = Instantiate(prefabs[i], transform);

                // Posiciona o invasor na coluna j da linha i com espaçamento de 2 unidades.
                Vector3 position = rowPosition;
                position.x += 2f * j;
                invader.transform.localPosition = position;
            }
        }
    }

    private void Start()
    {
        // Inicia o ciclo de disparo de mísseis com o intervalo definido em missileSpawnRate.
        InvokeRepeating(nameof(MissileAttack), missileSpawnRate, missileSpawnRate);
    }

    // A cada ciclo, sorteia qual invasor vivo irá disparar um míssil.
    // A probabilidade de disparo por invasor é 1/amountAlive, então quanto menos
    // invasores restarem, maior a chance de cada um atirar (mantém a pressão no jogador).
    private void MissileAttack()
    {
        int amountAlive = GetAliveCount();

        if (amountAlive == 0) {
            return;
        }

        foreach (Transform invader in transform)
        {
            // Ignora invasores desativados (já eliminados).
            if (!invader.gameObject.activeInHierarchy) {
                continue;
            }

            // Random.value retorna um float entre 0 e 1; se menor que 1/amountAlive,
            // este invasor atira. O break garante no máximo um míssil por ciclo.
            if (Random.value < (1f / amountAlive))
            {
                Instantiate(missilePrefab, invader.position, Quaternion.identity);
                break;
            }
        }
    }

    private void Update()
    {
        // Calcula a porcentagem de invasores eliminados para usar na AnimationCurve de velocidade.
        int totalCount = rows * columns;
        int amountAlive = GetAliveCount();
        int amountKilled = totalCount - amountAlive;
        float percentKilled = amountKilled / (float)totalCount;

        // Avalia a velocidade na curva: quanto mais invasores eliminados, maior a velocidade.
        float speed = this.speed.Evaluate(percentKilled);
        transform.position += speed * Time.deltaTime * direction;

        // Obtém as bordas da câmera em coordenadas de mundo para checar se algum
        // invasor atingiu o limite lateral da tela.
        Vector3 leftEdge = Camera.main.ViewportToWorldPoint(Vector3.zero);
        Vector3 rightEdge = Camera.main.ViewportToWorldPoint(Vector3.right);

        // Percorre os filhos verificando se algum invasor vivo chegou à borda.
        // Usa margem de 1 unidade para que o invasor não ultrapasse a tela.
        foreach (Transform invader in transform)
        {
            if (!invader.gameObject.activeInHierarchy) {
                continue;
            }

            if (direction == Vector3.right && invader.position.x >= (rightEdge.x - 1f))
            {
                AdvanceRow();
                break;
            }
            else if (direction == Vector3.left && invader.position.x <= (leftEdge.x + 1f))
            {
                AdvanceRow();
                break;
            }
        }
    }

    // Inverte a direção horizontal e desce a grade uma unidade,
    // replicando o comportamento clássico do Space Invaders original.
    private void AdvanceRow()
    {
        direction = new Vector3(-direction.x, 0f, 0f);

        Vector3 position = transform.position;
        position.y -= 1f;
        transform.position = position;
    }

    // Reativa todos os invasores filhos e restaura direção e posição inicial da grade.
    public void ResetInvaders()
    {
        direction = Vector3.right;
        transform.position = initialPosition;

        foreach (Transform invader in transform) {
            invader.gameObject.SetActive(true);
        }
    }

    // Conta quantos invasores filhos ainda estão ativos (vivos).
    public int GetAliveCount()
    {
        int count = 0;

        foreach (Transform invader in transform)
        {
            if (invader.gameObject.activeSelf) {
                count++;
            }
        }

        return count;
    }

}
