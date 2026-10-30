# Decisões

## 2026-10-30 — Chave da API fora do git
**Decisão:** a chave fica em `youtube_api_key.txt` (ignorado pelo git) ou na variável de ambiente `YOUTUBE_API_KEY`.
**Motivo:** permite deixar o repositório público sem vazar a chave. A alternativa de commitar a chave num repositório privado foi descartada pelo risco de vazamento futuro.

## 2026-10-30 — Equipe de subagentes com modelos diferentes
**Decisão:** tech-leader no Opus, programador e designer no Sonnet, testador no Haiku. Só o programador e o designer editam arquivos.
**Motivo:** economizar tokens usando modelo forte só onde erro custa caro (arquitetura e revisão).

## 2026-10-26 — Rivais offline
**Decisão:** os rivais geram títulos e miniaturas sem chamar a YouTube API.
**Motivo:** a cota gratuita (10.000 pontos por dia) acabava em minutos com 15 rivais buscando a cada dia de jogo.
