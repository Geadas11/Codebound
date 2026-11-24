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
- Cliente Unity: `Game/Online/*` — chamadas HTTP fora do thread principal, perfil local, fila offline, sem nunca bloquear o jogo local.
- Ambientes: `Development` (localhost, emails escritos numa caixa de saída local) e `Production` (variáveis de ambiente; SMTP real; TLS obrigatório). Nenhum segredo no repositório nem na build Unity.

Ver `Services/README.md` para executar, configurar, migrações, logs, monitorização e cópias de segurança.

## 4. Registo por etapa
(preenchido no fim de cada etapa)
