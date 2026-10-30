---
name: documentar
description: Atualiza docs/changelog.md, docs/decisoes.md e docs/roadmap.md no TubeStar. Use ao final de cada feature, correção relevante ou decisão importante, antes do commit.
---

# Documentar

Ao concluir uma feature ou tomar uma decisão importante, atualize os três arquivos abaixo, em português. Cada arquivo que não existir deve ser criado com o título indicado. As entradas mais novas ficam no topo, e as datas no formato AAAA-MM-DD.

## docs/changelog.md — o que foi feito

Título: `# Changelog`. Agrupe por data, com um item por mudança, do ponto de vista do jogador ou do projeto:

```
## 2026-10-30
- Chave da YouTube API saiu do código e passou a ser lida de youtube_api_key.txt.
```

## docs/decisoes.md — por que escolhemos assim

Título: `# Decisões`. Registre só as decisões que alguém poderia questionar depois: escolha entre alternativas, regra nova ou algo que descartamos.

```
## 2026-10-30 — Título curto
**Decisão:** o que foi escolhido.
**Motivo:** por quê, e qual alternativa foi descartada.
```

## docs/roadmap.md — o que falta

Marque como feito removendo o item. Em vez de acumular uma lista de concluídos, o histórico fica no changelog. Acrescente o que surgiu de novo, como pendências, bugs encontrados e ideias que ficaram para depois, na seção de prioridade certa: **Agora / Próximo / Depois / Ideias guardadas**.

## Regras

- Escreva no máximo 1 a 3 linhas por entrada. Nada de copiar código ou repetir o que já está em outro arquivo.
- Só atualize o arquivo que realmente mudou. Uma correção pequena pode ir só para o changelog.
- Inclua os arquivos de docs no mesmo commit da mudança.
