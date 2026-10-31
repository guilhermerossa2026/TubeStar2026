# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Projeto

TubeStar: simulador casual de YouTuber em C#/WPF (.NET Framework 4.8), open-source (MIT), herdado para uso pessoal, sem fins comerciais. Diagnóstico completo em [docs/visao-geral.md](docs/visao-geral.md). Histórico e ideias iniciais em `../relatorio_avancos_fase_tecnica.md` e `../registro_avancos_e_proximos_passos.md`: o dono quer rediscutir tudo, mas só dar feedback sobre eles quando ele pedir.

## Como trabalhar comigo

**Quem decide o quê**
- O dono é iniciante em agentes e não escolhe quem ativar. Você decide qual subagente (`.claude/agents/`) usar em cada situação.
- O dono decide O QUE será feito e se ficou bom. Você decide o COMO e QUEM.

**Aprovação antes de modificar**
- Nunca modificar, criar ou apagar nenhum arquivo sem aprovação prévia, nem em tarefa pequena. Isso inclui commit e push.
- Antes de qualquer mudança, explicar em 2 ou 3 linhas simples: o que vai fazer, quais arquivos serão afetados e qual agente vai acionar e por quê. Esperar o "pode fazer".
- Só leitura e análise podem ser feitas sem pedir.

**Fluxo de trabalho**
- Ideia vaga: acionar o planejador e refinar com o dono antes de qualquer código.
- Para implementar: o tech-leader divide em tarefas, o programador (ou o designer, se for visual) implementa e o testador valida.
- Trabalhar em passos pequenos, uma tarefa por vez.

**Testes e compilação**
- Ao terminar cada tarefa, compilar (ver "Como rodar e testar o jogo"). O testador valida antes do dono e traz o resultado.
- Dizer ao dono exatamente como rodar e o que testar, em passos simples.

**Feedback antes de avançar**
- Ao concluir cada passo, parar e pedir que o dono teste. Só seguir depois da aprovação ou depois de ajustar o que ele apontar.
- Bugs ou resultado de que ele não gostou devem ser corrigidos antes de seguir.
- Só documentar (skill `documentar`: changelog, decisões, roadmap) depois da aprovação.

**Comunicação**
- Sempre em português, linguagem simples.
- Se algo for grande ou arriscado, avisar antes e sugerir um commit no git, para poder voltar atrás.

## Versionamento com Git

- Só depois que o dono testar e aprovar uma etapa, perguntar: "Foi aprovado, posso subir para o Git?" e esperar o OK. Nunca fazer commit ou push sem confirmação explícita.
- Antes de commitar, mostrar em 2 ou 3 linhas os arquivos incluídos e a mensagem sugerida.
- Mensagens claras, em português, descrevendo o que foi feito (ex.: "Adiciona sistema de rivalidade entre canais").
- Um commit por etapa aprovada; nunca misturar várias melhorias.
- Nunca subir código não aprovado ou apontado como quebrado pelo testador.
- Nunca usar comandos destrutivos (`push --force`, `reset --hard` etc.) sem pedido explícito.
- Depois de subir, confirmar que deu certo e só então passar para a próxima melhoria.
- Antes de uma mudança grande ou arriscada, sugerir um commit do estado atual.

## Como rodar e testar o jogo

Não há Visual Studio nesta máquina. O build usa o MSBuild que vem com o Windows. Rode os comandos no PowerShell, a partir desta pasta (a raiz do projeto, onde está o `TubeStarDesktop.sln`):

1. Compilar:
   ```powershell
   & C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe TubeStarDesktop.sln /p:Configuration=Release /v:m
   ```
   Deu certo se terminar sem linhas `error`. Os avisos `warning` (MSB3644, MSB3270, CS0168) são esperados.
2. Rodar: abrir `TubeStarDesktop\bin\Release\TubeStar.exe` (ou `& .\TubeStarDesktop\bin\Release\TubeStar.exe`).
3. Se o jogo travar, o erro fica em `Crash Report.txt`, na pasta do exe.

Detalhes:
- O MSBuild do Windows só aceita **C# 5**: nada de `?.`, `$"..."`, `nameof`, propriedades `=>` ou `out var`.
- A chave da API precisa estar em `TubeStarDesktop/youtube_api_key.txt` (o build copia para a saída) ou na variável `YOUTUBE_API_KEY`. Sem ela, as miniaturas não carregam.
- O save fica em `bin\Release\Saves\save.tss`, separado do save de `../Jogar_TubeStar/`. Essa pasta é a cópia "oficial" para jogar e é atualizada à mão.
- O git não está no PATH da sessão do Claude: usar `$env:Path += ";C:\Program Files\Git\cmd"`.
- Não há testes automatizados.

## Arquitetura (essencial)

- Estado global em singletons estáticos: `Player.Current` (dinheiro, canais, vídeos, tarefas, dia em `Iterations`), `Rivals.Current` e `Settings`.
- O ciclo do dia fica em `MainPage.xaml.cs`: o botão de novo dia chama `dailyPlanner.NewDay()` e `rivalViewer.NewDay()`, que avançam tarefas, visualizações e renda.
- Uma classe por conceito, com base abstrata + subclasses: `Tasks/` (Task, Studies), `Video/Attributes/`, `OnlineStore/` (StoreItem), `Rivals/` (Rival1..15).
- Textos: escrever em inglês em `Languages/EnglishStrings.cs` e usar `.Translate()`. As traduções ficam em `Languages/*.xml` (PT-BR = `Brazil.xml`).
- Save: JSON (Newtonsoft, `TypeNameHandling.Auto`) via `Game/SaveLoad/`. Precisa continuar lendo saves XML antigos.
- YouTube Data API v3 (`YouTube API/YouTubeAPI.cs`) só é chamada para os vídeos do jogador. Nunca colocar a chave no código. Os rivais são offline de propósito, para poupar cota; não reintroduzir chamadas para eles.
- GameJolt e mods (ModLauncher/RIVALMOD) foram removidos. Os troféus são locais, em `Game/Trophies/TrophyManager.cs`.

## Economia de tokens

Detalhes na skill `economia-de-tokens`. Em resumo:
- Ler só o necessário: buscar com Grep/Glob e ler apenas o trecho, sem reler o que já foi lido.
- Dividir tarefas grandes em passos.
- Para tarefas simples, usar subagentes leves (programador, testador).
- Manter este arquivo curto.
- Resumir antes de trocar de assunto.

## Estilo visual

Tema escuro estilo YouTube: fundo `#0F0F0F`, cabeçalhos `#1F1F1F`, campos `#141414`/`#1A1A1A`, hover `#262626`/`#2D2D2D`, destaque em vermelho YouTube. Não usar `LightGray`/`DodgerBlue`.
