# Decisões

## 2026-11-07 — Aba Carteira (M3 P6)
**Decisão:** lucro total já desconta o imposto; rentabilidade = lucro total ÷ tudo o que já foi aplicado; peso de cada ativo sempre sobre a carteira inteira; lucro realizado e total aplicado guardados em campos acumulados (não recalculados dos Trades, que guardam só 200).
**Motivo:** mostra o que de fato entrou no bolso e continua fazendo sentido depois de vender tudo; os Trades perdem o histórico antigo.

## 2026-11-07 — Painel Comprar/Vender (M3 P5)
**Decisão:** compra só por valor em $, venda só por quantidade (com 25/50/75/100%); sucesso em texto verde no painel; vírgula e ponto valem como decimal.
**Motivo:** o jogador pensa em quanto gastar ao comprar e em quanto do que tem ao vender; descartados os dois modos em cada lado (tela mais cheia) e a janela de aviso (clique a mais).

## 2026-11-07 — Finanças com o Mercado (M3 P4)
**Decisão:** linha "Lucro bruto em investimentos" (antes da corretagem da venda); transferências só no detalhe do extrato; carteira em roxo (#9575CD) na barra.
**Motivo:** "Lucro" sozinho confundia quando a alta é menor que a taxa; painel da semana fica limpo.

## 2026-11-07 — Mercado na Finanças (M3 P3)
**Decisão:** corretagem da compra só aparece na venda (embutida no lucro/prejuízo); imposto do Mercado fica fora do total de impostos pagos do canal.
**Motivo:** evita contar a corretagem duas vezes (já está no preço médio) e mantém o imposto do canal separado do de investimentos.

## 2026-11-07 — Regras de compra/venda do Mercado (M3 P2)
**Decisão:** comprar/vender liberado no fim de semana (preço de sexta); corretagem entra no preço médio; mínimo de $10 vale o valor digitado (com corretagem).
**Motivo:** fechar no fim de semana não evitaria nenhuma vantagem e deixaria o jogo mais chato; preço médio com corretagem é como numa corretora real.

## 2026-11-05 — Meta de monetização mais alta que a real
**Decisão:** 5 mil inscritos + 160 mil views (real: 1 mil + 4 mil horas), com análise de 3 a 7 dias. Saves antigos começam bloqueados (dono começou jogo novo).
**Motivo:** escolha do dono para o canal não ganhar desde o primeiro vídeo e o começo ser mais desafiador. Rever se o início ficar arrastado.

## 2026-11-05 — Equipamento: vale só o melhor nível
**Decisão:** Câmera/Edição contam só o maior nível comprado e exigem o anterior. Troféus desligados.
**Motivo:** somar os 5 níveis levava a nota média a 8,7 (66% dos vídeos ≥9); com o maior nível fica ~5,3. Alternativa descartada: manter soma com bônus menores. Troféus nunca funcionaram para o dono e não agregam agora.

## 2026-11-05 — RPM por nicho, compensado com views
**Decisão:** RPM fixo por nicho (Infantil 15 a Finanças 60) e nichos baratos ganham views a mais (expoente 0,35), sem inscritos a mais. Promover vídeo continua sem lucro: serve para crescer.
**Motivo:** realismo do YouTube; sem compensação ninguém escolheria nichos baratos. A quantidade de anúncios não faz perder inscritos (perda vem de nota baixa e canal parado); regra nova só se o dono pedir.

## 2026-11-03 — Promover vídeo caro e sem reembolso
**Decisão:** campanhas de $500/$1.000/$2.000 (preços inflados pelo dono), views em 3 dias, sem renda de anúncio. Se o canal/vídeo for suspenso, a campanha acaba e o dinheiro não volta.
**Motivo:** promover deve ser investimento de quem já ganha, não atalho; a suspensão é risco do jogo. As opções "por view" (baratas demais) foram descartadas.

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
