# Visão geral e diagnóstico — TubeStar

*Diagnóstico feito em 30/10/2026, sobre o código em `TubeStar-master/`.*

## 1. O que é

TubeStar é um simulador casual de YouTuber, criado por Mike Hall (cerca de 2012–2014, licença MIT). O jogador gerencia canais, grava, edita e publica vídeos, estuda para melhorar habilidades, compra itens e compete com canais rivais até conseguir largar o emprego e viver da renda de anúncios.

## 2. Linguagem e tecnologias

| Item | Situação |
|---|---|
| Linguagem | C# (≈11 mil linhas de `.cs` + `.xaml`) |
| Interface | WPF (XAML), só Windows |
| Runtime | .NET Framework 4.8 (migrado da 4.0 na "Fase A") |
| Formato de projeto | `.csproj` antigo (não SDK-style), `ToolsVersion` 4.0 / 12.0 |
| Dependências | DLLs soltas em `TubeStarDesktop/3rd Party/`, sem NuGet |
| Dados externos | YouTube Data API v3 (miniaturas, títulos e comentários reais) |
| Persistência | JSON via Newtonsoft (com leitura de saves XML antigos) |

## 3. Estrutura de pastas

```
Nova pasta/
├─ TubeStar-master/          ← código-fonte (raiz do projeto)
│  ├─ TubeStarDesktop.sln
│  ├─ TubeStarDesktop/       ← o jogo (gera TubeStar.exe)
│  │  ├─ Game/               Player (estado global), RandomEvents, Settings, SaveLoad/
│  │  ├─ Video/              Channel, Video, VideoCategory, comentários, estratégia de anúncios
│  │  │  └─ VideoAttributes/ atributos especiais (Cats, Memetic, SoBad, Copycat...)
│  │  ├─ Tasks/              ações do dia (Job, ShootVideo, EditVideo...) e Studies/ (cursos)
│  │  ├─ OnlineStore/        itens da loja (câmeras, software, advogado, empréstimo...)
│  │  ├─ Rivals/             Rival1..Rival15 + Rivals.cs (lista e carregamento de mod)
│  │  ├─ UI/                 diálogos e controles WPF
│  │  ├─ Languages/          strings em inglês (EnglishStrings.cs) + traduções em XML
│  │  ├─ Mods/               modelos de mod (Custom Language, Custom TopTubeStars)
│  │  ├─ YouTube API/        montagem das URLs da API
│  │  ├─ GameJolt Integration/ troféus/placar online (desativado)
│  │  ├─ Styles/, Resources/ tema e imagens
│  │  └─ 3rd Party/          DLLs de terceiros
│  └─ ModLauncher/           lançador que escolhe um mod de rivais e abre o jogo
├─ Jogar_TubeStar/           cópia jogável (exe + DLLs + Mods + Saves)
└─ *.md                      relatórios das fases anteriores
```

## 4. Como o jogo funciona

1. **Início**: `StartWindow` → `TitlePage` → `NewGameDialog` (modos Casual, Carreira/Challenge e Ultra) ou carregar save.
2. **Estado global**: tudo vive em singletons estáticos — `Player.Current` (dinheiro, canais, vídeos, habilidades, tarefas, dia atual em `Iterations`) e `Rivals.Current`.
3. **Ciclo diário** (`MainPage`): o jogador monta a agenda no `DailyPlanner` com `Task`s (trabalhar, gravar, editar, estudar). Ao clicar em "novo dia", `dailyPlanner.NewDay()` e `rivalViewer.NewDay()` avançam a simulação: tarefas progridem, vídeos ganham visualizações (`VideoViewer`), inscritos e renda de anúncios; `RandomEvents` pode disparar eventos.
4. **Vídeos**: o título digitado vira uma busca na YouTube API, que devolve miniaturas e comentários reais. Categoria, qualidade e atributos influenciam o desempenho.
5. **Progressão**: estudos aumentam `ShootingSkill`/`PostProductionSkill` e liberam pontos de atributo; a loja melhora equipamento. O objetivo é largar o emprego (`QuitJob`).
6. **Rivais**: 15 canais de IA; um XML em `Mods/Custom TopTubeStars` pode substituí-los (passado como `TubeStar.exe RIVALMOD "<caminho>"`, que é o que o ModLauncher faz).
7. **Tradução**: o código usa `EnglishStrings.X.Translate()`; o XML do idioma escolhido substitui o texto.
8. **Save**: `Saves/save.tss` ao lado do executável.

## 5. O que está pronto (trabalho já feito)

Segundo os relatórios de 26/10/2026 e o código atual:

