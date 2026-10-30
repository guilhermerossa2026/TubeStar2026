# Roadmap do TubeStar

> **Regra do dono:** cada ideia é discutida a fundo, **uma por vez**. Antes de qualquer código, a versão final da ideia é apresentada para aprovação.
> Status padrão: **ideia aprovada, falta detalhar**.

Decisões de 30/10/2026. A ordem abaixo é a ordem de discussão.

## Agora (bases)

**0. Consertar a base**: aprovado, vem antes do visual
- Simulação: trocar o loop view a view (`VideoViewer.ViewVideo`) e o loop por amigo (`ShareVideo`) por cálculo direto; vídeos antigos calculados de forma leve.
- Comentários: remover frases geradas e chamadas à API de comentários; fica só o ícone com número estimado pelas views.
- Tela de vídeos: parar de recriar tudo a cada dia.
- Save: limpar os dados de vídeos excluídos.
- Bugs: desinscrição por divisão inteira (`VideoViewer.cs:224`); `sqrt(subs/1000)` inteiro (`:305`); ramo de 50% inalcançável (`CategoryHelpers.cs:83`); Money/Subscribers acessados do BackgroundWorker sem sincronização.
- Registro (para o item 5): hoje não há ajuste de dificuldade ao criar o canal; o Ultra é só o Challenge com CTR de 10%; o gasto fixo é $50/dia.
- Dificuldade: Média. Depende de: nada.

**1. Visual novo: V1 Studio (WPF/XAML)**: versão escolhida
- Prévia: `docs/estudo-visual.html`. Etapas: (1) cores e fontes em `Styles/`; (2) cabeçalho, menu lateral e relógio Ano/Semana/Dia; (3) cards novos e Top TubeStars; (4) demais telas.
- Ajustes: tela título sem o botão Top TubeStars; Meus vídeos com seletor de canal, "+ Novo canal", grade de 9 com paginação, filtros, busca, painel direito mais estreito, ver todos e excluir; botão de thumbnail em Criar vídeo.
- Thumbnail (Criar vídeo): busca na API só pelo TÍTULO; categoria só como fallback sem resultado (hoje "categoria,nome" + sorteio de 1 em 50 em `Video.FetchRandomImage`, `Video.cs:549`). Sem sorteio, ordem de relevância. Carrossel com setas (direita = próxima, esquerda = anterior exata) e contador "N de 50". Os 50 resultados ficam guardados; ao passar do fim, busca a próxima página (pageToken, +1 busca). Cada imagem é baixada só quando exibida. Botões "Usar esta" e "Buscar outras" (troca o termo).
- Postar vídeo: lista compacta dos vídeos prontos à esquerda (uma linha por vídeo); o selecionado aparece à direita com o canal e o "Promover vídeo".
- Cuidado: modais e telas sem caixas vazando (visto na prévia de "Novo canal"); o testador deve verificar.
- Ajuda "?" ao lado de conceitos (RPM, algoritmo, nicho etc.) em todas as telas novas.
- Loja (fotos, mais imersão) e Funcionários: revisitar mais para frente.
- Dificuldade: Média. Depende de: 0.

**2. Top TubeStars**
- Abas YouTube (ativa), Twitch e Podcast (estas duas só com o nome por enquanto).
- 20 rivais famosos com perfil completo e cerca de 500 canais genéricos. 100% offline, com volatilidade. Rankings geral, por nicho e "quem mais cresceu na semana". Variação, dias no #1, placas.
- Dificuldade: Média. Depende de: 1 (etapa 3) e 3.

**3. Calendário**
- Semana de 7 dias, ano de 112 dias: "Ano X · Semana Y · Dia Z". Resumo diário e semanal (por canal), virada do ano com imposto anual e Awards.
- Dificuldade: Média. Depende de: 1 (etapa 2).

## Próximo (diversão e equilíbrio)

**4. Canais e nichos**
- Canais ilimitados; nicho escolhido na criação, fixo depois. Vídeo no nicho rende mais; fora do nicho rende menos, mas atrai público novo.
- Nichos (substituem as 18 categorias antigas), cada um com RPM e concorrência próprios: Games, Vlog/Lifestyle, Tecnologia, Humor, Educação, Finanças, Fitness/Saúde, Culinária, Música, Beleza/Moda, Esportes, Reacts, Mistério/True Crime, Viagem, Infantil.
- Cabeçalho com canal ativo e seletor (inclui "Visão geral" = soma). Painel do canal: views e ganhos totais, inscritos, RPM, nº de vídeos, gráfico.
- Dinheiro global; inscritos e algoritmo por canal.
- Dificuldade: Difícil (mexe no save). Depende de: 0, 1.

**5. Criação de vídeo nova**
- Atributos atuais REMOVIDOS. Entra "tipo de vídeo" (Tutorial, Review, React, Desafio, Vlog, Top 10, Documentário; cada um combina com certos nichos) + até 2 "iscas" (título chamativo, thumbnail com rosto, tendência, participação especial, sorteio). Nº de iscas cresce com roteiro e roteirista. Prévia mostra os efeitos.
- Horas fixas pela experiência: gravar/editar fica mais rápido com a habilidade; sliders de horas saem.
- "Custo" vira "investimento na produção" (aumenta a nota). "Comprar views" REMOVIDO. "Promover vídeo" na hora de postar, com estimativa de views e inscritos antes de confirmar.
- Dificuldade: Média. Depende de: 4.

**6. Economia**
- Impostos progressivos, custos fixos, investimentos. Moeda: dólar. Ganho = views monetizáveis / 1000 × RPM (nicho, época, qualidade, marketing, algoritmo), visível. Patrocínio separado.
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
**13. TubeStar Awards e placas**. Fácil/Média. Depende de: 2, 3.
**14. Podcast**. Média. Depende de: 6, 11.
**15. Lado empresário**. Difícil. Depende de: 6, 9.
**16. Empresas no exterior e bancos** (precisa de estudo próprio). Difícil. Depende de: 15.
**17. Vida pessoal e árvore familiar**. Difícil. Depende de: 6.

## Ideias guardadas

- **Preço das views compradas "por view"**: após a correção do custo, $0,02/$0,04 (=$20/$80) ficou bem mais barato que os pacotes de $100/$200. O dono quer aumentar; rever com o planejador junto do "Promover vídeo".
- **Instagram** e **Kick**: futuro, talvez.
- **Caminho C (estúdio ilustrado)**: descartado, falta arte.
- **Trocar de tecnologia**: descartado.
- **Relação com rivais e feed de notícias**: por enquanto não.
- **Shorts, TikTok, polêmicas e cancelamentos**: fora do jogo.
- **Comentários com frases geradas** e **comprar views**: removidos.
