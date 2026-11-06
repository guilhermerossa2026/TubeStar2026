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

## 3. Verificação (você)
- Para cada achado Crítico/Alto, abra o trecho e confirme. Descarte o que não se sustenta; marque como "a confirmar" o que depender de rodar o jogo.
- Junte duplicados e respeite as decisões fixas (memória e skill `visual-tubestar`: dólar americano, cores Twitch/Podcast etc.).

## 4. Relatório
Escrever `docs/revisoes/AAAA-MM-DD.md` (único arquivo criado; avisar o dono antes) em português simples:
1. **Resumo em 5 linhas**: saúde geral (nota 0–10 por área: Bugs, Save, Desempenho, Código, Visual, Equilíbrio) e os 3 pontos mais urgentes.
2. **Achados por gravidade** (tabela): # · área · problema em uma frase · onde (`arquivo:linha`) · o que acontece para o jogador · esforço.
3. **Gargalos de desempenho** com estimativa de impacto.
4. **Melhorias e ideias** (não são bugs): código, visual, jogo.
5. **Plano sugerido**: 3 a 6 pacotes pequenos em ordem (cada um = uma etapa com commit próprio), indicando o agente de cada um.
6. O que **não** foi possível verificar.

Depois, mostre ao dono o resumo e o plano no chat (curto) e pergunte por qual pacote começar. Nada é corrigido sem o "pode fazer".

## Regras
- Gastar tokens com cabeça: agentes leem os lotes; você só lê trechos para confirmar.
- Não reportar estilo pessoal como bug. Cada achado precisa de um cenário concreto.
- Não repetir achados já resolvidos (conferir changelog e a revisão anterior).
