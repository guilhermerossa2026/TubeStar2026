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
  - **6f Ativos colecionáveis** (aba da tela Finanças, depois da 6e): vinhos raros, carros clássicos, moedas históricas, cards e quadrinhos. Preços oscilam e tendem a valorizar. Cards "crus" vão para certificadoras fictícias (sem usar PSA/Beckett/CGC), que dão nota 1-10 após alguns dias e taxa; nota 10 vale até 100× a nota 7. Ideia: vinho valoriza com o tempo, carro tem manutenção. Detalhar quando chegar.
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
