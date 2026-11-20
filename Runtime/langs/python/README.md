# Python runtime (CPython 3.12, wasm32-wasi)

Not committed (26 MB). Download into this folder:

```bash
curl -L -o python.wasm "https://github.com/vmware-labs/webassembly-language-runtimes/releases/download/python%2F3.12.0%2B20231211-040d5a6/python-3.12.0.wasm"
```

- Source: VMware Wasm Labs `webassembly-language-runtimes` (build scripts Apache-2.0); CPython under the PSF License.
- Licences must be re-checked and bundled with the game before release (see `TECHNICAL_ARCHITECTURE.md`).
- `WasmSandbox` caches the compiled native code as `python.wasm.cwasm` next to it (also not committed).
- Status: **not yet verified**. The tests in `Runtime/tests/Codebound.Runtime.Tests` were written but not run in Session 1 (see `NEXT_SESSION.md`).
