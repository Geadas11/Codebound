# ONLINE_PLAN — Contas, social e serviços online (missão de 2026-11-24)

## 1. Auditoria do multiplayer e da infraestrutura atual
| Área | Estado encontrado no código | Consequência |
|---|---|---|
| Transporte | TCP direto ao anfitrião (`Game/NetSession.cs`, porta fixa `Multiplayer.Port` = 47820), JSON por linha | Na internet só funciona com porta aberta (UPnP opcional em `RouterPort`) |
| Cifra | `SecureChannel`: troca de chaves + palavra-passe do mundo; tudo cifrado depois do handshake | Mantém-se: um relay futuro só veria texto cifrado |
| Autoridade | Anfitrião autoritativo; convidados enviam ações validadas em `Core/Multiplayer.cs`; instantâneos 2×/s | Mantém-se |
| Identidade | O convidado escolhe o **nome**; o anfitrião guarda corpo e inventário **por nome** | Qualquer pessoa pode usar o nome de outra e ficar com o inventário dela — resolvido com bilhetes de entrada ligados à conta |
| Gestão | Papéis (visitante/construtor/admin), expulsar, máx. 8 convidados | Mantém-se; expulsão passa a ficar registada no serviço |
| Descoberta | Nenhuma (escrever o IP) | Novo: registo de sessões no serviço |
| Reconexão | Não automática (o convidado volta a entrar com o mesmo nome) | Novo: reconexão automática com novo bilhete |
| Contas, perfis, amigos, estatísticas, cloud | Nada | Tudo novo |
| Menus | `TitleMenu` (IMGUI), `OptionsMenu`, painel N de multijogador | Integração do ecrã de conta nestes menus |
| Transferência de autoridade | Impossível sem enviar o mundo inteiro para outro PC | **Não suportada** (documentado) |
| Servidores dedicados | O jogo corre o mundo dentro do cliente Unity | Não há servidor dedicado; o serviço online não corre mundos |

## 2. Escolha do backend
| Opção | Contas com email + verificação + recuperação | Amigos / presença / convites | Cloud saves / estatísticas | Validação no servidor | Custo / dependências | Testável aqui sem credenciais |
|---|---|---|---|---|---|---|
| Unity Gaming Services | Só utilizador/palavra-passe e plataformas (sem verificação de email própria) | Friends, Lobby | Cloud Save, Leaderboards | Cloud Code (limites do plano gratuito) | Conta Unity + projeto ligado; relay pago acima do gratuito | Não |
| PlayFab | Sim (modelos de email) | Amigos, sem presença real | Sim | CloudScript | Gratuito em desenvolvimento, pago em produção | Não (precisa de Title ID) |
| Firebase | Sim | Firestore + regras | Firestore/Storage | Cloud Functions só no plano pago (Blaze) | Google; SDK Unity pesado | Não |
| Supabase | Sim | Postgres + RLS | Sim | Funções Deno | Plano gratuito pausa por inatividade | Não (sem SDK Unity oficial) |
| Nakama (open source) | Email sem verificação própria | Amigos, presença, matchmaker | Storage, leaderboards | Módulos Go/Lua/TS | Docker + Postgres/CockroachDB | Não (sem Docker nesta máquina) |
| **Serviço próprio (ASP.NET Core 8 + SQLite)** | Implementado por nós | Implementado | Implementado | Total | Grátis; um VPS pequeno em produção (≈ 4–6 €/mês) + SMTP | **Sim**: o SDK .NET 8 e o runtime ASP.NET Core 8 já estão instalados (Unity) |

