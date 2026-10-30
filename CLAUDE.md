# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Projeto

TubeStar: simulador casual de YouTuber em C#/WPF (.NET Framework 4.8), open-source (MIT), herdado para uso pessoal, sem fins comerciais. Responder sempre em português. Diagnóstico completo em [docs/visao-geral.md](docs/visao-geral.md).

## Build e execução

```powershell
& C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe TubeStarDesktop.sln /p:Configuration=Release /v:m
```

- Não há Visual Studio na máquina: esse MSBuild usa o compilador do **C# 5**. Nada de `?.`, `$"..."`, `nameof`, propriedades `=>` etc.
- A saída vai para `TubeStarDesktop/bin/Release/`. A cópia jogável fica em `../Jogar_TubeStar/` e é atualizada **manualmente** (exe, pdb, DLLs).
- Não há testes automatizados nem git.

## Arquitetura (essencial)

- Estado global em singletons estáticos: `Player.Current` (dinheiro, canais, vídeos, tarefas, dia em `Iterations`), `Rivals.Current` e `Settings`.
- O ciclo do dia fica em `MainPage.xaml.cs`: o botão de novo dia chama `dailyPlanner.NewDay()` e `rivalViewer.NewDay()`, que avançam tarefas, visualizações e renda.
- Uma classe por conceito, com base abstrata + subclasses: `Tasks/` (Task, Studies), `Video/VideoAttributes/`, `OnlineStore/` (StoreItem), `Rivals/` (Rival1..15).
- Textos: escrever em inglês em `Languages/EnglishStrings.cs` e usar `.Translate()`. As traduções ficam em `Languages/*.xml` (PT-BR = `Brazil.xml`).
- Save: JSON (Newtonsoft, `TypeNameHandling.Auto`) em `Saves/save.tss`, via `Game/SaveLoad/`. Precisa continuar lendo saves XML antigos.
- YouTube Data API v3 (`YouTube API/YouTubeAPI.cs`) só é chamada para os vídeos do jogador. A chave é lida de `youtube_api_key.txt` ao lado do exe (fonte em `TubeStarDesktop/`, ignorado pelo git) ou da variável de ambiente `YOUTUBE_API_KEY`; nunca colocar a chave no código. Os rivais são offline de propósito, para poupar cota; não reintroduzir chamadas para eles.
- GameJolt está desativado (código morto).
- Mods: `TubeStar.exe RIVALMOD "<xml>"` troca os rivais (é o que o `ModLauncher` faz).

## Estilo visual

Tema escuro estilo YouTube: fundo `#0F0F0F`, cabeçalhos `#1F1F1F`, campos `#141414`/`#1A1A1A`, hover `#262626`/`#2D2D2D`, destaque em vermelho YouTube. Não usar `LightGray`/`DodgerBlue`.
