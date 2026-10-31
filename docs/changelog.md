# Changelog

## 2026-10-30
- Comentários viraram só um número (likes ÷ 20 + dislikes ÷ 10): saíram as frases geradas, a lista na janela do vídeo e a busca de comentários na API do YouTube.
- Desinscrições e bônus de renda por inscritos deixaram de usar divisão inteira: agora há perda real de inscritos (com piso de zero) e o bônus cresce de forma suave.
- Opções de views "por view" ao postar agora conferem o dinheiro antes, como os pacotes fixos (evitava game over por compra barata).
- Custo "por view" agora é cobrado só nas views compradas (antes, em todas as views do vídeo para sempre); views extras do SecondTime viraram bônus sem custo.
- Tela de postar mais robusta: resposta inválida da internet não fecha o jogo, falha na busca mostra a imagem "sem internet", cliques rápidos nas setas mantêm a última imagem e o handler da imagem é removido ao fechar.
- Corrigido o travamento ao postar quando a busca de miniaturas caía no fallback por categoria (`UploadVideoDialog.xaml.cs`).
- Projeto colocado no git e publicado em github.com/guilhermerossa2026/TubeStar2026.
- Chave da YouTube API saiu do código: agora é lida de `youtube_api_key.txt` (ao lado do exe) ou da variável `YOUTUBE_API_KEY`.
- `.gitignore` atualizado (bin/, obj/, chave da API).
- Criados `CLAUDE.md`, `docs/visao-geral.md` (diagnóstico), os subagentes (planejador, tech-leader, programador, designer, testador) e as skills `economia-de-tokens` e `documentar`.

## 2026-10-26 (Fase A, antes do git)
- Migração para .NET Framework 4.8, TLS 1.2, save em JSON compatível com o XML antigo, rivais offline, tema escuro estilo YouTube e GameJolt desativado.
