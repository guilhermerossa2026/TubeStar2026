---
name: revisao-projeto
description: Revisão completa do TubeStar - lê todo o código e entrega um estudo com bugs, riscos, gargalos de desempenho, problemas de save, visual, UX, equilíbrio do jogo e melhorias, priorizados. Use quando o dono pedir "revisa o projeto", "auditoria", "o que dá pra melhorar", "procura bugs no jogo todo".
---

# Revisão do projeto

Objetivo: um estudo honesto e priorizado do projeto inteiro. **Só leitura**: não editar código; o único arquivo criado é o relatório.

## 1. Preparação (você, sem agente)
- `git log --oneline -15` e `git status`; ler `CLAUDE.md`, `docs/onde-paramos.md`, `docs/roadmap.md` (só o necessário) e a última revisão em `docs/revisoes/` (se houver) para não repetir achados já resolvidos.
- Compilar (MSBuild, ver CLAUDE.md) e anotar avisos novos.
- Listar arquivos `.cs`/`.xaml` por pasta e dividir em lotes.

## 2. Varredura em paralelo (subagentes, numa única mensagem)
Dispare de 3 a 5 agentes, cada um com um lote e a lista de checagem da sua área. Peça a cada um: achados com `arquivo:linha`, cenário concreto de falha, gravidade (Crítico/Alto/Médio/Baixo) e esforço (P/M/G). Sem achados vagos.

1. **testador (Bugs e travamentos)**: nulos, índices, divisão por zero, exceções engolidas (`catch {}`), threads/BackgroundWorker mexendo em estado, eventos não desinscritos (vazamento), lógica de dia/semana/ano, dinheiro contado duas vezes ou nunca, arredondamentos, `Random` mal usado.
2. **tech-leader (Save e arquitetura)**: compatibilidade de saves (JSON `TypeNameHandling.Auto` + XML antigo), campos novos sem padrão, listas que crescem sem limite, singletons (`Player.Current`, `Rivals.Current`), acoplamento tela↔regra, código morto, duplicação, C# 5 respeitado, chave da API fora do código, cota da YouTube API (rivais offline; biblioteca de miniaturas 1×/semana).
3. **tech-leader (Desempenho)**: o que roda a cada "Próximo dia" (custo por vídeo/rival), telas recriadas inteiras, imagens sem `DecodePixelWidth`, listas sem virtualização, efeitos (sombras/blur), LINQ em laços quentes, I/O na thread da tela, tamanho do save.
4. **designer (Visual e UX)**: aplicar a skill `visual-tubestar` em todas as telas e diálogos: cores soltas, larguras fixas, telas que não cabem em 1366×768, estados vazios/desativados, textos sem tradução PT-BR, consistência entre telas, feedback ao jogador.
5. **designer (Equilíbrio do jogo)**: economia (renda, RPM, custos, imposto, loja), progressão (começo lento? fim sem objetivo?), itens inúteis, números absurdos, realismo do YouTube (preferência do dono).

## 2b. Simulação da economia (Python)
Não julgar equilíbrio só lendo. Peça ao agente de Equilíbrio (ou faça você) para:
1. Mapear as regras com os **números reais do código** e o arquivo de cada uma: renda (RPM por nicho/época, emprego, meio período), custos (custo de vida, imposto semanal progressivo, loja, promoção, compra de views, empréstimo), views (nota, sorte flop/viral, empurrão de canal pequeno, impulso, cauda), inscritos e meta de monetização.
2. Escrever scripts Python no **scratchpad da sessão** (nunca na pasta do projeto), reproduzindo essas regras e avisando que é uma réplica do C#.
3. Simular com semente fixa (Monte Carlo) estilos de jogador: cauteloso, agressivo (compra tudo cedo), ocioso, otimizado. Medir:
   - dinheiro ao longo do tempo (inflação, falência, dinheiro inútil);
   - tempo até marcos: monetizar, 100k inscritos, top 10, comprar cada item da loja;
   - estratégias dominantes (algo que sempre compensa) e inúteis (itens/opções que nunca valem);
   - ciclos de lucro infinito (exploits) e estados sem saída;
   - sensibilidade: variar ±20% os parâmetros principais e ver quais mudam mais o resultado.
4. Todo número de ajuste sugerido precisa ter sido **testado no script**. Informar o que foi simplificado e onde ficaram os scripts.

## 3. Verificação (você)
- Classifique cada achado como **confirmado no código**, **provável** ou **hipótese a testar**. Sem evidência (`arquivo:linha` ou trecho), não é achado.
- Para cada achado Crítico/Alto, abra o trecho e confirme. Descarte o que não se sustenta.
- Agrupe ocorrências do mesmo padrão num achado só, listando os locais.
- Junte duplicados e respeite as decisões fixas (memória e skill `visual-tubestar`: dólar americano, cores Twitch/Podcast etc.).

## 4. Relatório
Escrever `docs/revisoes/AAAA-MM-DD.md` (único arquivo criado; avisar o dono antes) em português simples:
1. **Resumo em 5 linhas**: saúde geral (nota 0–10 por área: Bugs, Save, Desempenho, Código, Visual, Equilíbrio) e os 3 pontos mais urgentes.
2. **Cobertura**: quantos arquivos e linhas foram lidos, o que ficou de fora e por quê.
3. **Achados por gravidade** (tabela): # · área · problema em uma frase · onde (`arquivo:linha`) · o que acontece para o jogador · confirmado/provável/hipótese · esforço.
4. **Gargalos de desempenho** com impacto estimado e quando aparece (ex.: "com 300 vídeos", "após 2 anos de jogo").
5. **Simulação da economia**: tabelas pequenas, estratégias dominantes/inúteis, tempo até cada marco e ajustes testados.
6. **Upgrades** separados em **mecânicos** (novas mecânicas, metas, feedback), **técnicos** (código, arquitetura, ferramentas) e **lógicos** (regras mais justas, casos de borda). Cada um: o que muda, por que vale, onde mexer, esforço.
7. **Plano sugerido**: 3 a 6 pacotes pequenos em ordem de impacto × esforço (cada um = uma etapa com commit próprio), indicando o agente de cada um.
8. O que **não** foi possível verificar (ex.: não rodou o jogo).

Depois, mostre ao dono o resumo e o plano no chat (curto) e pergunte por qual pacote começar. Nada é corrigido sem o "pode fazer".

## Regras
- Gastar tokens com cabeça: agentes leem os lotes; você só lê trechos para confirmar.
- Não reportar estilo pessoal como bug. Cada achado precisa de um cenário concreto.
- Não repetir achados já resolvidos (conferir changelog e a revisão anterior).
