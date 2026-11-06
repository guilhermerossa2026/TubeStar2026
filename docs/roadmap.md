# Roadmap do TubeStar

> **Regra do dono:** cada ideia é discutida a fundo, **uma por vez**. Antes de qualquer código, a versão final da ideia é apresentada para aprovação.
> Status padrão: **ideia aprovada, falta detalhar**.

Decisões de 30/10/2026. A ordem abaixo é a ordem de discussão.

## Agora (bases)

**0. Consertar a base**: aprovado, vem antes do visual
- Tela de vídeos: a reconstrução completa dos cartões fica para o visual novo (passo 9 fez só cache de miniatura, vazamento e atualização em dobro).
- Bugs: `RandomHelpers.Chance(p)` dá (p+1)% (`Next(100) <= p`), afeta o jogo todo, discutir antes de mudar; `RandomEvents.cs:114` tira inscritos sem piso de zero (seguro hoje, rever nos Eventos 2.0);- Registro (para o item 5): hoje não há ajuste de dificuldade ao criar o canal; o Ultra é só o Challenge com CTR de 10%; o gasto fixo é $50/dia.
- Dificuldade: Média. Depende de: nada.

**1. Visual novo: V1 Studio (WPF/XAML)**: concluído em 31/10/2026 (etapas 1 a 4). Ficaram para itens futuros: seletor de canal e Algoritmo no cabeçalho (itens 4 e 8), RPM e "Que mais rendem" (item 6), "Promover vídeo" (item 5), telas Principal, Universidade e Funcionários ("Em breve").
- Prévia: `docs/estudo-visual.html`. Etapas: (1) cores e fontes em `Styles/` (feita em 31/10/2026); (2) cabeçalho, menu lateral e relógio Ano/Semana/Dia (feita em 31/10/2026; seletor de canal e Algoritmo ficam para os itens 4 e 8); (3) cards novos e Top TubeStars (feita em 31/10/2026: só visual, com os 15 rivais atuais; mecânica completa no item 2); (4) demais telas.
- Ajustes: tela título sem o botão Top TubeStars; Meus vídeos com seletor de canal, "+ Novo canal", grade de 9 com paginação, filtros, busca, painel direito mais estreito, ver todos e excluir; botão de thumbnail em Criar vídeo.
- Thumbnail (Criar vídeo): busca na API só pelo TÍTULO; categoria só como fallback sem resultado (hoje "categoria,nome" + sorteio de 1 em 50 em `Video.FetchRandomImage`, `Video.cs:549`). Sem sorteio, ordem de relevância. Carrossel com setas (direita = próxima, esquerda = anterior exata) e contador "N de 50". Os 50 resultados ficam guardados; ao passar do fim, busca a próxima página (pageToken, +1 busca). Cada imagem é baixada só quando exibida. Botões "Usar esta" e "Buscar outras" (troca o termo).
- Postar vídeo: lista compacta dos vídeos prontos à esquerda (uma linha por vídeo); o selecionado aparece à direita com o canal e o "Promover vídeo".
- Cuidado: modais e telas sem caixas vazando (visto na prévia de "Novo canal"); o testador deve verificar.
- Ajuda "?" ao lado de conceitos (RPM, algoritmo, nicho etc.) em todas as telas novas.
- Loja (fotos, mais imersão) e Funcionários: revisitar mais para frente.
- Dificuldade: Média. Depende de: 0.

**2. Top TubeStars** (feito em 2026-11-02: ranking vivo, perfil e ranking por nicho; placas no item 13)
- Abas YouTube (ativa), Twitch e Podcast (estas duas só com o nome por enquanto).
- 20 rivais famosos com perfil completo e cerca de 500 canais genéricos. 100% offline, com volatilidade. Rankings geral, por nicho e "quem mais cresceu na semana". Variação, dias no #1, placas.
- Dificuldade: Média. Depende de: 1 (etapa 3) e 3.

