# Onde paramos

*Atualizado em 05/11/2026. Use este arquivo para retomar o trabalho em outra máquina ou sessão.*

## Decisões gerais
- Todo o plano está em [roadmap.md](roadmap.md). Cada ideia é discutida a fundo, uma por vez, e a versão final é apresentada ao dono antes de qualquer código.
- Visual escolhido: **V1 Studio** (caminho B, redesenho 2D em WPF). A prévia navegável está em [estudo-visual.html](estudo-visual.html) e em https://claude.ai/artifact/RrZkRcY3Y4yTVEqJ2Dz1At (link privado do dono).
- Ordem: primeiro **"Consertar a base"**, depois o visual novo, depois as mecânicas novas.

## Etapa atual: "Consertar a base"
O plano completo em 10 passos (feito pelo tech-leader) está no roadmap. Situação:

| Passo | O que faz | Estado |
|---|---|---|
| 1 | Apagar código morto dos rivais (`DoAddNewVideo`) e o pop-up do jogo mobile | ✅ feito |
| extra | Remover GameJolt (troféus agora locais em `Game/Trophies`), mods/ModLauncher e sobras | ✅ feito |
| extra | Remover a dinâmica dos robôs (evento, tarefas, rebelião, fim de jogo) | ✅ feito |
| 2 | Corrigir o travamento ao postar (fallback em `UI/UploadVideoDialog.xaml.cs:87`, usa `response` em vez de `r`) | ✅ feito |
| 2b | Tela de postar: tratar erro/resposta inválida da busca, aviso sem internet/chave, cliques rápidos nas setas, remover handler `OnImageFetched` | ✅ feito |
| 3 | Bugs de cobrança em `VideoViewer.cs` (SecondTime usa `numViews` na linha 145; CostPerView cobrado em todas as views, linha 138) | ✅ feito |
| 3b | Opções de views "por view" conferem o dinheiro antes de comprar (evita game over por compra barata) | ✅ feito |
| 4 | Ramo de 50% que nunca acontece em `CategoryHelpers.cs:83` | ❌ não era bug: `RandomVideoCategory` sorteia de novo a cada leitura, então o 2º `if` é uma segunda chance real |
| 5 | Desinscrição e bônus de inscritos com divisão inteira (`VideoViewer.cs:224` e `:305`) | ✅ feito |
| novo | `RandomHelpers.Chance(p)` dá 1 ponto a mais (5% vira 6%); afeta o jogo todo | a discutir |
| 6 | Trocar a simulação view a view por cálculo direto (`VideoViewer.cs`, risco alto) | ✅ feito (lotes + sorteio binomial, ~11× a 40× mais rápido) |
| 7 | Sincronizar Money/Subscribers, que hoje são alterados pelo BackgroundWorker (`DailyPlanner.xaml.cs`) | ✅ feito: simulação passou a rodar na thread da tela |
| 8 | Remover as frases de comentários e deixar só o número (manter `Video.Comments` por causa dos saves) | ✅ feito |
| 9 | A tela de vídeos para de ser recriada inteira a cada dia | ✅ feito (versão enxuta: cache de miniatura, vazamento, sem update em dobro; reconstrução fica para o visual novo) |
| 9b | Game over no meio do dia: `RunIterations` segue rodando depois de `GameExit` (pode abrir evento sobre a tela de título) | ✅ feito |
| 10 | Limpar `_videoShares`/`_videoBoughtViews` de vídeos excluídos no save | ✅ feito |
| 10b | Load de save JSON apagava o canal "não lançados" (`RemoveAt(0)` só vale para XML) | ✅ feito |
| 10c | "Novo jogo" na mesma sessão herda vídeos não lançados; save sem canais deixa o jogo quebrado | ✅ feito |

Decisões do dono para esta etapa:
- Os passos 3, 4 e 5 corrigem os bugs agora, mesmo mudando o equilíbrio do jogo (opção A).
- No passo 6, é aceitável uma variação menor de um dia para o outro.

