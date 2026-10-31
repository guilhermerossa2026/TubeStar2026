# Onde paramos

*Atualizado em 31/10/2026. Use este arquivo para retomar o trabalho em outra máquina ou sessão.*

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
3. Pedir ao Claude: "continuar o Visual novo, etapa 4 (demais telas)". Etapas 1 (cores e fonte Inter), 2 (menu lateral e cabeçalho) e 3 (cartões, Loja e Top TubeStars em tabela) feitas em 31/10/2026. O "Continuar" agora volta ao mesmo dia (agenda salva). Textos novos só precisam de PT-BR (decisão do dono)..