**3. Calendário** (feito em 2026-11-02; imposto anual fica para a Economia e Awards para o item 13)
- Semana de 7 dias, ano de 112 dias: "Ano X · Semana Y · Dia Z". Resumo diário e semanal (por canal), virada do ano com imposto anual e Awards.
- Dificuldade: Média. Depende de: 1 (etapa 2).

## Próximo (diversão e equilíbrio)

**6. Economia**
- Impostos progressivos, custos fixos, investimentos. Moeda: dólar. Ganho = views monetizáveis / 1000 × RPM (nicho, época, qualidade, marketing, algoritmo), visível. Patrocínio separado.
- Observação do dono (02/11/2026): empregado sobra dinheiro; ao sair do emprego o custo fixo quebra o jogador rápido. Equilibrar a transição para viver só do canal.
- **Em planejamento (03/11/2026), sem código ainda.** Hoje: canal paga $0,02-0,08/view pela estratégia + bônus √(inscritos/1000)×0,01 (até +$0,50/view) × nicho (`VideoViewer.ViewIncome`); 1 mi de inscritos ≈ $534/mil views. Sair do emprego = −$50/dia.
- Decisões do dono: manter escala de jogo (preços atuais); etapas 6a RPM, 6b meio período (4h, metade do salário), 6c imposto semanal sobre o lucro do canal (no resumo da semana), 6d investimentos por último.
- Observação do dono (03/11/2026): só trabalhando juntou $9 mil (salário $100 − custo $50 = +$50/dia) e a loja inteira custa $9 mil; o RPM "só aumenta".
- **Plano aprovado em 03/11/2026** (substitui a proposta 6a anterior). Uma etapa por vez, com teste do dono em cada uma:
  - **6a RPM por nicho e época: feita em 05/11/2026** (ver changelog e decisões).
  - **6b Emprego só paga as contas: feita em 05/11/2026** (ver changelog).
  - **6c Loja mais cara: feita em 05/11/2026** (vale só o maior nível; ver changelog).
  - **6d Imposto semanal: feito em 05/11/2026** (progressivo sobre o lucro do canal, dívida fiscal, Advogado Fiscal; ver changelog).
  - **6e Investimentos (decidido em 05/11/2026)**: item novo "Finanças" no menu (abaixo da Loja; o "Principal" continua reservado ao painel de visão geral do item 8). Tela com abas: agora só **Mercado** funciona; as outras mostram "Em breve" (Colecionáveis, Cripto, Negócios, Impostos).
    - Poupança: deposita e saca quando quiser, ~0,2%/semana, sem risco. Ações/fundo: média ~0,5%/semana, de −5% a +6%, resgate pelo valor do dia. Imóvel: aluguel ~0,4%/semana + valorização lenta; vender leva 2-4 semanas.
    - Mínimo $1.000. Imposto 15% sobre o lucro, cobrado no resgate. Saldo, rendimento e gráfico na tela; linha "Investimentos: +$X" no resumo da semana.
    - Próximo: designer desenha a tela (prévia para o dono aprovar), depois plano do tech-leader.
    - **Revisão do dono (06/11/2026):** a tela vira 3 partes. (1) **Finanças** = visão geral: Disponível, Investido/Patrimônio, Resultado da semana, Lucro no ano, gráfico de **patrimônio** (dinheiro + investimentos a valor atual − empréstimo − dívida fiscal; equipamentos não contam; no futuro inclui imóveis, empresas, colecionáveis e alimenta o **ranking dos mais ricos**), extrato **por semana**, sem metas. (2) **Finanças > Investimentos** (em breve) = análise da carteira (ações, cripto...), volatilidade, gráfico de patrimônio investido. (3) **Mercado financeiro** (em breve) = comprar/vender; o dono vai mandar referências.
    - Referências em `docs/referencias/<tela>/` com `notas.md`. Case study com 4 opções (A Extrato, B Patrimônio em destaque, C Próximas cobranças, D Painel do criador, recomendada) em `docs/case-study-financas.html`: **ainda fora da estética do jogo**; o designer deve refazer a partir das telas reais (`MainPage.xaml`, `Styles/`, painel do canal) antes do dono escolher.
    - **Visual aprovado (06/11/2026): opção D** em `docs/case-study-financas.html` (refeita a partir do print real da Agenda). Conteúdo:
      - Abas: Visão geral · Investimentos (em breve) · Mercado (em breve) · Colecionáveis (em breve, marcador provisório).
      - 4 números: Dinheiro disponível · Total investido · **Resultado da semana** (entrou − saiu no caixa; não usar "Rendimento") · Lucro acumulado (soma dos resultados semanais do ano).
      - Gráfico de linha do patrimônio por semana + barra de composição. Hoje patrimônio = **dinheiro − dívida fiscal** (só o que existe no jogo; empréstimo entra quando existir; equipamentos e canais **não** contam, decisão confirmada). O gráfico de patrimônio investido vai para a aba Investimentos.
      - Entradas **por canal** (nada por vídeo) + emprego/meio período. Saídas agrupadas: Contas fixas (custo de vida, imposto), Canal (promover vídeo, compra de views), Loja.
      - Extrato: últimas 8 semanas, clicar abre o detalhe; linhas com respiro (~40px). Sem "Próximas cobranças".
      - Regras futuras dos investimentos: valorização não realizada só mexe no patrimônio; transferência caixa ↔ carteira aparece no extrato como "Transferência" (cinza, neutra); Entradas = dinheiro de fora (YouTube, emprego, dividendos, juros) + lucro realizado na venda (só o lucro).
      - Próximo: tech-leader faz o plano técnico, programador implementa.
    - **Proposta do Mercado financeiro (06/11/2026, para o dono revisar; vem antes de patrocínio e "Minha marca")**. Substitui a ideia antiga de "poupança/ações/imóvel" da linha acima, se aprovada.
      - **Ideia em uma frase:** o jogador pega o dinheiro dos vídeos e coloca em empresas, criptos, fundos ou renda fixa de verdade (nomes reais), vê os preços mudarem todo dia e aprende que risco maior = ganho e perda maiores.
      - **Moeda:** tudo em dólar, sempre. Ativos do Brasil têm o preço convertido para dólar (ex.: ação de R$ 32 vira ~$6). Preços 100% simulados no jogo, sem internet e sem gastar cota da API. Valores iniciais perto do real (out/2026, aproximados); depois cada jogo segue seu próprio caminho.
      - **1. Ativos iniciais (29).** Logo = círculo com a inicial e uma cor. "Tend." = quanto sobe em média por ano do jogo (112 dias). "Vol." = quanto costuma mexer num dia. Div. = paga dividendo a cada 28 dias (1 estação).
        - *Ações Brasil (10), vol. 1,5-3%/dia:* PETR4 Petrobras (petróleo, ~$6, tend. +6%, div. alto), VALE3 Vale (mineração, ~$11, +5%, div. alto), ITUB4 Itaú (banco, ~$7, +10%, div.), BBDC4 Bradesco (banco, ~$3, +4%, div.), BBAS3 Banco do Brasil (banco, ~$4, +7%, div. alto), WEGE3 WEG (indústria, ~$8, +12%, div. baixo), ABEV3 Ambev (bebidas, ~$2,4, +4%, div.), ELET3 Eletrobras (energia, ~$8, +8%, div.), RENT3 Localiza (aluguel de carros, ~$9, +9%), MGLU3 Magazine Luiza (varejo, ~$1,8, +3%, vol. alta 3,5%, sem div.).
        - *Ações EUA (8), vol. 1-3%/dia:* AAPL Apple (~$230, +10%, div. baixo), MSFT Microsoft (~$480, +12%, div. baixo), NVDA Nvidia (chips, ~$180, +18%, vol. 3%), GOOGL Alphabet/Google (~$250, +11%), AMZN Amazon (~$220, +11%), TSLA Tesla (carros, ~$400, +10%, vol. 4%), KO Coca-Cola (bebidas, ~$70, +5%, vol. 0,8%, div.), DIS Disney (mídia, ~$115, +6%, div. baixo). Bônus de tema: GOOGL é dona do YouTube (ver item 8 do Futuro).
        - *Cripto (6), funciona 7 dias, vol. alta:* BTC Bitcoin (~$110.000, +20%, vol. 3%), ETH Ethereum (~$4.000, +18%, vol. 4%), SOL Solana (~$200, +15%, vol. 5%), XRP (~$2,80, +8%, vol. 5%), DOGE Dogecoin (meme, ~$0,25, 0%, vol. 7%, **arriscada**), PEPE (meme, ~$0,00001, −10%, vol. 10%, **arriscada**). Cripto pode comprar fração (0,001 BTC).
        - *Fundos (3), vol. baixa:* SPY (fundo das 500 maiores dos EUA, ~$660, +9%, vol. 0,9%, div. baixo), BOVA11 (fundo das maiores do Brasil, ~$27, +8%, vol. 1,2%), HGLG11 (fundo imobiliário de galpões, ~$30, +3%, vol. 0,6%, **aluguel alto** a cada 28 dias).
        - *Renda fixa (3), preço não cai:* Tesouro Selic (~14% ao ano, saca quando quiser), CDB Banco Grande (100% do CDI ≈ 14%/ano, saca quando quiser), CDB Banco Médio (120% do CDI ≈ 17%/ano, só saca no vencimento: 56 dias = meio ano do jogo; sacar antes perde os juros).
        - *Por classe, resumindo:* renda fixa = sem risco, ganho certo pequeno; fundos = pouco risco; ações grandes = risco médio; NVDA/TSLA/MGLU3 = mais nervosas; cripto = muito risco; meme = cassino.
      - **2. Como o preço muda a cada dia (explicado simples).** Preço de amanhã = preço de hoje × (1 + **tendência do dia** + **susto do dia** + **efeito de notícia**).
        - Tendência do dia = tendência anual dividida pelos 112 dias (ex.: +10% ao ano ≈ +0,085% por dia). É o "deixar rendendo": no longo prazo a maioria sobe.
        - Susto do dia = volatilidade × um sorteio (na maioria dos dias pequeno, às vezes grande). É o que faz subir e descer.
        - Notícia = empurrão extra por alguns dias (ver item 5). Três tipos: **da empresa** (só ela, ±3% a ±12%), **do setor** (ex.: "petróleo dispara", todos do setor ±2-6%), **crash do mercado** (raro, ~1 vez a cada 2 anos do jogo: tudo cai 15-30% em 3-5 dias e se recupera aos poucos; cripto cai mais).
        - Cripto e o mercado se movem um pouco juntos (dia ruim do mercado puxa quase todos), para o jogador sentir que diversificar ajuda, mas não salva de tudo.
        - **Limites para nunca ficar absurdo:** mexida máxima por dia ±12% (ações/fundos) e ±30% (cripto); preço nunca abaixo de 10% do inicial (meme: 1%); se subir mais de 3× ou cair abaixo da metade da "linha de tendência", passa a puxar de volta devagar. Assim não há falência total nem foguete infinito.
        - Bolsa (ações e fundos) só mexe em dias úteis (seg-sex); fim de semana o preço fica parado. Cripto mexe todo dia.
        - Renda fixa: juros por dia = taxa anual convertida para o ano de 112 dias (14%/ano ≈ +0,117% por dia, composto). Um ano do jogo rende o mesmo que um ano real.
      - **3. Regras.**
        - Corretagem: $2 por ordem em ações e fundos; 0,5% do valor em cripto; renda fixa grátis. Pequena, mas castiga quem compra e vende todo dia.
        - Imposto: 15% sobre o **lucro** na venda (só se teve lucro; prejuízo não paga). Mesma regra para cripto e renda fixa (no resgate). Fica no extrato da semana em Saídas > Contas fixas > Imposto. Mais simples que o imposto semanal do canal e separado dele.
        - Dividendos e aluguel do HGLG11 caem direto no dinheiro: Entrada "Dividendos" na Finanças.
        - Resgate: ações/fundos/cripto vendem na hora pelo preço do dia; Tesouro Selic e CDB 100% sacam na hora; CDB 120% só no vencimento.
        - DOGE e PEPE com etiqueta vermelha "Arriscada" e aviso na primeira compra ("pode perder quase tudo").
        - Sem mínimo de compra além do preço de 1 cota (cripto em fração, valor mínimo $10). Sem venda a descoberto, sem alavancagem.
        - **Encaixe com a Finanças (regras já decididas):** comprar = "Transferência" cinza (caixa → carteira, neutra, não é gasto); vender = Transferência de volta do valor investido + **Entrada "Lucro realizado"** só da parte que foi lucro (prejuízo realizado aparece como Saída "Prejuízo em investimentos"); valorização que não foi vendida só mexe no patrimônio. Patrimônio passa a ser dinheiro + carteira a valor do dia − dívida fiscal (o cálculo atual do patrimônio só ganha a parcela da carteira). "Total investido" no topo da Finanças passa a mostrar o valor da carteira hoje.
        - **Aba Investimentos da Finanças** (análise, sem comprar): valor da carteira, quanto colocou, ganho/perda total (em $ e %), rosca de alocação por classe (renda fixa, fundos, ações BR, ações EUA, cripto), gráfico do patrimônio investido por semana, tabela "minhas posições" (quantidade, preço médio, preço hoje, resultado), dividendos recebidos no ano e um indicador simples de risco da carteira (Baixo/Médio/Alto pela mistura).
      - **4. Telas.**
        - Menu: dentro de Finanças, aba **Mercado** (já reservada). Sem item novo no menu.
        - **Mercado (visão geral, estilo Prometheus):** faixa no topo com SPY, BOVA11, BTC e dólar parado; "Maiores altas / maiores quedas do dia"; desempenho por setor (barrinhas); notícias do dia com etiqueta Boa/Ruim/Neutra; tabela com abas Ações BR · Ações EUA · Cripto · Fundos · Renda fixa (nome, preço, variação do dia, mini-gráfico de 30 dias). Cartões: lucro total da carteira e "acerto" (vendas com lucro ÷ vendas).
        - **Ativo (estilo Rungkazt):** clicar numa linha abre. Watchlist à esquerda (favoritos com estrela), gráfico de linha no centro com períodos 7D · 28D (estação) · 112D (ano) · Tudo, texto curto "o que é esta empresa" e dividendos; painel Comprar/Vender à direita (quantidade ou valor em $, botões 25/50/75/100%, mostra corretagem e imposto estimado antes de confirmar). Embaixo, minhas ordens desse ativo. **Sem livro de ofertas e sem candle** no início (candle fica para depois).
        - Cores: tema escuro do jogo, alta/baixa com YtGain/YtLoss.
      - **5. Notícias.** 0 a 3 por dia, sorteadas. Aparecem na aba Mercado e numa linha curta no resumo do dia ("Petrobras sobe com petróleo em alta"). Cada notícia tem etiqueta Boa/Ruim e um "por quê" de uma frase ("empresa de petróleo ganha quando o petróleo sobe"). Às vezes chega **um dia antes** do efeito maior ("boatos de...") para premiar quem lê. Uma "Dica do mercado" fixa por semana ensina um conceito (diversificar, não vender no pânico, juros compostos, dividendos). Textos em inglês em `EnglishStrings.cs` + tradução no `Brazil.xml`; 40-60 modelos de notícia com o nome do ativo preenchido.
      - **6. Save.** Guardar: preço de hoje de cada ativo; histórico **só dos últimos 112 dias** por ativo (29 × 112 números ≈ pequeno) + um ponto por semana para "Tudo", com teto de ~5 anos; carteira (ativo, quantidade, preço médio, data de compra para o CDB); últimas ~200 transações; notícias ativas (efeito restante); semente do sorteio. Saves antigos: se não houver mercado no arquivo, o jogo cria um novo com preços iniciais e carteira vazia. Lista de ativos fica no código (não no save), para dar para ajustar sem quebrar saves.
      - **7. Etapas (cada uma testável pelo dono).**
        - M1 Motor de preços sem tela: ativos, preço diário, limites, renda fixa, save. Teste: passar 30 dias e ver no save/lista simples que os preços mexem com sentido. Média.
        - M2 Aba Mercado com tabela e maiores altas/quedas (só olhar). Média.
        - M3 Tela do ativo com gráfico e Comprar/Vender, carteira, corretagem, imposto, ligação com Transferência/Lucro realizado/patrimônio. Difícil (é onde mexe no dinheiro).
        - M4 Dividendos, aluguel do fundo e CDB com vencimento. Fácil-Média.
        - M5 Notícias, eventos de setor, crash e dicas. Média.
        - M6 Aba Investimentos da Finanças (análise da carteira). Média.
        - M7 Ajuste de equilíbrio: simular 2-3 anos e conferir que renda fixa ≈ +14%/ano, carteira mista ≈ +8-12%, meme quase sempre perde. Fácil.
        - Dificuldade geral: Difícil (mas dividida). Depende de: Finanças (6e visão geral) pronta.
        - **Riscos:** ficar fácil demais (investir vira melhor que fazer vídeo; resolver com tendências modestas e imposto), ou chato (muitos números; resolver com notícias e telas limpas); erros de dinheiro somando duas vezes no extrato; save crescer se não limitar histórico; nomes reais de empresas são só referência num jogo pessoal, sem logos oficiais.
      - **8. Futuro (a discutir, só esboço).**
        - Patrocínio de empresas da bolsa: marcas do Mercado (ex.: Coca-Cola, Magazine Luiza, Nvidia) oferecem publi; aceitar dá dinheiro, mas marca "odiada" ou excesso de publi baixa o buzz; notícia ruim da empresa pode respingar no canal. Talvez seus vídeos patrocinados mexam um pouquinho na ação.
        - "Minha marca": abrir empresa própria (roupa, energético, curso), divulgar nos vídeos, lucro depende do tamanho do canal; mais tarde "abrir capital" e a empresa aparecer no Mercado com ticker próprio.
    - **Respostas do dono (06/11/2026):** (1) mercado rende **devagar**: o canal continua sendo a fonte principal; (2) crash **raro** (~1 a cada 2 anos do jogo); (3) lista de 29 ativos aprovada por enquanto; (4) **fração de ação permitida**; (5) notícias na aba Mercado **e** uma linha no resumo do dia; (6) imposto de **15% sobre o lucro na venda para tudo**, inclusive renda fixa. Próximo: designer faz as prévias (Mercado estilo Prometheus e Ativo estilo Rungkazt, no tema do jogo), depois tech-leader planeja a etapa M1.
    - **Visual do Mercado aprovado (06/11/2026)** em `docs/mercado-preview.html`:
      - **"Mercado financeiro" vira item próprio no menu** (abaixo de Finanças), com abas **Mercado** (abre primeiro) e **Carteira**. Isso substitui "Mercado como aba da Finanças". A **aba Investimentos sai da Finanças** (a Carteira faz esse papel); Finanças fica com Visão geral e Colecionáveis (em breve), e o card "Total investido" mostra o valor da carteira.
      - Mercado: faixa de cotações, maiores altas/baixas com etiqueta Ação/Cripto/Fundo, setores (cartão na mesma altura dos vizinhos), notícias do dia, tabela por abas com estrela de favorito em cada linha. Sem cartões de carteira/lucro.
      - Carteira (ref. `docs/referencias/mercado/carteira-investidor10.png`): 4 cartões (patrimônio investido, lucro total = ganho + dividendos, dividendos recebidos, rentabilidade), barras empilhadas "Evolução da carteira" (aplicado + ganho, 4/8/16 semanas), rosca por tipo (centralizada, ~260px), "Meus ativos" em um cartão só com filtros Todos · Ações BR · Ações EUA · Cripto · Fundos · Renda fixa e resumo do filtro.
      - Tela do ativo: botão "☆ Favoritar" (vira "★ Favorito"), favoritos na lista da esquerda, gráfico 7d/28d/112d/Tudo, painel Comprar/Vender (fração, 25/50/75/100%, corretagem, imposto estimado). **"Sua posição"** só informa: quantidade, preço médio, total investido e variação desde a compra (sem descontos); o valor líquido fica só no painel Vender. Sem ativo: "Você ainda não tem X".
  - **Polimento visual:** ícones novos com o pacote **Phosphor** (grátis, sem crédito obrigatório, estilo único), convertidos para `Styles/Icons.xaml`.
  - **Andamento do Mercado (06/11/2026):** M1 motor de preços ✅ (log-normal, validado em 40 simulações de 2 anos; PEPE com tendência −35%/ano). M2 telas só para olhar ✅ (menu próprio, abas Mercado/Carteira, tela do ativo, favoritos no save; mostra os 8 setores que mais mexeram; bolinha colorida por ativo via `AssetDef.Color`; Dica da semana no cartão de notícias até a M5). Próximo: **M3**.
  - **Rever a fonte do jogo (pedido do dono, 06/11/2026): próximo passo logo depois de terminar o Mercado financeiro (M7).** O dono não gostou dos números com a Inter atual. Já testado e recusado: Poppins (ficou borrada). Ideias: números tabulares (`Typography.NumeralAlignment="Tabular"`), outra fonte só para números, peso/tamanho dos valores; fazer prévia comparando opções lado a lado no jogo antes de trocar.
  - **6f Ativos colecionáveis** (aba da tela Finanças, depois da 6e): vinhos raros, carros clássicos, moedas históricas, cards e quadrinhos. Preços oscilam e tendem a valorizar. Cards "crus" vão para certificadoras fictícias (sem usar PSA/Beckett/CGC), que dão nota 1-10 após alguns dias e taxa; nota 10 vale até 100× a nota 7. Ideia: vinho valoriza com o tempo, carro tem manutenção. Detalhar quando chegar. Dúvida aberta (06/11/2026): talvez comprar/vender numa aba da **Loja** e acompanhar o valor em Finanças (são ativos, não equipamentos).
