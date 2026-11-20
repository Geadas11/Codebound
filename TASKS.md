# Tarefas

Legenda: `[x]` feito e verificado (o método está entre parênteses) · `[~]` feito mas não verificado no Unity · `[ ]` por fazer

## Em progresso
- [~] Etapa 3 (antecipada): host de sandbox Wasmtime + CPython WASI. Código e testes escritos, **não executados**: a execução foi bloqueada pelo modo automático e precisa de autorização (ver `NEXT_SESSION.md`)

## Etapa 1 — Fundação
- [~] Criar projeto Unity (manifest, versão, asmdefs) — **por abrir no editor**
- [x] Estrutura de pastas e módulos (documentada em `TECHNICAL_ARCHITECTURE.md`)
- [~] Personagem (sprite procedural, 4 direções, animação de passo)
- [x] Movimento top-down com colisão na grelha (lógica: testes dotnet) · [~] integração Unity
- [~] Câmara (seguimento suave, zoom em múltiplos inteiros de pixel, snapping)
- [x] Mundo de teste procedural com seed (testes de determinismo e acessibilidade dos recursos; imagem de pré-visualização) · [~] renderização Tilemap
- [x] Sistema de interação: recolher recursos, abrir armazenamento (lógica: testes) · [~] Unity
- [x] Inventário básico com stacking e transferências (testes) · [~] HUD de desenvolvimento
- [x] Construção em grelha: multi-célula, rotação, pré-visualização, obstáculos, custo, desmontar com reembolso (testes) · [~] Unity
- [ ] **Abrir no Unity 6, corrigir erros de compilação/importação, jogar 5 min e registar o resultado**
- [ ] Guardar/carregar mundo (base para Save; necessário antes do multiplayer)
- [ ] Camadas de construção (chão + objeto na mesma célula)
- [ ] Modo de planeamento: blueprints colocados sem gastar recursos e construídos depois

## Etapa 2 — Primeiras máquinas
- [ ] Framework de máquinas no tick da simulação (estado, consumo, inventário de entrada/saída)
- [ ] Perfuradora (2x2 sobre depósitos de minério)
- [ ] Energia: gerador a carvão, rede elétrica por proximidade/postes, consumo e falta de energia
- [ ] Computador (CPU, RAM, armazenamento, rede, energia)
- [ ] Ligações entre máquinas (computador ↔ máquinas controladas)
- [ ] Primeira máquina programável (depende do runtime da Etapa 3)
- [ ] Regra: máquinas sem programa válido não funcionam

## Etapa 3 — Programação funcional
- [ ] Host de sandbox num processo separado (Wasmtime): limites de CPU (fuel), RAM e tempo; interrupção segura
- [ ] Protocolo IPC Unity ↔ host (JSON) e cliente `Codebound.Execution`
- [ ] Primeira linguagem executável + API Codebound (`drill.mine()`, `energy.available()`, `computer.on(...)`)
- [ ] Execução contínua e por eventos
- [ ] Gestão de CPU/RAM ligada ao hardware do computador
- [ ] Editor de código in-game, explorador de ficheiros, terminal
- [ ] Debugger: breakpoints, inspeção de variáveis, histórico de execução
- [ ] Documentação e exemplos in-game; níveis Guiado / Normal / Avançado
- [ ] Restantes linguagens: JavaScript, TypeScript, Python, PHP, SQL, C, C++, C#; HTML/CSS no HUD

## Etapa 4 — Automação
- [ ] Transportadores básicos, inteligentes e programáveis
- [ ] Armazéns e armazenamento automático (SQL)
- [ ] Fabricação: fundição, processamento, componentes, circuitos
- [ ] Redes (routers, switches, fibra, antenas, servidores, firewalls)
- [ ] HUD programável HTML/CSS com botão "+" e painéis móveis, redimensionáveis e guardáveis

## Etapa 5 — Mundo completo
- [ ] Mundo infinito por chunks; seeds partilháveis
- [ ] Biomas: floresta, deserto, vulcânico, ruínas tecnológicas, radioativo
- [ ] Dia/noite, clima, estações, tempestades, eventos
- [ ] Subterrâneo (túneis, ventilação, água, gases, temperatura)
- [ ] Sobrevivência: vida, fome, sede, oxigénio, temperatura
- [ ] Agricultura (estufas, hidroponia, aeroponia, genética, irrigação), sem pecuária
- [ ] Ecologia e poluição
- [ ] Criaturas e máquinas hostis; combate; segurança informática
- [ ] Investigação: laboratórios, árvore híbrida, competências por uso
- [ ] Instalações abandonadas, registos, missões e narrativa

## Etapa 6 — Multiplayer
- [ ] Servidor local (host), até 8 jogadores, convites
- [ ] Sincronização (simulação autoritária no host)
- [ ] Permissões, construções privadas/públicas/de equipa, histórico, conflitos
- [ ] Comércio por troca, contratos, reputação
- [ ] Salvamento consistente; o mundo pausa quando todos saem

## Etapa 7 — Espaço
- [ ] Naves modulares e sistemas de bordo programáveis
- [ ] Viagens automáticas/manuais; luas, asteroides, planetas
- [ ] Estações, colónias, sondas; combate espacial simples

## Etapa 8 — Polimento
- [ ] Arte final (sprites, animações, iluminação 2D/URP, partículas, clima)
- [ ] Áudio adaptativo, áudio espacial, sons por estado de máquina
- [ ] Acessibilidade completa (escala, daltonismo, contraste, legendas, flashes, remapeamento, teclado)
- [ ] Localização para 10 idiomas
- [ ] Otimização, testes, correção de erros

## Concluídas (resumo)
- [x] Skills do projeto: `codebound-session`, `unity-conventions`
- [x] Ferramentas: `Tools/LogicTests` (42 testes), `Tools/UnityCompileCheck`, `Tools/WorldPreview`