## Pendências pequenas anotadas
- Passo 6: em vídeos bem pequenos (~100 views/dia), compartilhamentos ficam ~3/dia abaixo do antigo (5,7 × 8,6); renda até 0,4% menor. Investigar como item separado.
- Imagem baixada inválida (`Video.ImageBytes`) gera exceção ao montar a miniatura e fica salva; validar no download.
- Rivais: a lista de vídeos cresce sem limite. No Top TubeStars, decidir entre rivais só com números (A) ou famosos guardando os últimos ~10 vídeos (B, recomendada).
- Saves carregados e salvos de novo com a versão anterior ao passo 10b podem ter perdido o canal "não lançados"; a correção não recupera esses arquivos.
- `rivalDailyIncome` (`DailyPlanner.xaml.cs:298`) é calculado e nunca usado; rever no Top TubeStars.
- Textos de idioma sem uso (robôs, `MissingToken` do GameJolt, frases de comentário em `CommentStrings.cs`) continuam em `EnglishStrings.cs` e nos 20 XML. Para remover, é preciso tirar do enum e de **todos** os XML juntos, senão a leitura do idioma falha em silêncio e o jogo volta para o inglês.
- Os troféus `RobotMasters` e `RebelLeader` e o campo `Player.RobotRulers` ficaram mantidos por compatibilidade com saves antigos.
- `docs/visao-geral.md` está desatualizado: ainda cita GameJolt e mods.

