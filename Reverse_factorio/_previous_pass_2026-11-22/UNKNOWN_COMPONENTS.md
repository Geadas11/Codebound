# UNKNOWN_COMPONENTS — O que ainda não se sabe

Lista honesta das lacunas. Cada linha diz o que falta e como resolver, sempre em modo só-leitura.

| # | Desconhecido | Porque importa | Como resolver |
|---|---|---|---|
| 1 | Conteúdo de `factorio.pdb` (completo ou só públicos?) | Dá nomes de ~100k funções e classes | `llvm-pdbutil dump -summary -publics` (cópia do PDB fora da pasta do jogo) |
| 2 | [PARCIAL: 140 exports] Lista de exports (3 500 bytes) | Pode revelar flags de GPU ou API interna | Parser PE: ler Export Directory |
| 3 | [PARCIAL: curl+Schannel] Cliente HTTP/TLS usado | Serviços online | Strings `curl_`, `Schannel`, xrefs a `CertGetCertificateChain` |
| 4 | Algoritmo do CRC de desync | Reconstrução do lockstep | PDB + xrefs a "crc test" |
| 5 | Formato binário de `level.dat*` e compressão | Save | Abrir **cópia** de um save de teste |
| 6 | [NÃO CONFIRMADO] Magic "FACO" | Citado antes sem prova | Mesmo que #5 |
| 7 | Ordem exata das fases do tick | Simulação | PDB (`Map::update`, `Surface::update`…) + decompilação |
| 8 | Representação de posições (ponto fixo?) | Física/determinismo | PDB: tipos `MapPosition`, `RealOrientation` |
| 9 | Pathfinding (inimigos, comboios) | Algoritmos | PDB + strings |
| 10 | Implementação das correias | Logística | PDB: `TransportLine` |
| 11 | Threads usadas na simulação | Performance | `SubmitThreadpoolWork` xrefs; análise dinâmica |
| 12 | Backend gráfico por omissão e seleção | Render | strings de `config.ini` (`graphics-api`?) + xrefs |
| 13 | [PARCIAL: cache de sprites] Uso de ZSTD (save? rede? cache de sprites?) | Compressão | xrefs a `ZSTD` |
| 14 | [RESOLVIDO] Pastas DLC (`space-age`, `quality`, `elevated-rails`) | Âmbito de conteúdo | `ls data\` |
| 15 | Contagem exata de itens/entidades | Pergunta do utilizador | Correr o jogo com `--dump-data` numa **pasta temporária** de escrita, ou parser Lua completo |
| 16 | [PARCIAL: Box2D ausente] Box2D/PhysX/Bullet | Confirmar ausência de física rígida | `search_strings` |
| 17 | [PARCIAL: válida até 2028-08-15] Validade da assinatura Authenticode (12 240 bytes) | Integridade | `Get-AuthenticodeSignature` (só leitura) |
| 18 | [PARCIAL: Lua 5.2] Versão exata do Lua e modificações | API Lua | strings `$LuaVersion`, comparação com Lua 5.2.1 |
| 19 | [PARCIAL: título `Factorio`] `ChromaAppInfo.xml` | Periféricos | Ler ficheiro (trivial) |
| 20 | Pastas de dados do utilizador (`%APPDATA%\Factorio`) | Config/mods/saves reais | Só listar, nunca alterar |

## Componentes fora do âmbito
- Hydra launcher: credenciais, tokens e dados privados **não** foram nem serão investigados.
- Servidores da Wube: nenhum tráfego foi gerado.

## Evidência adicional (scan 2026-11-23)
Fonte: `EVIDENCE_SCAN.txt` (scan só de leitura, Python/mmap; REA indisponível — `provider_unavailable`). Recursos PE: Evidence ID `ev_a9903e7c31d7f73ee72e27699d064657da6163bd680018c8b711ce595e569db6`.
- #14: resolvida. Parciais: #2 (140 exports listados), #3 (curl+Schannel), #13 (ZSTD no cache de sprites), #16 (sem Box2D), #17 (assinatura válida), #18 (Lua 5.2), #19 (Chroma `Factorio`). #6 (`FACO`) continua **não confirmado**.
- As restantes lacunas continuam abertas; a análise **não** está completa.
