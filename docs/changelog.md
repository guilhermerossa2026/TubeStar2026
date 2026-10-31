# Changelog

## 2026-10-31
- Visual novo, etapa 4c: "Criar vídeo" virou tela do menu (sai a janela AddVideoDialog/AddVideoDialog2), com horas e custo em chips, resumo à direita e carrossel de miniaturas: busca só pelo título (categoria como fallback), sem sorteio, contador "N de 50", próxima página por pageToken, imagem baixada só quando exibida, "Usar esta" e "Buscar outras".
- Visual novo, etapa 4b: Meus vídeos com chips de canal e "+ Novo canal", filtros (Mais recentes, Mais vistos, Melhor nota), busca, grade 3×3 com paginação e "Ver todos", nota 0–10 no cartão, botão Excluir (sai a lixeira de arrastar) e painel do canal com KPIs. `VideoBlock` e `ChannelBlock` removidos.
- Visual novo, etapa 4a: Agenda com barra "Horas do dia (15h)" colorida por tipo e cartões de tarefa com progresso e botões +1h, −1h e Remover (regras iguais). Componentes antigos `TodoTask` e `AppointmentBlock` removidos.
- Visual novo, etapa 3: estilos de cartão reutilizáveis (`Styles/Components.xaml`), cartões de vídeo em grade e lista, Loja em grade (itens comprados ficam no fim com "Comprado"; comprar sempre pede confirmação) e Top TubeStars em tabela de ranking (15 rivais + seus canais destacados, abas Twitch/Podcast "Em breve"). Menu lateral passou para 240px.
- "Continuar" volta exatamente ao mesmo dia: o save guarda a agenda do dia e o dinheiro do início do dia (antes, carregar o jogo encerrava o dia e simulava um dia novo). Salvar não tira mais as horas já marcadas.
- Visual novo, etapa 2: menu lateral (Agenda, Meus vídeos, Loja e Top TubeStars; demais itens "Em breve"), cabeçalho com data "Ano · Semana · Dia" (ano de 112 dias), dinheiro, inscritos e botão "Próximo dia", ícones vetoriais em `Styles/Icons.xaml`. A faixa inferior do "Novo dia" saiu.
- Visual novo, etapa 1 (cores e fonte): paleta da prévia V1 Studio centralizada em `Styles/Custom.xaml` (novas cores de ganho, perda e Twitch), ~100 cores fixas trocadas pelo tema, gráficos com tema escuro e fonte Inter embutida (licença SIL OFL, em `TubeStarDesktop/Fonts`).
- "Novo jogo" na mesma sessão não herda mais os vídeos não lançados do jogo anterior; save corrompido (sem canais) é recusado e o jogo volta a um estado novo coerente.
- Corrigido o carregamento de saves JSON: o jogo apagava o canal "não lançados" e tratava o primeiro canal real como ele (vídeos publicados podiam sumir). Saves sem listas de shares/views compradas também carregam.
- Save mais enxuto: compartilhamentos e views compradas só são gravados para vídeos que ainda existem e ainda têm algo a entregar.
- Game over no meio do dia agora encerra o dia na hora (antes o dia seguia por trás e podia abrir evento sobre a tela de título).
- Tela de vídeos mais leve: miniatura de cada vídeo fica pronta em memória (sem refazer a cada atualização), cartões param de vazar memória e o fim do dia com evento não atualiza as telas em dobro.
- O dia agora é simulado na thread da tela (sem BackgroundWorker): acaba a disputa com os rivais e com o gerador aleatório, erros vão para o Crash Report sem travar a tela, e os vídeos novos dos rivais entram antes da simulação.
- Simulação diária muito mais rápida (~11× a 40×): views processadas em até ~50 lotes por vídeo, com sorteio de "quantos" curtem/se inscrevem/compartilham em cada lote; médias e variação iguais às do jogo antigo (comparação numérica do testador).

## 2026-10-30
- Comentários viraram só um número (likes ÷ 20 + dislikes ÷ 10): saíram as frases geradas, a lista na janela do vídeo e a busca de comentários na API do YouTube.
- Desinscrições e bônus de renda por inscritos deixaram de usar divisão inteira: agora há perda real de inscritos (com piso de zero) e o bônus cresce de forma suave.
- Opções de views "por view" ao postar agora conferem o dinheiro antes, como os pacotes fixos (evitava game over por compra barata).
- Custo "por view" agora é cobrado só nas views compradas (antes, em todas as views do vídeo para sempre); views extras do SecondTime viraram bônus sem custo.
- Tela de postar mais robusta: resposta inválida da internet não fecha o jogo, falha na busca mostra a imagem "sem internet", cliques rápidos nas setas mantêm a última imagem e o handler da imagem é removido ao fechar.
- Corrigido o travamento ao postar quando a busca de miniaturas caía no fallback por categoria (`UploadVideoDialog.xaml.cs`).
- Projeto colocado no git e publicado em github.com/guilhermerossa2026/TubeStar2026.
- Chave da YouTube API saiu do código: agora é lida de `youtube_api_key.txt` (ao lado do exe) ou da variável `YOUTUBE_API_KEY`.
- `.gitignore` atualizado (bin/, obj/, chave da API).
- Criados `CLAUDE.md`, `docs/visao-geral.md` (diagnóstico), os subagentes (planejador, tech-leader, programador, designer, testador) e as skills `economia-de-tokens` e `documentar`.

## 2026-10-26 (Fase A, antes do git)
- Migração para .NET Framework 4.8, TLS 1.2, save em JSON compatível com o XML antigo, rivais offline, tema escuro estilo YouTube e GameJolt desativado.
