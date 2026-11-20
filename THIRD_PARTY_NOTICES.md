# Componentes de terceiros

Componentes redistribuídos com o Codebound (builds) ou incluídos no projeto. Antes de cada release, confirmar que estes ficheiros de licença seguem no build.

| Componente | Versão | Licença | Onde está | Origem / verificação |
|---|---|---|---|---|
| Node.js (runtime JavaScript/TypeScript) | 24.13.0 | MIT + avisos de terceiros (V8, ICU, OpenSSL, …) | Build: `Codebound_Data/StreamingAssets/Runtime/node/{node.exe,LICENSE}` · Projeto: `Codebound/Tools/node/LICENSE` | Binário da instalação local (`C:\Program Files\nodejs`). `LICENSE` descarregado em 2026-11-20 de `raw.githubusercontent.com/nodejs/node/v24.13.0/LICENSE` (143 310 bytes). |
| TypeScript (compilador, usado no runtime) | 5.9.3 | Apache-2.0 | `Codebound/Assets/StreamingAssets/Runtime/js/vendor/typescript/` (`LICENSE.txt`, `ThirdPartyNoticeText.txt`) | Copiado da instalação global local do npm. |
| CPython embeddable (runtime Python) | 3.15.2 (Windows x64) | PSF License (+ componentes listados no `LICENSE.txt`) | Projeto: `Codebound/Tools/python/` · Build: `Codebound_Data/StreamingAssets/Runtime/python/` (inclui `LICENSE.txt`) | Descarregado em 2026-11-20 de `python.org/ftp/python/3.15.2/python-3.15.2-embed-amd64.zip`. SHA-256 `d012f3aa29cf07436dae8aea1fec0fd195c2612a0b059b1f915ae0afb9ad1f62` — confere com a página oficial do release. |
| Unity 6 (motor) | 6000.6.10f1 | Unity Software Terms | — | Licença Unity Personal do utilizador. |

Não há outras dependências externas no código do jogo (JSON, ruído, sprites e protocolos são implementação própria).
