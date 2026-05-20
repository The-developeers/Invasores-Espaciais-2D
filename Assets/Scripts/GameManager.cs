using UnityEngine;
using UnityEngine.UI;

// Garante que o GameManager seja inicializado antes de qualquer outro script,
// evitando erros de referência nula quando outros objetos tentam acessá-lo no Awake.
[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    // Singleton: permite que qualquer script acesse o GameManager sem precisar de
    // uma referência direta, usando GameManager.Instance.
    public static GameManager Instance { get; private set; }

    // Referências aos elementos de UI configurados no Inspector do Unity.
    [SerializeField] private GameObject gameOverUI;  // Painel exibido ao encerrar o jogo
    [SerializeField] private Text scoreText;          // Texto que exibe a pontuação
    [SerializeField] private Text livesText;          // Texto que exibe as vidas restantes

    // Referências aos principais componentes de jogo encontrados na cena.
    private Player player;
    private Invaders invaders;
    private MysteryShip mysteryShip;
    private Bunker[] bunkers;  // Array com todos os bunkers da cena

    // Propriedades públicas de leitura apenas para pontuação e vidas;
    // somente o GameManager pode modificá-las internamente.
    public int score { get; private set; } = 0;
    public int lives { get; private set; } = 3;

    private void Awake()
    {
        // Implementação do padrão Singleton: se já existir uma instância,
        // destrói este objeto imediatamente para não haver duplicatas.
        if (Instance != null) {
            DestroyImmediate(gameObject);
        } else {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        // Limpa a referência estática ao destruir o objeto, evitando ponteiro inválido.
        if (Instance == this) {
            Instance = null;
        }
    }

    private void Start()
    {
        // Busca automaticamente na cena os componentes necessários ao gerenciamento do jogo.
        player = FindObjectOfType<Player>();
        invaders = FindObjectOfType<Invaders>();
        mysteryShip = FindObjectOfType<MysteryShip>();
        bunkers = FindObjectsOfType<Bunker>();

        NewGame();
    }

    private void Update()
    {
        // Aguarda o pressionar de Enter para reiniciar o jogo após o Game Over.
        // Só funciona quando não há mais vidas restantes.
        if (lives <= 0 && Input.GetKeyDown(KeyCode.Return)) {
            NewGame();
        }
    }

    // Reinicia completamente o jogo: oculta o Game Over, zera pontuação/vidas e inicia um round.
    private void NewGame()
    {
        gameOverUI.SetActive(false);

        SetScore(0);
        SetLives(3);
        NewRound();
    }

    // Prepara um novo round: reativa os invasores, restaura os bunkers e reposiciona o jogador.
    private void NewRound()
    {
        invaders.ResetInvaders();
        invaders.gameObject.SetActive(true);

        for (int i = 0; i < bunkers.Length; i++) {
            bunkers[i].ResetBunker();
        }

        Respawn();
    }

    // Reposiciona o jogador no centro da tela (x = 0) e o reativa.
    private void Respawn()
    {
        Vector3 position = player.transform.position;
        position.x = 0f;
        player.transform.position = position;
        player.gameObject.SetActive(true);
    }

    // Exibe a tela de Game Over e desativa todos os invasores.
    private void GameOver()
    {
        gameOverUI.SetActive(true);
        invaders.gameObject.SetActive(false);
    }

    // Atualiza a pontuação e formata o texto com zeros à esquerda (ex: 0042).
    private void SetScore(int score)
    {
        this.score = score;
        scoreText.text = score.ToString().PadLeft(4, '0');
    }

    // Atualiza as vidas garantindo que nunca fique negativo (mínimo 0).
    private void SetLives(int lives)
    {
        this.lives = Mathf.Max(lives, 0);
        livesText.text = this.lives.ToString();
    }

    // Chamado quando o jogador é destruído por um míssil ou invasor.
    // Decrementa uma vida; se ainda houver vidas, reinicia o round após 1 segundo;
    // caso contrário, encerra o jogo.
    public void OnPlayerKilled(Player player)
    {
        SetLives(lives - 1);

        player.gameObject.SetActive(false);

        if (lives > 0) {
            Invoke(nameof(NewRound), 1f);
        } else {
            GameOver();
        }
    }

    // Chamado quando um invasor é abatido pelo laser do jogador.
    // Desativa o invasor, soma sua pontuação e verifica se todos foram eliminados
    // para iniciar um novo round.
    public void OnInvaderKilled(Invader invader)
    {
        invader.gameObject.SetActive(false);

        SetScore(score + invader.score);

        if (invaders.GetAliveCount() == 0) {
            NewRound();
        }
    }

    // Chamado quando a nave misteriosa é abatida; adiciona sua pontuação ao total.
    public void OnMysteryShipKilled(MysteryShip mysteryShip)
    {
        SetScore(score + mysteryShip.score);
    }

    // Chamado quando algum invasor alcança a borda lateral da tela.
    // Desativa os invasores e trata como morte do jogador (o invasor alcançou a base).
    public void OnBoundaryReached()
    {
        if (invaders.gameObject.activeSelf)
        {
            invaders.gameObject.SetActive(false);
            OnPlayerKilled(player);
        }
    }

}