**Escolha: serviço próprio** (`Services/`), porque é o único que se pode construir, testar e verificar por completo nesta máquina, sem contas externas, sem custos recorrentes obrigatórios, com validação total no servidor e no mesmo idioma do jogo (C#). Custo: a segurança é responsabilidade nossa — por isso usa só primitivas da biblioteca padrão (.NET 8: PBKDF2, `RandomNumberGenerator`, SHA-256, `RateLimiter`) e tem testes de cada regra. SQLite chega para um servidor único (WAL, cópias de segurança online); a camada de dados está num só ficheiro, para trocar por PostgreSQL quando for preciso escalar.

## 3. Arquitetura
- `Services/Codebound.Services` — API HTTP/JSON (HTTPS em produção, atrás de um proxy com TLS; HTTP só em `localhost` no desenvolvimento). Dependência única: `Microsoft.Data.Sqlite`.
- `Services/Codebound.Services.Tests` — testes de integração no próprio processo (o serviço arranca numa porta livre; relógio controlável para testar expirações).
- Cliente Unity, lógica sem motor (testável): `Core/Online/OnlineClient.cs` (HTTP com transporte trocável, renovação automática de tokens, falhas de rede = "offline", nunca exceções) e `Core/Online/OfflineSync.cs` (política de sessão em cache, fila de estatísticas, decisões dos saves na cloud).
- Cliente Unity, integração: `Game/Online.cs` (configuração do serviço, gravação de estatísticas, publicação de jogos, bilhetes, reconexão; `OnlineDriver` persistente entre cenas), `Game/AccountMenu.cs` (ecrãs), `Game/NetSession.cs` (bilhetes no handshake, banir, listar online), `TitleMenu`/`OptionsMenu` (entrada e estado).
- Ambientes: `Development` (localhost, emails escritos numa caixa de saída local) e `Production` (variáveis de ambiente; SMTP real; TLS obrigatório). Nenhum segredo no repositório nem na build Unity.

Ver `Services/README.md` para executar, configurar, migrações, logs, monitorização e cópias de segurança.

## 4. Registo por etapa
| Etapa | Feito | Verificação |
|---|---|---|
| Auditoria + arquitetura | Secções 1–3; serviço próprio escolhido | — |
| Autenticação | Registo (sem enumeração de emails), verificação por código, login com bloqueio progressivo, refresh com rotação e deteção de reutilização, logout, sessões por dispositivo, recuperação, mudança de palavra-passe/email, eliminação com retenção, exportação | 12 testes `AuthTests` |
| Perfis | Perfil público separado da conta; avatar, bio, visibilidade, pedidos, presença, consentimento de estatísticas; pesquisa com limite | `SocialTests` |
| Amigos e presença | Pedidos (cruzados aceitam-se), aceitar/recusar/cancelar, remover, bloquear (apaga amizade, pedidos, convites, favoritos), denúncias, presença online/ocupado/em jogo/offline | 5 testes `SocialTests` |
| Convites e sessões | Registo de jogos (público/amigos/privado), heartbeat, lista, bilhetes de uso único verificados pelo anfitrião, convites com expiração só entre amigos, expulsar/banir, saída, histórico, favoritos | 5 testes `SessionTests` |
| Integração no multijogador | Jogo listado ⇒ o anfitrião só aceita convidados com bilhete e usa o nome **da conta** (acaba a usurpação por nome); jogos por IP/LAN sem conta continuam como antes; expulsão/ban/saída comunicados ao serviço; reconexão automática do convidado (3 bilhetes novos, 5/15/45 s); presença "em jogo" | `GameClientTests.OnlineGame_*` (código do jogo contra o serviço real); Play Mode: listar e deixar de listar |
| Estatísticas e conquistas | Lotes com id (repetir é seguro), limite por taxa × tempo não sobreposto, conquistas calculadas no servidor, estatísticas verificadas pelo próprio serviço (jogos, jogadores, amigos) | 6 testes `ProgressTests` |
| Offline e sincronização | Perfil local sem conta; sessão guardada só com o refresh token; vale offline até 30 dias; sem rede ⇒ estado "offline" sem terminar sessão; reconexão com backoff 15 s → 5 min, reautenticação e envio da fila | 9 testes `OnlineClientTests` (Unity) + `GameClientTests` (perda e regresso da rede) |
| Saves na cloud | Versões, SHA-256 verificado nos dois sentidos, conflito 409 em vez de sobrescrever, 5 versões guardadas; no jogo: nunca apaga um save local (cópia em `saves/backups` antes de substituir; "manter ambos" grava `<slot>-cloud`) e em conflito o jogador escolhe | `ProgressTests` + `OnlineClientTests` + `GameClientTests.CloudSaves_TwoPcs_*` |
| Interface | Painel de conta (iniciar sessão, registo, verificação, recuperação, serviço, perfil, amigos, pedidos, bloqueios, denúncias, jogos online, convites, favoritos, histórico, estatísticas e conquistas, saves na cloud, dispositivos, palavra-passe, email, exportar, eliminar, terminar sessão); estado da conta/ligação no ecrã inicial e no menu Esc; pedidos/convites pendentes no ecrã inicial; aviso de modo offline; 178 textos × 10 línguas | Play Mode (capturas) |
| Segurança, testes, docs | Ver abaixo | Serviço 34/34; Unity 223/223; build + self-test PASS |

### Verificação final
- Serviço: `dotnet test` → **34/34** (28 do serviço + 6 que compilam o código `Core/Online` do jogo e o usam contra o serviço real: sessão, renovação, reinício, dispositivo revogado, perda e regresso da rede, bilhetes/convites/ban, saves em dois PCs, serviço desligado).
- Unity: **223/223** EditMode (9 novos em `OnlineClientTests`); build Windows + self-test PASS (8 linguagens).
- Play Mode contra o serviço local: registo → código da caixa de saída de desenvolvimento → confirmação → login → amigos (200) → reinício do Play Mode com a sessão mantida → anfitrião lista o mundo para amigos → ao deixar de alojar, a listagem fecha (`DELETE /sessions`). Captura local (fora do git) em `Codebound/Screenshots/account_profile.png`.

### Segurança
- Palavras-passe: só PBKDF2-SHA256 (600 000 iterações) no servidor; o jogo nunca as guarda (campos limpos depois do envio; o ficheiro de sessão tem só o refresh token).
- Tokens aleatórios de 256 bits guardados só como SHA-256; acesso 15 min; refresh 30 dias com rotação e revogação por reutilização.
- O jogo só aceita `https`, exceto `http` para um serviço neste PC. Nenhum segredo na build: só o URL público (`-online <url>` ou o campo "Serviço…").
- O servidor valida tudo o que importa (permissões, bilhetes, conquistas, limites de estatísticas, versões de saves); limites de pedidos por IP e por conta.

### O que falta / não suportado
- **Não há servidor de produção**: não existe URL público nem SMTP configurados. Uma build de lançamento abre em modo offline ("nenhum serviço online definido"); em desenvolvimento usa `http://127.0.0.1:5080`. Para pôr online: `Services/README.md` → Produção (VPS ≈ 4–6 €/mês, Caddy com TLS grátis, um fornecedor SMTP à escolha).
- Relay/NAT: o anfitrião continua a precisar da porta aberta (UPnP ou reencaminhamento); o serviço só faz descoberta e autorização.
- Transferência de autoridade/servidores dedicados: não suportados (o mundo vive no PC do anfitrião).
- Notificações em tempo real: o jogo consulta o serviço a cada 60 s.
- Não verificado ao vivo: a entrada de um segundo jogo com bilhete por TCP (o caminho do serviço está testado de ponta a ponta; o handshake com bilhete só foi compilado e revisto).
- O ficheiro da sessão (`persistentDataPath/online/signin-*.json`) não está cifrado pelo sistema operativo; contém apenas o refresh token, que pode ser revogado em "Conta → Dispositivos".