- Dificuldade: Média. Depende de: 3, 4.

**7. Dificuldade**: escolhida no início; rever o que foi registrado no item 0. Fácil. Depende de: 6.

**8. Algoritmo visível**: "nota do canal" (frequência, constância, qualidade, tendência) + nota de cada vídeo. Média. Depende de: 3, 4.

**9. Universidade**: substitui os estudos; cursos livres com teto e universidade longa que quebra o teto. Áreas: gravação, edição, roteiro, marketing, administração. Empréstimo com juros altos. Difícil. Depende de: 6.

**10. Funcionários**
- Editor, roteirista, cinegrafista, marketing, gerente; júnior/pleno/sênior, salário fixo.
- Têm horas próprias, fora da agenda do jogador: permitem mais vídeos por semana. Cada cartão explica claramente o efeito.
- Dificuldade: Média. Depende de: 5, 6.

**10b. Eventos 2.0**: direção aprovada, falta detalhar
- Hoje: `Game/RandomEvents.cs`, 25% de chance por dia de 1 entre 16 eventos, em janela modal (`CustomMessageBox`) com fotos genéricas de `Resources/Messages`.
- Sem janela no meio do dia: eventos vão para o resumo do dia (só os graves em destaque).
- Causa explicada (ex.: desinscrição por ficar dias sem postar, viral num fórum).
- Ligados às mecânicas novas: tendência da semana, proposta de patrocínio, convite de collab, funcionário pedindo aumento, imposto anual, equipamento quebrado. Alguns com escolha (aceitar/recusar).
- Ícones no estilo do visual novo no lugar das fotos.
- Registro: eventos de robôs removidos a pedido do dono (30/10/2026).
- Dificuldade: Média. Depende de: 1, 3 (resumo do dia), 6.

