# Codebound

**BUILD. CODE. SURVIVE. EXPLORE.**

Jogo de sobrevivência, automação industrial e exploração num planeta desconhecido, em que **a programação real é a mecânica central**: o jogador escreve código verdadeiro (HTML, CSS, JavaScript, SQL, Python, PHP, TypeScript, C, C++, C#) para controlar máquinas, robôs e redes. Pixel art, top-down, construção em grelha, até 8 jogadores em cooperação. Unity, para Windows, Linux e macOS.

- Especificação do jogo: [`GAME_DESIGN.md`](GAME_DESIGN.md)
- Arquitetura: [`TECHNICAL_ARCHITECTURE.md`](TECHNICAL_ARCHITECTURE.md)
- Estado e tarefas: [`TASKS.md`](TASKS.md) · Registo: [`DEVELOPMENT_LOG.md`](DEVELOPMENT_LOG.md) · Retomar: [`NEXT_SESSION.md`](NEXT_SESSION.md)

## Estado

Em desenvolvimento, **Etapa 1 (Fundação)**. A lógica está testada fora do Unity; o jogo **ainda não foi aberto nem testado no editor Unity**.

## Abrir e jogar (Unity)

1. Instala o **Unity Hub** e um **Unity 6 LTS (6000.0.x)**. Se o Hub pedir outra versão 6000.0.x, aceita.
2. No Hub: *Add → Add project from disk* → escolhe a pasta **`Codebound/`** (a pasta dentro do repositório, não a raiz).
3. Abre o projeto. O Unity resolve os pacotes de `Packages/manifest.json`.
4. Menu **Codebound → Create Test Scene** (cria `Assets/_Codebound/Scenes/TestWorld.unity`) e carrega em **Play**.
   - Em alternativa, carrega em Play em qualquer cena: o `GameBootstrap` cria-se sozinho.
5. Se aparecer um aviso sobre o *Input System* a pedir para reiniciar, aceita. O jogo funciona com o sistema novo e com o antigo.

### Controlos (Etapa 1)

| Ação | Tecla |
|---|---|
| Mover | WASD / setas |
| Recolher recurso / abrir caixa | **E** ou clique esquerdo (manter para continuar a recolher) |
| Modo de construção | **B** (ou clicar numa estrutura no painel) |
| Escolher estrutura | **1–4** |
| Rodar | **R** |
| Construir / desmontar | clique esquerdo / clique direito (no modo de construção) |
| Zoom | roda do rato |
| Inventário | **Tab** |
| Fechar / sair do modo | **Esc** |
| Idioma (pt-PT / en) | **F1** |

Começa a recolher **sucata** à volta da nave despenhada, **madeira** das árvores e **pedra**. Constrói paredes, chão, caixas de armazenamento e módulos de abrigo.

## Verificação sem Unity

Pré-requisito: .NET SDK 8.

```bash
cd Tools/LogicTests
dotnet test
```

```bash
mkdir -p /tmp/unityref && cd /tmp/unityref && curl -sSL -o m.nupkg https://api.nuget.org/v3-flatcontainer/unityengine.modules/2021.3.33/unityengine.modules.2021.3.33.nupkg && unzip -qo m.nupkg -d pkg
```

```bash
cd Tools/UnityCompileCheck
dotnet build
```

(Opcional: `UNITY_REF_DIR=<pasta com as DLLs> dotnet build`.) Estas DLLs servem só para verificar a compilação; não são incluídas no repositório nem no jogo.

```bash
cd Tools/WorldPreview
dotnet run -- 2026 world.ppm
```

## Licença

Por definir pelo proprietário do projeto.
