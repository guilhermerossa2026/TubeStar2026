# Decisões

## 2026-11-02 — Horas pela habilidade e investimento em níveis
**Decisão:** gravar 4/3/2/1h e editar 6/4/3/2h conforme a habilidade (<40, 40-59, 60-79, 80+); horas extras não dão mais nota. Investimento em níveis fixos com retorno decrescente. Iscas (5b) adiadas.
**Motivo:** a evolução fica visível e não dá para "comprar" nota com horas. Iscas ficariam soltas sem Tendências e Collabs.

## 2026-11-02 — Tipo de vídeo: bônus pequeno, sem penalidade
**Decisão:** tipo que combina com o nicho dá +10% de views orgânicas; não combinar é neutro. Os cursos "Análise de Público I e II" foram escondidos (davam pontos de atributo, que perderam o uso).
**Motivo:** +20% virava bônus fixo (todo nicho tem tipos que combinam) e pesava mais que a escolha do nicho. A "personalidade" de cada tipo ficou para depois.

## 2026-10-30 — Chave da API fora do git
**Decisão:** a chave fica em `youtube_api_key.txt` (ignorado pelo git) ou na variável de ambiente `YOUTUBE_API_KEY`.
**Motivo:** permite deixar o repositório público sem vazar a chave. A alternativa de commitar a chave num repositório privado foi descartada pelo risco de vazamento futuro.

## 2026-10-30 — Equipe de subagentes com modelos diferentes
**Decisão:** tech-leader no Opus, programador e designer no Sonnet, testador no Haiku. Só o programador e o designer editam arquivos.
**Motivo:** economizar tokens usando modelo forte só onde erro custa caro (arquitetura e revisão).

## 2026-10-26 — Rivais offline
**Decisão:** os rivais geram títulos e miniaturas sem chamar a YouTube API.
**Motivo:** a cota gratuita (10.000 pontos por dia) acabava em minutos com 15 rivais buscando a cada dia de jogo.