## Depois (ideias grandes)

**11. Collabs e live em collab (Twitch)**. Média. Depende de: 2.
**12. Patrocínios**. Média. Depende de: 2, 6.
**13. TubeStar Awards e placas**. Fácil/Média. Depende de: 2, 3. Ideia do dono (02/11/2026): além de "Melhor YouTuber do ano", prêmios por nicho (Melhor Gamer, Melhor Vlogger etc.).
**14. Podcast**. Média. Depende de: 6, 11.
**15. Lado empresário**. Difícil. Depende de: 6, 9.
**16. Empresas no exterior e bancos** (precisa de estudo próprio). Difícil. Depende de: 15.
**17. Vida pessoal e árvore familiar**. Difícil. Depende de: 6.
**18. Gráfico de views por dia na tela Ver vídeo** (guardar histórico diário de cada vídeo). Fácil/Média.

## Ideias guardadas

- **Miniaturas por nicho como reserva da busca**: quando a cota de 100 buscas/dia acabar ou o título não achar nada, usar miniaturas recentes de canais reais do nicho (playlistItems, cota geral de 10.000). Falta o plano do planejador.

- **Iscas no vídeo (ex-5b)**: título chamativo, thumbnail com rosto, tendência, participação especial, sorteio; 1 isca, 2ª com roteiro/roteirista. Adiada em 02/11/2026: fica solta sem Tendências (Eventos 2.0) e Collabs (item 11).
- **Personalidade dos tipos de vídeo** (sobra do item 5, feito em 03/11/2026): ex.: Tutorial dura mais, React pico rápido, Documentário nota maior. Rever depois de jogar com os tipos.
- **Título aleatório em Criar vídeo** (ideia do dono, 03/11/2026): gerar título pelo tipo de vídeo e nicho, em português. Condição: nunca repetir. Lembrar que a miniatura é buscada pelo título.
- **Ecossistema fiscal** (ideia do dono, 05/11/2026): banco central com tela de impostos (histórico pago, quando vence, dívidas fiscais), fiscalização. A 6d já guarda o total pago. Vira a aba "Impostos" da tela Finanças.
- **Polimento visual (discutir com o dono, 05/11/2026 à noite)**: o dono acha o WPF menos nítido que a prévia HTML. Ideias: (1) texto em tons de cinza em vez de ClearType no tema escuro e conferir a fonte Inter embutida com todos os pesos; (2) miniaturas com `BitmapScalingMode` de alta qualidade; (3) ícones presos aos pixels; (4) telas que se adaptam ao tamanho (pendência antiga do notebook), uma por vez; (5) acabamento (sombras, cantos, hover, espaçamentos). DPI já está certo no manifest. Troca de tecnologia: dono decidiu **ficar no WPF** por agora (Avalonia seria o melhor caminho, mas levaria semanas); foco em entregar o polimento com o máximo de qualidade.
- **Finanças, abas futuras** (ideias do dono, 05/11/2026): **cripto** (vai ser feito), investir em negócios, comprar participações em empresas, projetos imobiliários, criar indústrias (ligar aos itens 15 e 16).
- **Eventos de mercado** (para os Eventos 2.0): crise na bolsa, bairro valorizou etc.; pensar com calma como notificar e como funciona.
- **Meta de monetização**: feita em 05/11/2026 (5k inscritos + 160k views, análise 3-7 dias).
- **Reformulação completa da Loja** (pedido do dono, 03/11/2026): mais opções, fotos, ganhos e valores revistos. Depois da Economia.
- **Instagram** e **Kick**: futuro, talvez.
- **Caminho C (estúdio ilustrado)**: descartado, falta arte.
- **Trocar de tecnologia**: descartado.
- **Relação com rivais e feed de notícias**: por enquanto não.
- **Shorts, TikTok, polêmicas e cancelamentos**: fora do jogo.
- **Comentários com frases geradas** e **comprar views**: removidos.
