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
- **6a proposta, aguardando "pode fazer":** base da estratégia igual ($40/mil na Normal); bônus de inscritos vira ≈5×log10(inscritos/100): +$5 (1 mil), +$10 (10 mil), +$15 (100 mil), +$20 (1 mi), teto +$25. RPM em $/mil visível no painel do canal (com "?") e em Ver vídeo (aba Dinheiro). Saves avançados perdem bastante renda.
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

- **Iscas no vídeo (ex-5b)**: título chamativo, thumbnail com rosto, tendência, participação especial, sorteio; 1 isca, 2ª com roteiro/roteirista. Adiada em 02/11/2026: fica solta sem Tendências (Eventos 2.0) e Collabs (item 11).
- **Personalidade dos tipos de vídeo** (sobra do item 5, feito em 03/11/2026): ex.: Tutorial dura mais, React pico rápido, Documentário nota maior. Rever depois de jogar com os tipos.
- **Instagram** e **Kick**: futuro, talvez.
- **Caminho C (estúdio ilustrado)**: descartado, falta arte.
- **Trocar de tecnologia**: descartado.
- **Relação com rivais e feed de notícias**: por enquanto não.
- **Shorts, TikTok, polêmicas e cancelamentos**: fora do jogo.
- **Comentários com frases geradas** e **comprar views**: removidos.
