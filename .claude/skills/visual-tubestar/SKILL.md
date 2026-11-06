---
name: visual-tubestar
description: Guia visual do TubeStar (WPF/XAML, tema escuro estilo YouTube). Use SEMPRE antes de desenhar, prévia HTML ou mudar qualquer tela, estilo, ícone, cor, layout ou espaçamento, e para conferir uma tela antes de entregar ao dono.
---

# Visual TubeStar

O jogo é WPF (.NET 4.8, C# 5), não web. Toda prévia HTML tem que **imitar o jogo real**; nunca inventar um visual novo.

## 1. Antes de começar
- Peça/abra um **print da tela real** (`docs/referencias/<tela>/`). Sem print, leia `MainPage.xaml`, `Styles/*.xaml` e a tela vizinha mais parecida.
- Reaproveite estilos existentes (`Styles/Components.xaml`, `Custom.xaml`, `Icons.xaml`). Estilo novo só se nenhum servir, e no arquivo de estilos, nunca solto na tela.
- Prévia HTML pronta: abrir no navegador (`Start-Process arquivo.html`) sem pedir.

## 2. Tokens (fonte: `Styles/Custom.xaml`)
| Uso | Cor | Recurso |
|---|---|---|
| Fundo da página | #0F0F0F | YtDarkBgBrush |
| Menu lateral / campos | #141414 | YtDarkSurfaceBrush |
| Cartão / cabeçalho | #1F1F1F | YtDarkCardBrush |
| Hover | #262626 | YtDarkCardHoverBrush |
| Borda | #2D2D2D | YtDarkBorderBrush |
| Destaque / botão primário | #E53935 (hover #EF5350) | YtRedBrush |
| Texto principal / secundário | #F1F1F1 / #AAAAAA | YtTextPrimary/SecondaryBrush |
| Ganho / perda | #3EA66B / #E5534B | YtGainBrush / YtLossBrush |
| Desativado | fundo #2A2A2A, texto #6B6B6B | — |

Nunca: `LightGray`, `DodgerBlue`, cores soltas (`#xxxxxx` direto na tela) quando existe recurso. Profundidade no escuro = **superfície mais clara**, não sombra.

## 3. Layout
- Toda página principal: ScrollViewer vertical > contêiner **MaxWidth 1400**, margem lateral 24, centralizado. Título grande + subtítulo cinza no topo da página; a barra superior **não** repete o título.
- Espaçamento em múltiplos de 4 (4, 8, 12, 16, 24). Cartões: CornerRadius 8, padding 16.
- Precisa caber em **1366×768 com zoom 125%** (~1092×614 úteis) e ficar bonito em 2560×1440: nada de Width fixo em painéis grandes; use colunas `*`, MaxWidth, WrapPanel/UniformGrid com colunas calculadas e ScrollViewer.
- Diálogos: altura máxima acompanha a janela; botão OK sempre visível fora da rolagem.
- Cuidado: dentro de ScrollViewer, linhas `*` viram `Auto`.

## 4. Nitidez (lições aprendidas)
- Na janela raiz já existe: `UseLayoutRounding`, `SnapsToDevicePixels`, `TextOptions.TextFormattingMode="Display"`, `TextRenderingMode="Grayscale"`, `BitmapScalingMode="HighQuality"`. Não remover.
- **Não** usar DropShadowEffect/BlurEffect em pais de texto ou em listas: borra o texto e custa desempenho. Sombra só em diálogo, e no Border de fundo, não no conteúdo.
- Botão/pill colorido: **sem borda de 1px da mesma cor** com CornerRadius (gera halo escuro nos cantos). Use BorderThickness 0 e compense no Padding.
- Opacidade: preferir cor com alfa no Brush a `Opacity` no elemento.
- Fonte Inter embutida (Regular, Medium, SemiBold, Bold). Não trocar (Poppins foi testada e recusada: borrada).

## 5. Ícones
- Phosphor **regular** (MIT), geometrias em `Styles/Icons.xaml` com prefixo `F1` (NonZero).
- Uso: `Viewbox` > `Canvas 256×256` > `Path Fill=... Stretch="None"`. Sempre **Fill**, nunca Stroke. Novo ícone: baixar de `https://unpkg.com/@phosphor-icons/core@2.1.1/assets/regular/<nome>.svg` e copiar o `d`.
- Nada de emoji (fica sem cor no WPF).

## 6. Desempenho
- Miniaturas: `BitmapImage` com `DecodePixelWidth` no tamanho exibido quando o uso é fixo; `Freeze()` em brushes/imagens que não mudam.
- Listas longas: ItemsControl com VirtualizingStackPanel; não recriar a tela inteira a cada dia.

## 7. Decisões fixas do dono (não "corrigir")
- Dinheiro em **dólar, formato americano** ($1,030.00).
- Abas Twitch (roxo) e Podcast (azul) no Top TubeStars são de propósito.
- Textos novos: EnglishStrings + `Brazil.xml` (Ids idênticos ao enum; Id inexistente quebra o idioma inteiro).
- Selo de texto (VIRAL / EM ALTA / FLOPOU), sem emoji.

## 8. Checklist antes de entregar
- [ ] Conferido contra o print/tela vizinha (mesmas cores, fontes, raio, espaçamento).
- [ ] Cabe em ~1092×614 (rola, nada cortado) e não estica feio em 2560.
- [ ] Sem cor solta, sem LightGray/DodgerBlue, sem Stroke em ícone, sem efeito em texto.
- [ ] Estados: hover, selecionado, desativado (cinza) e vazio ("nenhum item") pensados.
- [ ] Compila (MSBuild, C# 5) e o dono recebe o que testar.
