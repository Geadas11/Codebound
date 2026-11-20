# Próxima sessão
_Atualizado: 2026-11-20 (Sessão 1)_

## Estado atual
- Etapa em curso: **Etapa 1 (Fundação)** concluída na lógica; **Etapa 3** antecipada (sandbox) iniciada.
- Funciona e foi verificado:
  - Lógica de Core, Items, Inventory, World, Building, Player: 42 testes passam (`Tools/LogicTests`).
  - O código Unity (`Scripts/Game`) compila contra UnityEngine 2021.3 (`Tools/UnityCompileCheck`).
- Existe mas não foi verificado:
  - Projeto Unity nunca aberto no editor Unity 6 (pacotes, asmdefs, Input System, cena, jogabilidade).
  - `Runtime/host/Codebound.Runtime.Sandbox` + `Runtime/tests/Codebound.Runtime.Tests`: nem compilado nem executado.

## Fazer a seguir (por ordem)
1. **Ambiente:** instalar o .NET 8 se faltar (`apt-get install -y dotnet-sdk-8.0 sqlite3`; o script `dot.net` está bloqueado pelo proxy).
2. **Sandbox (precisa de autorização do utilizador):** na Sessão 1, o verificador do modo automático recusou correr os testes do sandbox. Pedir ao utilizador que permita esta ação (ou que a corra), depois:
   - `curl -L -o Runtime/langs/python/python.wasm <URL em Runtime/langs/python/README.md>`
   - `cd Runtime/tests/Codebound.Runtime.Tests && dotnet test`
   - Corrigir até passar: execução normal, stdin, exceção → Failed, loop infinito parado por fuel e por tempo, limite de memória, raiz sem diretórios do anfitrião, custo de CPU proporcional ao trabalho.
   - Critério de concluído: 8/8 testes passam; registar os tempos.
3. **API Codebound no sandbox:** canal de pedidos/respostas entre o programa e o host. Opções a avaliar por esta ordem:
   (a) importações de funções do host (requer runtime compilado com essas importações, por exemplo QuickJS-ng com wasi-sdk);
   (b) protocolo por linhas em stdin/stdout com FIFO ou implementação própria de `fd_read`.
   Primeira API: `drill.mine()`, `energy.available()`, `computer.on(event, fn)`. Ficheiros: `Runtime/api/` (esquema), `Runtime/host/`.
4. **Etapa 2 (lógica, testável sem Unity):** módulo `Machines` no `SimulationClock` (estado, inventários de entrada e saída), `Drill` 2x2 sobre minério, módulo `Energy` (gerador a carvão, rede, falta de energia), `Computer` (CPU, RAM, armazenamento). Regra: uma máquina sem programa válido não funciona.
5. Quando houver Unity: abrir `Codebound/` no Unity 6, menu *Codebound → Create Test Scene*, Play, corrigir erros, migrar para URP 2D e registar o resultado.

## Bloqueios e limitações conhecidas
- Sem editor Unity no ambiente da cloud: a integração Unity só é verificada por compilação (API 2021.3).
- Execução do sandbox/Wasm bloqueada pelo modo automático nesta sessão (precisa de decisão do utilizador).
- A pesquisa de pacotes NuGet (`azuresearch`) está bloqueada; os downloads diretos de `api.nuget.org` e dos *releases* do GitHub funcionam.

## Como verificar rapidamente o estado
```bash
cd Tools/LogicTests && dotnet test
```
```bash
mkdir -p /tmp/unityref && cd /tmp/unityref && curl -sSL -o m.nupkg https://api.nuget.org/v3-flatcontainer/unityengine.modules/2021.3.33/unityengine.modules.2021.3.33.nupkg && unzip -qo m.nupkg -d pkg
```
```bash
cd Tools/UnityCompileCheck && dotnet build
```
```bash
cd Tools/WorldPreview && dotnet run -- 2026 /tmp/world.ppm
```
