# Changelog

## 2026-11-07
- Mercado financeiro, M3 passo P5: dá para comprar e vender na tela do ativo. Compra por valor em $, venda por quantidade, botões 25/50/75/100%, valor do ativo, corretagem, imposto estimado e total antes de confirmar, aviso verde de sucesso, card "Sua posição" (quantidade, preço médio, total investido, variação). Aceita vírgula ou ponto. Corrigidos: compra 100% em cripto falhando por 1 centavo e venda 100% deixando sobra mínima (DOGE/PEPE).
- Mercado financeiro, M3 passo P4: tela Finanças mostra o Mercado: "Lucro bruto em investimentos" nas Entradas, grupo "Investimentos" (prejuízo, corretagem, imposto) nas Saídas, transferências em cinza no detalhe da semana do extrato, cartão "Total investido" com o valor da carteira e fatia roxa na barra de patrimônio.
- Mercado financeiro, M3 passo P3 (sem tela nova): compras/vendas registradas na Finanças (compra e principal = transferência; lucro em Entradas; prejuízo, corretagem e imposto em Saídas, fora de "Outros"); patrimônio soma a carteira; imposto semanal do canal ignora o lucro do Mercado; valor da carteira gravado por semana.
- Mercado financeiro, M3 passo P2 (sem tela): regras de compra/venda em `Game/Market/Trading.cs` (corretagem $2 / 0,5% cripto / 0 renda fixa, mínimo $10, fração 8 casas, imposto 15% só no lucro na venda, CDB Banco Médio bloqueado, proteção contra ordem repetida). Autoteste com `trading_debug.txt` ao lado do exe gera `trading_selftest.txt`.
- Mercado financeiro, M3 passo P1 (sem tela): dados da carteira (`Holding`) e do histórico de ordens (`MarketTrade`, até 200) dentro de `Player.Market`; save antigo e "Novo jogo" começam com carteira vazia.

## 2026-11-06
- Mercado financeiro, etapa M2 (só olhar): item "Mercado financeiro" no menu (abaixo de Finanças; Top TubeStars sempre o último), abas Mercado e Carteira (vazia até a M3). Mercado: faixa de cotações (BTC, ETH, PETR4, AAPL, SPY, BOVA11) com mini-gráfico e "7d", maiores altas/baixas, 8 setores que mais mexeram, cartão de notícias "em breve" + Dica da semana, tabela por abas com bolinha colorida, selo "Arriscada", estrela de favorito e "Negociar". Tela do ativo com favoritos, gráfico 7d/28d/112d/Tudo e Comprar/Vender desativado. Favoritos no save. Setores e nomes de fundos/renda fixa em português. Finanças ficou só com Visão geral e Colecionáveis.
- Mercado financeiro, etapa M1 (sem tela): motor de preços em `Game/Market/` com 29 ativos (ações BR/EUA, cripto, fundos, renda fixa). Preço log-normal com tendência, humor do mercado, cauda rara, puxada de volta e limites; ações/fundos só em dias úteis (5 de 7), cripto todo dia, renda fixa com juros diários. Histórico de 112 dias + semanal no save (`Player.Market`); save antigo ganha mercado novo. Modo de teste: `market_debug.txt` ao lado do exe gera `market_sim.csv` (2 anos).
- Save seguro (pacote 1 da revisão): grava em `.tmp` e troca mantendo `save.tss.bak`; se o save estiver ilegível carrega o `.bak` com aviso; se os dois falharem, renomeia para `save_corrompido_<data>.tss` e fica na tela inicial (nunca abre jogo novo por engano). Ao carregar, edições, gravações e estudos em andamento voltam a apontar para o vídeo/estudo real; canal "fantasma" no Postar vídeo corrigido.
- Ajustes visuais: todas as telas com a mesma largura (máx. 1400, centralizadas); título só na página (barra de cima mostra só data e anúncios); botões desativados em cinza; painel do canal sem o gráfico de inscritos por dia, números em 2 colunas; Criar vídeo cabe em 1080p sem rolar; Finanças com eixo do gráfico sem rótulos repetidos e extrato com cabeçalho e "Semana N".
- Escala: o jogo se ajusta a notebook e monitor grande (painel do canal e menu com rolagem, colunas de vídeos variáveis, janela mínima 900×560, diálogos com altura da janela).
- Polimento visual: ícones Phosphor (MIT) em tamanho uniforme, texto em tons de cinza mais firme, barras de rolagem escuras, botões vermelhos sem halo escuro nos cantos.
- Etapa 6e: tela Finanças (Visão geral; Investimentos, Mercado e Colecionáveis "em breve"): dinheiro, resultado da semana, lucro acumulado, gráfico do patrimônio (dinheiro − dívida fiscal − empréstimo), entradas por canal, saídas agrupadas, extrato das últimas 8 semanas. Loja, estudos e advogado passam a contar nos resumos; empréstimo recebido não conta como entrada.