- Migração para .NET Framework 4.8 nos dois projetos.
- TLS 1.2 forçado em `App.xaml.cs` (necessário para a API do Google).
- Chave própria da YouTube Data API v3 configurada.
- Rivais 100% offline (títulos/miniaturas gerados) para não gastar cota da API.
- Save em JSON com leitura dos saves XML antigos.
- Tema escuro estilo YouTube em todos os diálogos e cards.
- GameJolt desativado e botão de login escondido.
- **Compila**: `MSBuild.exe TubeStarDesktop.sln /p:Configuration=Release` termina sem erros (testado hoje).

## 6. O que está quebrado ou merece atenção

| Problema | Detalhe |
|---|---|
| **Chave da API no código** | `YouTube API/YouTubeAPI.cs` tem a chave fixa no código. Se o projeto for para o GitHub, a chave vaza. Melhor mover para um arquivo de configuração fora do controle de versão. |
| **Crash ao iniciar novo jogo (histórico)** | `Jogar_TubeStar/Crash Report.txt` (26/10, 10:14) mostra uma exceção no construtor de `RivalControl` ao abrir a `MainPage`. O exe é de 11:22 do mesmo dia, então pode já estar corrigido. Precisa ser confirmado jogando. |
| **GameJolt morto** | Código ainda presente em `GameJolt Integration/`, `TitlePage`, `DailyPlanner`, `TodoTask` e no tratamento de argumentos em `App.xaml.cs`. Dá para remover. |
| **Sem testes automatizados** | Não existe projeto de testes. |
| **Sem controle de versão** | A pasta não é um repositório git; não há histórico das mudanças da Fase A. |
| **Cópia manual para `Jogar_TubeStar`** | Nada no `.csproj` copia o build para lá; a saída vai para `TubeStarDesktop/bin/Release/`. |
| **Ferramenta de build antiga** | Sem Visual Studio na máquina; o build usa o MSBuild 4.0 do Windows, que só aceita C# 5 (sem `?.`, `$""`, `nameof` etc.) e avisa da falta do pacote de referência do .NET 4.8. |
| **Estado global estático** | `Player.Current`, `Rivals.Current` e `Settings` estáticos espalham dependências pelo código e dificultam testes e novas mecânicas. |
| **`.gitignore` desatualizado** | Ainda cita pastas de uma versão antiga (`/TubeStar/...`). |
| **Lixo de build** | `obj/` e `bin/` estão dentro da pasta de código, e há `TubeStar-master.zip` e arquivos `.md` duplicados em `Jogar_TubeStar/`. |
| **Tradução PT-BR incompleta** | Existe `Languages/Brazil.xml`, mas os relatórios listam a tradução completa como pendente; nomes de rivais e comentários (`RivalStrings.cs`, `CommentStrings.cs`) estão em código, em inglês. |

## 7. Dependências desatualizadas

| Dependência | Versão no projeto | Situação |
|---|---|---|
| .NET Framework | 4.8 | Ainda suportado, mas sem evolução; o caminho moderno é .NET 8 (WPF também roda lá). |
| Newtonsoft.Json | 4.5 (≈2012) | Atual é 13.x. A 4.5 tem falhas de segurança conhecidas, e o save usa `TypeNameHandling.Auto` — risco ao abrir saves de terceiros. |
| OxyPlot / OxyPlot.Wpf | 2013.1 | Atual é 2.x (API mudou). Usado nos gráficos de estatísticas do canal. |
| Xceed.Wpf.Toolkit | DLL antiga sem versão | Versões novas mudaram de licença; a última livre é a 3.8 (pacote comunitário "Extended.Wpf.Toolkit"). |
| Nicenis | DLL antiga | Biblioteca abandonada. Vale verificar o que se usa e substituir. |
| GameJolt API | — | Integração morta. |
| Formato `.csproj` | antigo | Converter para SDK-style permite NuGet e `dotnet build`. |

## 8. Sugestão de ordem para as melhorias

1. Pôr o projeto no git e mover a chave da API para fora do código.
2. Confirmar se o crash do `RivalControl` ainda acontece.
3. Converter os `.csproj` para SDK-style e trocar as DLLs soltas por NuGet (Newtonsoft 13, OxyPlot 2, Extended.Wpf.Toolkit), com cópia automática para `Jogar_TubeStar`.
4. Avaliar migrar para .NET 8 (C# moderno e melhor suporte a DPI).
5. Remover o GameJolt.
6. Depois: ranking, tradução PT-BR completa e novas mecânicas (Shorts, Lives etc.), como já planejado em `registro_avancos_e_proximos_passos.md`.
