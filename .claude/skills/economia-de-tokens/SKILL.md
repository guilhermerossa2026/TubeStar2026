---
name: economia-de-tokens
description: Boas práticas para gastar poucos tokens no TubeStar. Use ao começar uma tarefa nova, antes de ler muitos arquivos, ao planejar trabalho grande, ao escolher qual subagente chamar ou quando a conversa estiver ficando longa ou mudando de assunto.
---

# Economia de tokens no TubeStar

Tokens são o "combustível" de cada conversa. Ler arquivos, repetir contexto e usar modelos fortes em tarefas simples é o que mais gasta.

## 1. Ler só o necessário

- Comece pelo `CLAUDE.md` e, se precisar de detalhes, pelo `docs/visao-geral.md`. Eles já resumem o projeto.
- Encontre antes de ler: use Grep/Glob para achar o arquivo e a linha certos, depois leia **só aquele trecho** (com `offset`/`limit`).
- Não abra pastas inteiras "para entender". As pastas `bin/`, `obj/`, `3rd Party/`, `Resources/` (imagens) e `Languages/*.xml` (traduções enormes) quase nunca precisam ser lidas.
- Evite comandos que despejam muita saída. No MSBuild, use `/v:m` ou `/v:q` e filtre só os erros.

## 2. Não reler

- O que já foi lido nesta conversa continua no contexto. Não leia de novo, a não ser que o arquivo tenha mudado.
- Depois de editar um arquivo, não leia de novo para conferir: a ferramenta avisa se a edição falhou.

## 3. Dividir tarefas grandes

- Tarefa grande vira passos pequenos (o tech-leader faz essa divisão). Cada passo deve ser compilável e revisável sozinho.
- Um assunto por vez: termine, faça o commit e só então comece o próximo.

## 4. Usar o subagente certo

- Tarefa simples e bem definida → **programador** (Sonnet) ou **testador** (Haiku).
- Só use o **tech-leader** (Opus) para decisões de arquitetura ou revisão de mudanças relevantes.
- Não chame subagente para algo que se resolve com uma busca ou uma edição rápida: cada subagente começa do zero e relê contexto.
- Passe para o subagente tudo o que ele precisa no pedido (arquivos, linhas, objetivo), para ele não ter que explorar.

## 5. Manter o CLAUDE.md curto

- O `CLAUDE.md` é carregado em **toda** conversa. Cada linha dele custa tokens sempre.
- Nele, só regras e fatos essenciais. Detalhes vão para `docs/` e são lidos quando necessário.
- Ao mudar algo no projeto, atualize a linha correspondente em vez de acrescentar novas.

## 6. Resumir antes de trocar de assunto

- Ao terminar um assunto, faça um resumo curto do que foi decidido e feito. Se for algo duradouro, registre em `docs/` ou no `docs/roadmap.md`.
- Para um assunto novo e sem relação, prefira uma conversa nova (`/clear`) a continuar uma conversa longa. Use `/compact` quando ainda precisar do contexto, mas a conversa estiver grande.
