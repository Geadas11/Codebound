# Codebound

**BUILD. CODE. SURVIVE. EXPLORE.** — sobrevivência, automação industrial em grelha, exploração e **programação real** (HTML, CSS, JavaScript, SQL, Python, PHP, TypeScript, C, C++, C#) num planeta desconhecido. Unity 2D, pixel art top-down.

## Estado
Etapas 1–2, partes das 3–5 e da 8: quatro linguagens reais (**JavaScript**, **TypeScript**, **Python**, **SQL**) com debugger a comandar máquinas, HUD em HTML/CSS, sobrevivência, clima, agricultura, fabricação, investigação, criaturas, opções de acessibilidade e áudio sintetizado — ver [TASKS.md](TASKS.md).

## Requisitos
- Unity **6000.6.10f1** (Unity 6) — instalado via Unity Hub ou `unity install 6000.6.10f1`.
- **Node.js** (≥ 22; testado 24.13) no PATH — runtime isolado do código JavaScript/TypeScript.
- **Python** (runtime de Python e SQL): CPython 3.15.2 embeddable para Windows em `Codebound/Tools/python/` — **não está no git** (26 MB). Num clone novo, descarregar `python-3.15.2-embed-amd64.zip` de python.org, confirmar o SHA-256 indicado em [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) e extrair para essa pasta.
- Unity CLI (`unity`) opcional, para testes/automação.

## Abrir e jogar
1. Abrir a pasta `Codebound/` no Unity Hub (ou `unity open Codebound`).
2. Abrir `Assets/Scenes/SampleScene.unity` e carregar **Play**. O jogo constrói-se sozinho em código (`GameBootstrap`).
3. Seed: aleatória, mostrada no HUD. Para fixar numa build: `Codebound.exe -seed 12345`.

### Controlos
| Ação | Tecla |
|---|---|
| Mover | WASD / setas |
| Minerar / cortar árvore | manter LMB sobre jazida (alcance 3) |
| Modo construção | B (ou 1–9, 0 para escolher estrutura) |
| Rodar | R |
| Colocar / remover (reembolso) | LMB / RMB em modo construção |
| Sair do modo | Esc ou Q |
| Abrir máquina / editor de código do computador | E (alcance 5) |
| Inventário | Tab |
| Guardar / carregar | F5 / F9 |
| Nova caixa do HUD (HTML/CSS) | botão **+** no canto superior direito |
| Editar / minimizar / fechar caixa | **E** / **_** / **x** na barra da caixa; arrastar a barra move, o canto redimensiona |
| Beber água / comer fruta alienígena | manter LMB sobre água / **F** |
| Opções, pausa, acessibilidade, remapear teclas | **Esc** |
| Árvore de investigação | **T** |
| Atacar criatura | manter LMB sobre ela (alcance 3) |
| Zoom | roda do rato |

## Primeiro programa
1. Mina pedra, ferro, cobre e carvão; constrói **Perfuradora** (4) sobre minério, **Baú** (2) à frente dela (lado de cima), **Gerador** (6) com carvão (E → put) e **Computador** (5) perto; usa **Postes** (7) se estiver longe do gerador.
2. E no computador → **Run**. O exemplo "Drill loop" põe a perfuradora a trabalhar.
3. Para depurar: clica num número de linha (breakpoint) e carrega **Debug**; F8 continua, F10 avança uma instrução.
4. Correias (8) e transportadores programáveis (9) levam os itens entre máquinas; `conveyor.start()` liga o programável.

## Build Windows
No Unity: menu **Codebound > Build Windows (with Node runtime)** → `Codebound/Builds/Windows/Codebound.exe` (inclui o Node para o código dos jogadores). Verificação automática:
```bash
Codebound/Builds/Windows/Codebound.exe -selftest -logFile selftest.log
```
Procura `[SELFTEST] PASS` no log (código de saída 0).

## Testes
```bash
unity test Codebound --mode EditMode --output test-results.xml
node Codebound/Tests/runtime/host.test.js
Codebound/Tools/python/python.exe -I Codebound/Tests/runtime/python_host_test.py
```

## Documentação
- [GAME_DESIGN.md](GAME_DESIGN.md) · [TECHNICAL_ARCHITECTURE.md](TECHNICAL_ARCHITECTURE.md) · [DEVELOPMENT_LOG.md](DEVELOPMENT_LOG.md) · [TASKS.md](TASKS.md) · [NEXT_SESSION.md](NEXT_SESSION.md)
