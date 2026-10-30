# Onde paramos

*Atualizado em 30/10/2026. Use este arquivo para retomar o trabalho em outra máquina ou sessão.*

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
| 3 | Bugs de cobrança em `VideoViewer.cs` (SecondTime usa `numViews` na linha 145; CostPerView cobrado em todas as views, linha 138) | ⏭️ **próximo** |
| 4 | Ramo de 50% que nunca acontece em `CategoryHelpers.cs:83` | pendente |
| 5 | Desinscrição e bônus de inscritos com divisão inteira (`VideoViewer.cs:224` e `:305`) | pendente |
| 6 | Trocar a simulação view a view por cálculo direto (`VideoViewer.cs`, risco alto) | pendente |
| 7 | Sincronizar Money/Subscribers, que hoje são alterados pelo BackgroundWorker (`DailyPlanner.xaml.cs`) | pendente |
| 8 | Remover as frases de comentários e deixar só o número (manter `Video.Comments` por causa dos saves) | pendente |
| 9 | A tela de vídeos para de ser recriada inteira a cada dia | pendente |
| 10 | Limpar `_videoShares`/`_videoBoughtViews` de vídeos excluídos no save | pendente |

Decisões do dono para esta etapa:
- Os passos 3, 4 e 5 corrigem os bugs agora, mesmo mudando o equilíbrio do jogo (opção A).
- No passo 6, é aceitável uma variação menor de um dia para o outro.

## Pendências pequenas anotadas
- Textos de idioma sem uso (robôs, `MissingToken` do GameJolt) continuam em `EnglishStrings.cs` e nos 20 XML. Para remover, é preciso tirar do enum e de **todos** os XML juntos, senão a leitura do idioma falha em silêncio e o jogo volta para o inglês.
- Os troféus `RobotMasters` e `RebelLeader` e o campo `Player.RobotRulers` ficaram mantidos por compatibilidade com saves antigos.
- `docs/visao-geral.md` está desatualizado: ainda cita GameJolt e mods.

## Como retomar
1. Fazer `git pull`.
2. Ler o `CLAUDE.md` e este arquivo.
3. Pedir ao Claude: "continuar do passo 2 do Consertar a base".
