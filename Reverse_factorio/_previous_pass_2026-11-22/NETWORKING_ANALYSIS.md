# NETWORKING_ANALYSIS — Rede e multijogador

**Legenda:** [CONFIRMADO] · [INFERÊNCIA] · [HIPÓTESE] · [CONHECIMENTO PÚBLICO] = documentação/FFF da Wube, não verificado no binário.

## 1. Evidência no binário — [CONFIRMADO]
- Imports `WS2_32.dll` (30 funções, maioria por ordinal; `getaddrinfo`, `freeaddrinfo`, `WSAIoctl`, `WSAAddressToStringW`), `IPHLPAPI.DLL!GetIpAddrTable`, `DNSAPI.dll!DnsQuery_UTF8`.
- Strings: `UDPSocket.cpp`, `UDPSocket@@`, `UDPSocketInterface@@`, `UDP Socket failed: %s`.
- `34197` aparece 4× → [INFERÊNCIA] porta UDP por omissão (coincide com a porta documentada publicamente).
- `heartbeat`, `heartbeats_per_second` / `heartbeats-per-second`, `heartbeat(%u) that we don't have.`, `heartbeat(%s) behind the latency window`.
- `latency window`, `latency_in_ticks`, `latencyInputHandler`, `latency state. Latency tick=%u, actio...`, `latency update.`
- `desynchronisation: crc test (%s) failed for cr...`, `desync-report`, `desync on loading.`, `desync on loading: %u >= %zu.`
- `server-settings.json`.

## 2. Modelo de rede
[INFERÊNCIA forte] **Lockstep determinístico**: as strings "crc test failed" + "desync" + "desync on loading" mostram que cada par calcula um CRC do estado e compara — só faz sentido se todos simulam o mesmo estado a partir das mesmas ações.
[INFERÊNCIA] **Transporte UDP** próprio (classe `UDPSocket`), sem TCP para o jogo.
[INFERÊNCIA] **Heartbeats** são as mensagens periódicas por tick que transportam ações de input; "heartbeat que não temos" = pacote em falta → pedido de retransmissão.
[INFERÊNCIA] **Latency hiding**: `latencyInputHandler` + `latency state` → o cliente aplica localmente as próprias ações de forma provisória enquanto espera confirmação do servidor.
[CONHECIMENTO PÚBLICO] Factorio usa arquitetura cliente-servidor sobre lockstep (servidor ordena as ações) e, ao entrar, o cliente descarrega o mapa inteiro do servidor.

## 3. Fluxo provável (tick a tick)
```
Cliente: input → InputAction → heartbeat(tick N) ──UDP──► Servidor
Servidor: junta ações de todos → heartbeat ordenado(tick N) ──► todos
Todos: aplicam as mesmas ações no tick N → simulação determinística
Periodicamente: CRC do estado → comparar → se diferente: "desync" + desync-report
```
[INFERÊNCIA] — reconstruído a partir das strings, não de código desmontado.

## 4. Join / mapa
[INFERÊNCIA] `desync on loading` implica que, após transferência do mapa, o cliente carrega-o e verifica CRC contra o servidor.
[HIPÓTESE] A transferência do mapa usa o mesmo formato `.zip` do save (ver SAVE_SYSTEM.md).

## 5. Serviços online
[CONFIRMADO] Imports de TLS/certificados (CRYPT32, WINTRUST) e LDAP/DNS.
[INFERÊNCIA] Usados para HTTPS (portal de mods, matchmaking, autenticação factorio.com).
[HIPÓTESE] Cliente HTTP do tipo libcurl estático (padrão WLDAP32+DNSAPI típico); string `libcurl/` não encontrada.
Não foram investigados tokens/credenciais (restrição do utilizador).

## 6. Lua e rede
[CONFIRMADO] O código Lua (ex. `freeplay.lua`) usa `storage` e eventos; não acede a sockets.
[INFERÊNCIA] Todo o estado de mods corre dentro do lockstep: o Lua tem de ser determinístico (por isso `math.random` é substituído/semeado pelo motor — [CONHECIMENTO PÚBLICO]).

## 7. Não verificado
- Formato dos pacotes, tamanho do MTU, cifragem do tráfego de jogo.
- Algoritmo do CRC (string `crc32` **não** encontrada).
- Compressão do tráfego (ZSTD presente, uso não ligado à rede).
Próximo passo: PDB + xrefs para `UDPSocket.cpp` e captura pcap de uma sessão local (com consentimento).

## Evidência adicional (scan 2026-11-23)
Fonte: `EVIDENCE_SCAN.txt` (scan só de leitura, Python/mmap; REA indisponível — `provider_unavailable`). Recursos PE: Evidence ID `ev_a9903e7c31d7f73ee72e27699d064657da6163bd680018c8b711ce595e569db6`.
- [CONFIRMADO] String `Multiplayer desynchronisation: crc test (%s) failed for crcTick(%s) serverCRC(%u) localCRC(%u)` @0x21e3c9c; também `Map desynchronisation: tick %s. Replay crc`.
- [CONFIRMADO] PDB contém `ParallelDesyncReportCreator::doStart` e `desyncReportPath`.
- [CONHECIMENTO PÚBLICO] Porta UDP por omissão 34197 — não verificada no binário.
