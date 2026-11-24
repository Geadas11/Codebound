# ARCHITECTURE_DIAGRAMS — Diagramas

**Legenda:** caixas marcadas `(C)` = confirmadas por evidência local, `(I)` = inferência, `(H)` = hipótese.

## 1. Visão geral do processo
```
                     Hydra launcher (fora do âmbito)
                               │ lança
                               ▼
┌──────────────────────── factorio.exe (C) PE32+ x64, 49 MB, 20 DLLs import. ────────────────────────┐
│                                                                                                     │
│  Plataforma Win32 (C)        Lua 5.2 embutido (C)          Simulação (I)          Rede (C)          │
│  USER32/GDI32/IMM32          ├─ fase settings (I)          ├─ entidades           UDPSocket (C)     │
│  RawInput, IME               ├─ fase data  → data.raw (I)  ├─ correias/comboios   porta 34197 (I)   │
│  ThreadPool (C)              └─ fase control → eventos (C) ├─ redes/fluídos       heartbeats (C)    │
│                                                             └─ inimigos            CRC/desync (C)    │
│  Render (C, carregado dinamicamente)    Save (C strings)       Serviços online (I)                  │
│  D3D11*│ OpenGL(WGL)*│ Vulkan*          .zip: level.dat,       HTTPS/TLS via CRYPT32 (I)            │
│                                         level-init.dat,        cliente HTTP estático (H)            │
│  Periféricos: Chroma (C), Steam (C)     script.dat             ZSTD (C)                             │
└─────────────────────────────────────────────────────────────────────────────────────────────────────┘
                               │ lê
                               ▼
         data\core (C)  ── lualib, prototypes, graphics, locale, fonts, sound
         data\base (C)  ── 2.1.24, prototypes\entity (50 ficheiros), script\freeplay
```

## 2. Pipeline de carregamento de mods (I)
```
mod-list.json (C) ─► ordenar mods por dependências (info.json) (I)
   └─► settings*.lua ─► data.lua ─► data-updates.lua ─► data-final-fixes.lua
         └─► data.raw ─► motor converte em protótipos C++ (I) ─► CRC de protótipos (H)
              └─► carregar save / novo mapa ─► control.lua de cada mod ─► on_init / on_load
```

## 3. Loop de jogo multijogador (I)
```
 Input local ─► InputAction ─► heartbeat(t) ──► Servidor ──► heartbeat ordenado(t) ──► todos
                     ▲                                                                   │
                     └──────── latency hiding (latencyInputHandler) ◄──────────── aplicar ações
                                                                                         │
                                                       update simulação(t) ─► eventos Lua ─► CRC
```

## 4. Eventos em freeplay (C)
```
control.lua ─► event_handler
                 ├─ freeplay        (on_init, on_configuration_changed, eventos de jogador)
                 └─ space-age ativo? ─sim─► space-finish-script
                                     └não─► silo-script
```

## 5. Save (I)
```
save.zip
 ├─ level.dat*      estado C++ serializado
 ├─ level-init.dat  estado inicial
 ├─ script.dat      storage Lua de cada mod
 └─ control.lua + ficheiros do cenário
```
