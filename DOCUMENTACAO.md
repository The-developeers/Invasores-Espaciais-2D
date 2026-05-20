# Documentação — Invasores Espaciais 2D

Guia para o grupo entender e explicar o código e as funcionalidades do jogo.

---

## Visão Geral

O jogo é uma recriação do clássico **Space Invaders** feita em Unity (C#). É composto por **7 scripts**, cada um com uma responsabilidade clara. O fluxo central é coordenado pelo `GameManager`, enquanto os demais scripts controlam partes específicas do jogo.

---

## Scripts e Funcionalidades

### 1. `GameManager.cs` — Controlador Central

> **O que é:** O "cérebro" do jogo. Controla o fluxo geral: início de partida, vidas, pontuação e condições de vitória/derrota.

**Padrão usado:** Singleton — garante que exista apenas uma instância ativa do `GameManager` na cena. Qualquer script pode acessá-lo via `GameManager.Instance`.

**Variáveis importantes:**
| Variável | Valor inicial | Descrição |
|---|---|---|
| `score` | 0 | Pontuação atual |
| `lives` | 3 | Vidas restantes |

**Métodos e o que fazem:**

- `NewGame()` — Reseta score para 0 e vidas para 3, depois chama `NewRound()`.
- `NewRound()` — Reativa todos os invasores, restaura os bunkers e reposiciona o jogador no centro.
- `Respawn()` — Coloca o jogador de volta na posição inicial (centro inferior da tela).
- `GameOver()` — Exibe a tela de game over e desativa os invasores.
- `SetScore(int pts)` — Soma pontos e atualiza o texto na tela com formatação de 4 dígitos (ex: `0050`).
- `SetLives(int lives)` — Atualiza o contador de vidas (mínimo 0).
- `OnPlayerKilled()` — Subtrai uma vida. Se ainda há vidas, respawn após 1 segundo. Se não, `GameOver()`.
- `OnInvaderKilled(Invader invader)` — Desativa o invasor morto e soma os pontos dele. Se não restam invasores, inicia nova rodada.
- `OnMysteryShipKilled()` — Soma 300 pontos ao score.
- `OnBoundaryReached()` — Chamado quando um invasor alcança o limite inferior da tela; mata o jogador.

---

### 2. `Player.cs` — Nave do Jogador

> **O que é:** Controla a nave que o jogador pilota na base da tela.

**Variáveis importantes:**
| Variável | Valor | Descrição |
|---|---|---|
| `speed` | 5 | Velocidade de movimento lateral |
| `laserPrefab` | — | Prefab do projétil disparado |
| `laser` | — | Referência ao laser ativo no momento |

**Como funciona:**

- A cada frame (`Update()`), lê o input do jogador:
  - **A / Seta esquerda** → move para a esquerda
  - **D / Seta direita** → move para a direita
  - O movimento é **limitado pelas bordas da câmera** para o jogador não sair da tela
- **Espaço ou clique do mouse** → dispara um laser
  - Só pode existir **1 laser ativo por vez**; se o laser anterior ainda está na tela, não pode disparar
- `OnTriggerEnter2D()` — Se um míssil inimigo ou invasor colidir com o jogador, notifica o `GameManager`

---

### 3. `Invaders.cs` — Grade de Invasores

> **O que é:** Controla todos os 55 invasores como uma única entidade que se move em bloco.

**Variáveis importantes:**
| Variável | Valor | Descrição |
|---|---|---|
| `rows` | 5 | Número de linhas da grade |
| `columns` | 11 | Número de colunas da grade |
| `direction` | 1 | Direção atual: 1 = direita, -1 = esquerda |
| `speed` | AnimationCurve | Curva que aumenta velocidade conforme invasores morrem |
| `missileSpawnRate` | — | Taxa de disparo dos mísseis inimigos |

**Como funciona:**

- `CreateInvaderGrid()` — Instancia 55 invasores em um grid 5×11, centralizado na tela, com espaçamento de 2 unidades entre cada um. Cada linha usa um prefab diferente (tipos distintos de invasores).
- `Update()` — A cada frame:
  1. Move toda a grade horizontalmente na `direction` atual, com velocidade baseada em quantos invasores ainda estão vivos
  2. Verifica se algum invasor ultrapassou a borda da tela
  3. Se sim, chama `AdvanceRow()`
- `AdvanceRow()` — Inverte a direção horizontal e move toda a grade **1 unidade para baixo**.
- `MissileAttack()` — Executado em loop repetido: seleciona aleatoriamente um invasor ativo e faz ele disparar um míssil. Quanto menos invasores restam, maior a chance de disparo por ciclo.
- `ResetInvaders()` — Reativa todos os invasores e retorna a grade à posição e direção iniciais.
- `GetAliveCount()` — Retorna quantos invasores ainda estão ativos.

**Comportamento clássico do Space Invaders:**
A velocidade da grade aumenta conforme invasores são eliminados — é o que cria a tensão crescente do jogo.

---

### 4. `Invader.cs` — Invasor Individual

> **O que é:** Script atribuído a cada um dos 55 invasores individualmente.

**Variáveis importantes:**
| Variável | Valor padrão | Descrição |
|---|---|---|
| `animationSprites[]` | — | Array com 2 sprites para animação |
| `animationTime` | 1s | Intervalo entre troca de frames |
| `score` | — | Pontos que vale quando morto |

**Como funciona:**

- `Start()` — Inicia uma rotina (`InvokeRepeating`) que troca o sprite a cada `animationTime` segundos, criando o efeito visual de movimento clássico dos invasores.
- `AnimateSprite()` — Alterna entre os sprites do array. Quando chega no último, volta ao primeiro.
- `OnTriggerEnter2D()` — Detecta dois tipos de colisão:
  - **Laser do jogador** → chama `GameManager.Instance.OnInvaderKilled(this)`
  - **Boundary (limite de tela)** → chama `GameManager.Instance.OnBoundaryReached()`

---

### 5. `MysteryShip.cs` — Nave Bônus

> **O que é:** Nave especial que aparece periodicamente no topo da tela, valendo pontos extras.

**Variáveis importantes:**
| Variável | Valor | Descrição |
|---|---|---|
| `speed` | 5 | Velocidade de travessia |
| `cycleTime` | 30s | Intervalo entre cada aparição |
| `score` | 300 | Pontos ao ser destruída |
| `spawned` | false | Flag: se a nave está ativa |

**Como funciona:**

- `Start()` — Calcula as posições de spawn (fora da tela, esquerda e direita) e agenda a primeira aparição.
- `Spawn()` — Ativa a nave em uma das bordas e define a direção de travessia (alterna a cada aparição).
- `Update()` — Se `spawned` é `true`, move a nave na direção configurada.
- `MoveRight()` / `MoveLeft()` — Move e verifica se saiu da tela; se sim, chama `Despawn()`.
- `Despawn()` — Desativa a nave e agenda a próxima aparição em `cycleTime` segundos.
- `OnTriggerEnter2D()` — Se atingida pelo laser: `Despawn()` + `GameManager.Instance.OnMysteryShipKilled()`.

---

### 6. `Bunker.cs` — Estruturas de Defesa

> **O que é:** As 4 estruturas entre o jogador e os invasores que absorvem dano progressivamente.

**Variáveis importantes:**
| Variável | Descrição |
|---|---|
| `splat` | Textura de explosão (define o formato do dano) |
| `originalTexture` | Cópia da textura original para restauração |
| `spriteRenderer` | Componente visual do bunker |
| `boxCollider` | Collider para detectar colisões |

**Como funciona:**

- `CopyTexture()` — Cria uma cópia independente da textura para cada bunker, garantindo que o dano em um não afeta os outros.
- `ResetBunker()` — Restaura o bunker ao estado inicial aplicando a `originalTexture`.
- `CheckCollision(Collider2D coll)` — Verifica múltiplos pontos do objeto colidindo contra o bunker para precisão aprimorada. Chama `Splat()` nos pontos de impacto.
- `Splat(Vector3 worldPos)` — Converte a posição do impacto em coordenadas de pixel e apaga pixels da textura usando a `splat` como máscara — criando o efeito visual de destruição progressiva.
- `CheckPoint(Vector3 worldPos)` — Converte coordenadas do mundo (Unity) para coordenadas de pixel na textura.
- `OnTriggerEnter2D()` — Se um invasor (não um projétil) toca no bunker, ele é **destruído instantaneamente** (desativado).

**Por que isso é especial:** A destruição é pixel-perfect — o buraco no bunker tem exatamente o formato da textura `splat`, assim como no jogo original.

---

### 7. `Projectile.cs` — Projéteis

> **O que é:** Script genérico usado tanto pelo **laser do jogador** quanto pelos **mísseis dos invasores**.

**Variáveis importantes:**
| Variável | Descrição |
|---|---|
| `direction` | `Vector3.up` para laser, `Vector3.down` para míssil |
| `speed` | 20 — velocidade de viagem |

**Como funciona:**

- `Update()` — Move o projétil continuamente na `direction` × `speed` a cada frame.
- `OnTriggerEnter2D()` / `OnTriggerStay2D()` — Detecta colisão:
  - Se colidiu com um **Bunker**: chama `bunker.CheckCollision()` para aplicar dano pixel-perfect; depois se autodestrói.
  - Se colidiu com **qualquer outro objeto**: se autodestrói (o outro objeto cuida do próprio dano).

**Por que um script serve para dois tipos:** A única diferença entre laser e míssil é a `direction` — para cima ou para baixo. O mesmo script reutilizável elimina duplicação de código.

---

## Fluxo Completo do Jogo

```
Início da partida
  └─ GameManager.NewGame()
       ├─ score = 0
       ├─ lives = 3
       └─ NewRound()
            ├─ Invasores resetados e ativos
            ├─ Bunkers restaurados
            └─ Jogador reposicionado no centro

Durante o jogo
  ├─ Jogador move lateralmente e dispara laser para cima
  ├─ Grade de invasores move lado-a-lado, desce ao bater na borda
  ├─ Invasores disparam mísseis aleatoriamente para baixo
  ├─ MysteryShip cruza o topo a cada 30 segundos
  └─ Bunkers absorvem projéteis progressivamente

Fim de rodada
  ├─ Todos os invasores mortos → NewRound() (nova leva de invasores)
  └─ Invasor alcança o limite inferior → OnBoundaryReached() → jogador morre

Fim de jogo
  ├─ Jogador perde as 3 vidas → GameOver()
  └─ GameOver() exibe tela e desativa invasores
```

---

## Conceitos de Programação Utilizados

| Conceito | Onde aparece |
|---|---|
| **Singleton** | `GameManager` — instância global única acessível por todos |
| **Prefabs e Instanciação** | `Invaders` cria 55 objetos dinamicamente; `Player` instancia lasers |
| **Delegates / Callbacks** | Invasores notificam o `GameManager` via métodos públicos |
| **Physics 2D (Triggers)** | Todas as colisões usam `OnTriggerEnter2D` do Unity |
| **Manipulação de Textura** | `Bunker` edita pixels diretamente via `Texture2D.GetPixels/SetPixels` |
| **AnimationCurve** | `Invaders` usa curva para aumentar velocidade conforme invasores morrem |
| **InvokeRepeating** | Animação dos invasores e ataques de míssil usam repetição temporizada |
| **Reutilização de Script** | `Projectile` serve para laser e míssil mudando apenas a `direction` |

---

## Dicas para a Apresentação

- **Comece pelo `GameManager`** — ele conecta tudo. Mostre o fluxo `NewGame → NewRound → GameOver`.
- **Destaque o sistema de bunkers** — é a parte mais técnica do código (manipulação de pixels).
- **Explique o `AnimationCurve` no `Invaders`** — mostra como a dificuldade aumenta dinamicamente.
- **Mostre que `Projectile` é reutilizável** — um único script para dois comportamentos diferentes é um bom exemplo de design.
- **Fale sobre o padrão Singleton** — é um padrão de projeto clássico e o professor provavelmente vai perguntar sobre ele.