## 2026-11-05
- Meta de monetização: canal só recebe de anúncios depois de 5 mil inscritos + 160 mil views, pedido no painel e análise de 3 a 7 dias. Bloco "Monetização" com barras, cadeado cinza (âmbar em análise) e check verde ao aprovar; Crowdfunding funciona antes.
- Etapa 6d: imposto semanal progressivo sobre o lucro do canal (até $500 isento, 15% até $5 mil, 25% acima), cobrado ao fechar a semana; sem dinheiro vira dívida fiscal cobrada na semana seguinte; Advogado Fiscal ($50 mil) tira 5 pontos.
- RPM −$5 em todos os nichos (Games $20, Finanças $55, Infantil $10), bônus de views dos nichos baratos mantido. Promover vídeo entrega 60–130% do anunciado, puxado pela nota; a tela mostra "cerca de X views".
- Etapa 6c: loja mais cara. Câmera e Edição com 5 níveis ($600 a $50.000; +4/+10/+16/+22/+30), vale só o maior nível e a compra segue a ordem; Consultor $10 mil; Advogado $6 mil.
- Troféus desligados (`TrophyManager.Enabled = false`); código mantido para religar se quiser.
- Etapa 6b (parte 2): "Gasto" do resumo detalhado (custo de vida, vídeos, promoção de vídeo, empréstimo); aviso de custo de vida mostra o valor ("subiu $20/dia, agora $100/dia"); evento de promoção do emprego saiu do sorteio.
- Etapa 6b (parte 1): meio período (4h por $50) com botão no cartão do Emprego, troca 1×/semana valendo no dia seguinte; promoção do emprego removida (integral sempre $100); custo de vida base $80/dia; sair do emprego avisa que é definitivo; troféu GiveAndTake = demitido em período integral.
- Etapa 6a: o canal paga por RPM do nicho (Games ~$25/mil views, Finanças $60, Infantil $15), com época do ano, variação semanal, anúncios, qualidade e inscritos (`Game/RpmCalculator.cs`). Era ~$43/mil e subia sem limite.
- Nichos de RPM baixo ganham mais views (Infantil +20%; Humor/Música/React +8%), sem inscritos a mais. Views promovidas não pagam anúncio.
- "Estratégia de divulgação" virou "Anúncios nos vídeos", com tabela no "?". RPM no painel do canal, em Ver vídeo e em Criar canal; temporada de anúncios no Calendário e etiqueta "Anúncios em alta/fracos" ao lado da data.

