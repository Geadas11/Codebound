# PROGRESS — estado da investigação (2026-11-24)

## Estado
- 24/24 documentos obrigatórios + **GHIDRA_FINDINGS.md** (descompilação).
- Perguntas abertas: 15 resolvidas e 2 abertas, de 17 (ver UNKNOWN_COMPONENTS.md). As 2 abertas exigem correr o jogo.

## Ferramentas e estado do ambiente
- Ghidra 12.1.4 em `C:\Users\carlos\Tools\ghidra_12.1.4_PUBLIC`; servidor MCP `rea` configurado com `GHIDRA_INSTALL_DIR`/`JAVA_HOME` (backup `~/.claude.json.bak-before-ghidra-20261123`).
- `ghidra/` (1.º projeto): análise com tipos PDB, **encravada** em `removeUnnecessaryPadding` desde 21:11. **Parada a 2026-11-24 às 01:40**, a pedido do utilizador. Não gravou nada, por isso o projeto `ghidra/` ficou sem análise útil.
- `ghidra2/` (2.º projeto): completo. Para descompilar mais funções:
  `analyzeHeadless <dir>/ghidra2 factorio_fast -process factorio.exe -noanalysis -readOnly -scriptPath scripts/ghidra -postScript DecompileList.java <lista.txt> artifacts/decomp`
- Scripts só de leitura: `pdb_dump.py`, `pe_dump.py`, `calls.py`, `callers.py`, `layout.py`, `ghidra/*.java`.

## Próximos passos (se continuar)
1. REA: **feito a 2026-11-24**, com 7 Evidence exportadas para `artifacts/rea_evidence_bundle.json` (REA_FINDINGS §6). Histórico: a 1.ª tentativa falhou por timeout. O REA dá ao Ghidra 330 s para importar e analisar (`REA_GHIDRA_STARTUP_TIMEOUT_MS`), e este exe precisa de mais de 35 min (e de horas se o analisador PDB estiver ligado). Para repetir: acrescentar ao `env` do servidor `rea` em `~/.claude.json` `REA_GHIDRA_STARTUP_TIMEOUT_MS` (por exemplo 7200000) e `REA_GHIDRA_DISABLED_ANALYZERS` (por exemplo `PDB Universal,Decompiler Parameter ID`), reconectar o `rea` e reabrir. O timeout do próprio cliente MCP também pode cortar.
2. Com o Ghidra só restam refinamentos opcionais: comparar `luaV_execute` instrução a instrução com o Lua 5.2.4 oficial (precisa da fonte oficial, que não existe neste PC).
3. #14/#17 exigem correr uma cópia isolada do jogo, só com autorização.