## Como retomar
1. Fazer `git pull`.
2. Ler o `CLAUDE.md` e este arquivo.
3. Situação em 03/11/2026: itens 1 (Visual novo), 2 (Top TubeStars), 3 (Calendário), 4 (vários canais) e 5 (tipo de vídeo, horas e Promover vídeo) estão feitos. Também foram corrigidas as faixas pretas nas miniaturas e o botão Salvar (clique normal e aviso "Jogo salvo").
4. Próximo passo: pedir ao Claude "pode fazer a 6a RPM". O plano técnico aprovado (passos e números) está no `roadmap.md`, item 6. Começar pelos passos 1 a 3 e parar para o dono testar. Também já feitos em 03/11/2026: barra de espaço passa o dia e fecha avisos; valores grandes em formato curto; excluir canal e excluir vídeo na tela Postar; números do cartão numa linha só; ordem de "Mais recentes"; teto de inscritos dos rivais (maior rival fica ~30-90 mi; top 10 pede ~8 mi); "Continuar série" em Criar vídeo (lista de séries por "#número", botão +1, ignora maiúsculas e acentos, puxa nicho e tipo do último episódio); fonte: Poppins testada e recusada (ficou borrada), mantida a Inter com títulos e números maiores e em negrito.
5. Feito em 04/11/2026: vermelho mais suave (#E53935) e texto no modo "Ideal"; memória de buscas de miniatura (`YouTube API/ThumbSearch.cs`, `Saves\search_cache.json`) com contador "Buscas hoje: x/100" e aviso de cota; "×" para tirar série da lista "Continuar série".
6. **Feito em 04/11/2026 (tarde): biblioteca de miniaturas por nicho.**
   - `YouTube API/ThumbLibrary.cs` + `thumb_channels.txt` (canais por nicho, editável; inclui os de Minecraft do dono e GTA/GTA RP/Roblox/ARK/EA FC). Baixa uploads 1×/semana em segundo plano (`Saves\thumb_library.json`, ~600 unidades, fora das 100 buscas). Games baixa até 500 vídeos por canal; os outros nichos, 100.
   - Ordem no carrossel: acerto completo → "parecida" no nicho escolhido → busca pelo título → aleatórias do nicho só sem cota/chave. Etiqueta de origem e botão "Buscar mais".
   - Memória de buscas ignora "#número" (cada série gasta 1 busca). Siglas RP/FC/PC etc. contam como palavra.
   - Barra de progresso na tela inicial ("Miniaturas prontas ✓" fica) e em Criar vídeo.
   - Miniaturas já usadas por vídeos do jogador não aparecem de novo (excluir o vídeo libera).
   - Canais que a API não achou ou achou errado (o dono pode corrigir o @ no txt): AM3NIC, Cadres, Robin Hood Gamer, Skorpion Gamer, Casey Neistat, Ryan Trahan, TPM Receitas, Tasty, Babish, Gusttavo Lima, Marília Mendonça, Bruna Tavares, ge, Ei Nerd, Parafernalha, Like Nastya; errados: Geleia, Rubius, TEK, Mohamad Hindi, Casimiro, xQc.
   - Pasta `../Jogar_TubeStar` apagada; o dono joga pelo `bin\Release`.
   - Depois (04/11/2026): "Buscar outras" também usa a biblioteca antes da API; botão "Buscar mais (API)"; Shorts fora (biblioteca usa a playlist `UULF` do canal + filtro "#short/#shorts" no título, também na busca); etiqueta "Da busca (memória, não gastou)"; título que é só nome de nicho ("Tecnologia #1", "Games #3", "Lifestyle") mostra miniaturas aleatórias daquele nicho sem gastar busca (o nicho do título vence o nicho escolhido).
   - Fim do dia 04/11/2026: biblioteca lê playlists (referência "PL..." ou link com "list=" no `thumb_channels.txt`); 7 playlists de Minecraft do EduKof; canais corrigidos (Authentic Games, Cadres, Robin Hood Gamer, Skorpion); falta o @ do AM3NIC. Acerto e "parecida" mostram até 50 miniaturas embaralhadas.
   - **Feito em 05/11/2026: views mais realistas (3 entregas, commits c2834db, 4be6035, b0cacb1).** Alcance pela nota e sorte do vídeo (flop/normal/bom/viral, `Game/VideoLuck.cs`); empurrão para canais pequenos (80 views/dia nos 3 primeiros dias, some aos 2000 inscritos); impulso do canal (`Channel.Momentum`, 0,7–1,4, sobe com vídeos bons e cai parado); perda suave de inscritos (vídeo fraco até 0,5%, espalhada; canal parado >14 dias perde 0,05%/dia, teto 5%; suspenso não perde; frações acumuladas); avisos no "Resumo de ontem" (viral, flop, indo bem, inscritos perdidos; só vídeos com até 3 dias). Antiga `CheckUnsubscriptions` removida. Pendente pequeno: avisos de vídeo não reaparecem ao reabrir o jogo.
   - **Ajustes depois do teste do dono (commit a4e2f2e):** selo de texto VIRAL (vermelho) / EM ALTA (verde) / FLOPOU (cinza) no cartão (`UI/VideoBlockGrid`), sem emoji (ficaria sem cor); alcance nota 0–2 = 0,5%; empurrão e "cauda de busca" (`qualityViews`) multiplicados por `SearchFloorGradeFactor` (nota 7+ = 1, 5 = 0,6, 3 = 0,25, ≤2 = 0); flop nota <3 = 60%, nota 3–5 = 40%.
   - **Números do teste do dono (canal ~230-530 inscritos):** nota 3,4 → 682 views e nota 4,4 → 1,8k (antes da cauda pela nota); nota 6,9 → 5,4k e subindo (cauda total ~10k em ~2 meses, independe do tamanho do canal). Decisão: **deixar a cauda assim** e rever só depois da 6a, se os inscritos crescerem rápido demais (opções: B = cauda pela metade; C = cauda cheia só em EM ALTA/VIRAL).
   - **Feito em 05/11/2026: etapa 6a (RPM por nicho e época), aprovada pelo dono.** Detalhes no changelog. Rever a cauda de views só se os inscritos crescerem rápido demais.
   - **Próximo: 6b (emprego só paga as contas), testar logo após a 6a.** Plano no `roadmap.md`. Ideias abertas: tirar views promovidas da conta do RPM mostrado; textos novos só em PT/EN.
7. Pendente, adiado pelo dono (medo de estragar a resolução): telas em vários tamanhos. No notebook (1366×768) o painel do canal corta Estatísticas/Editar/Excluir; no monitor grande os cartões esticam (3 colunas fixas). Proposta: cartões de tamanho fixo com colunas variáveis e barras de rolagem; fazer com commit antes e uma tela por vez.
8. Textos novos só precisam de PT-BR (decisão do dono).