## 2026-11-04
- Criar vídeo: botão "×" na lista "Continuar série" tira a série da lista (os vídeos continuam); ela volta se um episódio novo for criado.
- Miniaturas: buscas na API ficam guardadas (Saves\search_cache.json) e o mesmo título não gasta cota de novo; contador "Buscas hoje: x/100" e aviso certo quando o limite diário do YouTube acaba.
- Vermelho de destaque mais suave (#E53935, era #FF0000) e texto no modo "Ideal" do WPF, sem letras serrilhadas.

## 2026-11-03
- Criação de vídeo, etapa 5d: "Comprar views" virou "Promover vídeo" com campanhas Pequena $500 (~3 mil views), Média $1.000 (~8 mil) e Grande $2.000 (~18 mil), entregues em 3 dias. Antes de postar, mostra a estimativa de views e inscritos (depende da nota e do nicho). Views promovidas não pagam anúncio.

## 2026-11-02
- Criação de vídeo, etapa 5c: horas de gravar/editar agora são fixas pela habilidade (iniciante 4h + 6h, até 1h + 2h no nível mestre); saíram os chips de horas e o slider de edição. "Custo" virou "Investimento na produção" em 4 níveis (Nenhum $0, Básico $100 até +8 de nota, Bom $300 até +15, Premium $800 até +25).
- Criação de vídeo, etapa 5a: os atributos saíram e entrou o "Tipo de vídeo" (Tutorial, Review, React, Desafio, Vlog, Top 10, Documentário, Jogatina), obrigatório. Tipo que combina com o nicho dá +10% de views, com selo "Combina com…". O tipo aparece em Postar e Ver vídeo; vídeos antigos mantêm seus atributos. Cursos "Análise de Público I e II" escondidos.
- Canais e nichos, etapa 4b: seletor de canal no menu lateral (com "Visão geral" e "+ Novo canal"), sincronizado com os chips de Meus vídeos; cabeçalho mostra os inscritos do canal escolhido ou a soma. Painel do canal com nicho, RPM, posição no ranking e gráfico de inscritos por dia; Visão geral lista cada canal e o total.
- Canais e nichos, etapa 4c: filtro por nicho no Top TubeStars (Todos os nichos + 15), combinável com Geral e Cresceu na semana. Rivais famosos e genéricos com nicho (rankings antigos migrados), mensagem para filtro vazio.
- Canais e nichos, etapa 4a: 15 nichos substituem as 18 categorias (vídeo e canal). Nicho do canal escolhido na criação e fixo, com ganho por view (RPM ×0,6 a ×1,8) e concorrência (views de busca ×0,8 a ×1,15) próprios. Vídeo fora do nicho: 25% menos inscritos assistem, mas 30% mais chance de inscrição; aviso ao postar. Troféus migrados para os nichos equivalentes.
- Resumos mostram a origem do ganho (Salário, Canal, Outros). "Ver vídeo" mostra os inscritos que o vídeo trouxe. Abas de Ver vídeo com o mesmo tamanho.
- Top TubeStars, etapa 2b: clicar num canal abre o perfil (banner, avatar, tema, posição, inscritos, views, vídeos, inscritos na semana, variação desde ontem, dias no #1, melhor posição e distância para você). Números curtos vão até trilhão (1,25 tri) e o truncamento não erra mais por arredondamento.
- Top TubeStars, etapa 2a (ranking vivo): 15 rivais famosos + 500 canais genéricos só com números (offline, sem vídeos; save mais leve), crescimento diário com volatilidade e virais, ▲▼ desde ontem, destaques da semana (maior alta, maior queda, você), abas Geral e Cresceu na semana, "Ir para minha posição", "Voltar ao #1" e paginação de 50. Renomear canal mantém o histórico no ranking.
- Números curtos no jogo todo (12,4 mil, 1,19 mi, 2,3 bi) para inscritos, views, likes e comentários; dinheiro continua exato.
- Cartão de Meus vídeos mostra likes, dislikes, comentários e dias no ar (views saíram da miniatura). "Ver vídeo" virou análise com abas Desempenho (views, posição no canal, aprovação, gravação e edição) e Dinheiro (custo, quanto rendeu e lucro). Cada vídeo passa a guardar quanto rendeu e gastou com views compradas.
- Calendário (item 3): cartão "Resumo de ontem" na Agenda, resumo automático ao fechar cada semana (dia 8, 15...) e cada ano (112 dias), com totais e tabela por canal (views, novos inscritos, melhor vídeo). Clicar na data do cabeçalho abre o Calendário: dias da semana, "Esta semana até agora" e histórico de resumos. Eventos aleatórios entram nos totais.
- "Postar vídeo" saiu do menu lateral: posta-se pelo botão "Postar" em Meus vídeos (com botão de voltar).
- "Ver vídeo" não abre mais em branco (o player antigo usava Flash): mostra uma ficha do vídeo.
- Botões padronizados com cantos de 8px (chips, filtros, Loja); etiqueta da nota com 4px.

## 2026-11-01
- Removidos da tela de título os botões Tutorial e Créditos (e a janela de créditos) e os nomes dos tradutores; removidos a opção "Creative Commons" do Novo jogo e o "?" do cabeçalho (vídeo antigo). No Top TubeStars, clicar num rival não abre mais a lista de vídeos (o perfil entra no item 2 do roadmap).

## 2026-10-31
- Visual novo, etapa 4e: todas as janelas no estilo novo (o estilo escuro nunca era aplicado às janelas; corrigido em `DialogHelper`), Adicionar tarefa, Novo jogo e Estudos em cartões, ComboBox escuro, gráficos escuros, mensagens com OK vermelho e tela de título como a prévia. Conclui o Visual novo (V1 Studio).
- Visual novo, etapa 4d: "Postar vídeo" virou tela do menu (sai a janela UploadVideoDialog): lista compacta dos vídeos prontos, canal em chips, opções de views atuais em "Promover vídeo" (mesmos valores e regras), "Trocar miniatura" com busca só ao clicar. O botão "Postar" de Meus vídeos abre essa tela.
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
