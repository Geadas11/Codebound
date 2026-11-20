# Codebound — Game Design

> **BUILD. CODE. SURVIVE. EXPLORE.**
> Esta é a especificação de referência. Alterações ao conceito têm de ser registadas em `DEVELOPMENT_LOG.md` com justificação.

## 1. Identidade
- **Género:** sobrevivência, construção e automação industrial, mundo aberto, exploração, programação real, ficção científica, cooperativo.
- **Plataformas:** Windows, Linux, macOS. **Motor:** Unity (2D, C#).
- **Visual:** pixel art detalhada, câmara top-down, construção em grelha (liberdade industrial ao estilo Factorio).
- **Público:** 12+/13+. Sem violência gráfica excessiva.
- **Modelo:** lançamento completo (não Early Access pago). Demo gratuita opcional, mais tarde.
- **Sem suporte a mods.**

## 2. Conceito e história
O jogador sofre um acidente espacial e fica preso num planeta desconhecido. Para sobreviver: recolher recursos → abrigo → energia → computadores → programar máquinas → automatizar → explorar → investigar → **reconstruir a nave e escapar**.
- Escapar conclui a campanha mas **não termina o jogo**: continua-se a expandir, colonizar e explorar o espaço.
- Narrativa através de missões, instalações abandonadas, laboratórios, computadores, registos e descobertas opcionais.
- **Proibido:** civilizações inteligentes ativas, NPCs humanos sobreviventes, sociedades alienígenas diplomáticas. Os jogadores são a única civilização tecnológica ativa.

## 3. Programação real (mecânica central)
- Linguagens (reais, nunca pseudocódigo ou linguagem fictícia): **HTML, CSS, JavaScript, SQL, Python, PHP, TypeScript, C, C++, C#**.
- Filosofia (inspiração *The Farmer Was Replaced*): escrever código verdadeiro → controla máquinas reais no mundo → observar → depurar → otimizar → aprender experimentando.
- **API Codebound** própria, exposta a todas as linguagens (ex.: `drill.mine()`, `energy.available()`, `computer.on("resource_detected", cb)`). Não faz parte das bibliotecas standard.
- **Execução isolada** fora do processo do Unity: limites de CPU, memória e tempo; permissões; acesso controlado a máquinas/redes; erros geridos; interrupção segura; proteção contra loops infinitos.
- Modos: **contínuo** (ciclos) e **por eventos** (handlers), podem coexistir.
- Cada computador tem **CPU, RAM, armazenamento, rede e energia**; o hardware limita a execução. Programas ineficientes consomem recursos e causam problemas.
- Usos: perfuradoras, transportadores inteligentes, armazenamento automático, robôs mineiros, redes de energia, torres de defesa, alarmes, laboratórios, servidores, HUD personalizado, fábrica inteira.
- Papel típico das linguagens (orientativo): HTML/CSS → HUD e painéis; JavaScript/TypeScript → eventos e controlo de máquinas; Python → automação e robôs; SQL → armazéns e bases de dados logísticas; PHP → servidores/terminais comerciais; C/C++ → firmware de baixo nível e máquinas de alto desempenho; C# → sistemas avançados e servidores.

## 4. Editor de programação (in-game)
Editor de código, explorador de ficheiros, terminal, debugger (breakpoints, inspeção de variáveis), documentação, exemplos, diagramas visuais, monitor CPU/RAM, histórico de execução, erros/avisos compreensíveis.
- Assistência: **Guiado / Normal / Avançado**. Aprendizagem híbrida e adaptativa: missões educativas, documentação, exemplos, explicações, pistas contextualizadas, experimentação livre. Não dar sempre a solução completa.

## 5. HUD e interface
- HUD personalizável e parcialmente programável; o jogador começa por aprender **HTML e CSS** para construir a sua interface (vida, inventário, radar, energia, fábrica, alarmes, painéis de máquinas, estatísticas).
- Botão **"+"** no HUD cria caixas de código.
- Estrutura principal estável + painéis móveis, redimensionáveis, ocultáveis e reorganizáveis; layouts guardáveis.
- **Acessibilidade:** escala de texto e UI, modos daltonismo, contraste, legendas, indicadores visuais de sons, redução de flashes/partículas, volumes separados, remapeamento, navegação por teclado.

## 6. Mundo e exploração
- Mundo aberto procedural, praticamente infinito, **seeds partilháveis**.
- Biomas: florestas, desertos, zonas vulcânicas, ruínas tecnológicas, regiões radioativas.
- Dia/noite, clima dinâmico, estações, tempestades, eventos ambientais, recursos escondidos, criaturas alienígenas, instalações abandonadas.
- Subterrâneo: mineração, recursos raros, túneis, perfuração, ventilação, água, gases, temperaturas extremas.
- Transformação de terreno limitada (construção, limpeza localizada, mineração em zonas apropriadas).

## 7. Construção e indústria
- Grelha; máquinas multi-célula e rotativas; construção modular; pré-visualização; verificação de obstáculos; ligações; planeamento antes de gastar recursos.
- **Todas as máquinas precisam de um programa válido para funcionar.**
- Cadeias: mineração → fundição → processamento → componentes → circuitos → fabrico avançado → armazenamento → logística.
- Transportadores básicos, inteligentes e programáveis; robôs, drones, armazéns, servidores centrais, redes logísticas.

## 8. Recursos, energia e sistemas técnicos
- Recursos: ferro, cobre, carvão, silício, titânio, urânio, petróleo, cristais alienígenas.
- Energia: carvão, solar, eólica, combustíveis, nuclear, alienígena.
- Sistemas: redes elétricas, baterias, transformadores, subestações, disjuntores; tubagens, bombas, reservatórios, fluidos; telecomunicações (routers, switches, fibra, antenas), servidores, firewalls, segurança. Tudo automatizável e programável.

## 9. Sobrevivência, agricultura, ecologia
- Vida, fome, sede, oxigénio, temperatura — importante mas não excessivamente punitiva.
- Agricultura, estufas, hidroponia, aeroponia, plantas alienígenas, melhoramento genético, processamento alimentar, irrigação automática. **Sem pecuária/domesticação.**
- Ecologia reage à poluição do ar, água e solo; descontaminação, filtragem, reflorestação, recuperação.

## 10. Combate e ameaças
- Inimigos biológicos alienígenas e máquinas hostis; reagem a ruído, emissões, consumo energético, expansão industrial, tecnologia experimental.
- Armas, equipamentos, drones, torres, minas, barreiras, sistemas automatizados.
- Máquinas hostis atacam redes e comprometem máquinas → segurança informática: permissões, palavras-passe, firewalls, backups, deteção de intrusões.

## 11. Multiplayer
- Até **8 jogadores**; um jogador aloja (o seu computador é o servidor).
- Convites, permissões, construções privadas/públicas/de equipa, projetos partilhados, histórico de alterações, gestão de conflitos, progressão individual.
- Competências de programação são individuais. Quando todos saem, o mundo pausa.

## 12. Economia
Sem moeda obrigatória; trocas diretas. Terminais comerciais, ofertas/pedidos, contratos, entregas, recompensas em recursos, reputação, comércio automatizado por programação.

## 13. Investigação e progressão
- Sem nível geral nem XP geral; competências melhoram com uso e resolução de problemas.
- Investigação híbrida: linha principal (reconstrução da nave) + ramos opcionais.
- Áreas: programação, computação, indústria, robótica, energia, ciência, materiais, defesa, segurança, exploração, tecnologia espacial.
- Laboratórios universais e especializados, programáveis.

## 14. Espaço
Lançar a nave conclui a história e abre a fase espacial: naves modulares, sistemas de bordo programáveis, luas, asteroides, planetas, estações, colónias (só jogadores), sondas, viagens automáticas ou manuais. Combate espacial simples, sem gestão de grandes frotas.

## 15. Áudio e apresentação
Áudio adaptativo (exploração, construção, indústria, combate, investigação, subterrâneo, espaço, perigo); sons de máquinas por estado; áudio espacial; música adaptativa; alarmes programáveis. Pixel art coerente com animações, iluminação, partículas, clima.

## 16. Localização
pt-PT, pt-BR, EN, ES, FR, DE, IT, JA, ZH-Hans, KO. A sintaxe das linguagens de programação **nunca** é traduzida; menus, documentação, explicações e legendas são localizáveis.

## 17. Ordem de desenvolvimento
1. Fundação · 2. Primeiras máquinas · 3. Programação funcional · 4. Automação · 5. Mundo completo · 6. Multiplayer · 7. Espaço · 8. Polimento.
Detalhe e estado em `TASKS.md`.
