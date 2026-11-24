namespace Codebound.Services;

/// <summary>
/// Numbered schema migrations, applied in order at start-up inside a transaction each; schema_version records the
/// last one. Never edit a published migration: add the next number.
/// </summary>
public static class Migrations
{
    public static readonly string[] All =
    {
        // 1: accounts (private), profiles (public), sessions and tokens, codes, audit
        @"
CREATE TABLE accounts(
  id INTEGER PRIMARY KEY,
  email TEXT UNIQUE COLLATE NOCASE,              -- NULL once the account is deleted
  email_verified INTEGER NOT NULL DEFAULT 0,
  password_hash TEXT,
  created_at INTEGER NOT NULL,
  deleted_at INTEGER,
  failed_logins INTEGER NOT NULL DEFAULT 0,
  locked_until INTEGER NOT NULL DEFAULT 0
);
CREATE TABLE profiles(
  account_id INTEGER PRIMARY KEY REFERENCES accounts(id) ON DELETE CASCADE,
  public_id TEXT NOT NULL UNIQUE,
  display_name TEXT NOT NULL UNIQUE COLLATE NOCASE,
  avatar INTEGER NOT NULL DEFAULT 0,
  bio TEXT NOT NULL DEFAULT '',
  visibility TEXT NOT NULL DEFAULT 'public',     -- public | friends | private
  allow_requests TEXT NOT NULL DEFAULT 'everyone', -- everyone | nobody
  show_presence INTEGER NOT NULL DEFAULT 1,
  stats_consent INTEGER NOT NULL DEFAULT 1,      -- may the service keep play statistics
  status TEXT NOT NULL DEFAULT 'online',         -- what the player chose: online | busy
  last_seen INTEGER NOT NULL DEFAULT 0,
  current_session TEXT,
  created_at INTEGER NOT NULL
);
CREATE TABLE auth_sessions(                        -- one per signed-in device (a refresh token family)
  id TEXT PRIMARY KEY,
  account_id INTEGER NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,
  device TEXT NOT NULL,
  created_at INTEGER NOT NULL,
  last_used INTEGER NOT NULL,
  ip TEXT,
  revoked_at INTEGER
);
CREATE TABLE refresh_tokens(
  token_hash TEXT PRIMARY KEY,
  session_id TEXT NOT NULL REFERENCES auth_sessions(id) ON DELETE CASCADE,
  expires_at INTEGER NOT NULL,
  used_at INTEGER
);
CREATE TABLE access_tokens(
  token_hash TEXT PRIMARY KEY,
  session_id TEXT NOT NULL REFERENCES auth_sessions(id) ON DELETE CASCADE,
  account_id INTEGER NOT NULL,
  expires_at INTEGER NOT NULL
);
CREATE TABLE codes(
  id INTEGER PRIMARY KEY,
  account_id INTEGER NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,
  purpose TEXT NOT NULL,                         -- verify | reset | email
  code_hash TEXT NOT NULL,
  new_email TEXT,
  expires_at INTEGER NOT NULL,
  attempts INTEGER NOT NULL DEFAULT 0,
  used INTEGER NOT NULL DEFAULT 0
);
CREATE TABLE audit(id INTEGER PRIMARY KEY, account_id INTEGER, event TEXT NOT NULL, at INTEGER NOT NULL, ip TEXT);
CREATE INDEX audit_account ON audit(account_id, at);
",
        // 2: friends, blocks, reports
        @"
CREATE TABLE friend_requests(
  id INTEGER PRIMARY KEY,
  from_id INTEGER NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,
  to_id INTEGER NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,
  created_at INTEGER NOT NULL,
  UNIQUE(from_id, to_id)
);
CREATE TABLE friendships(                          -- one row per pair, a < b
  a INTEGER NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,
  b INTEGER NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,
  since INTEGER NOT NULL,
  PRIMARY KEY(a, b), CHECK(a < b)
);
CREATE TABLE blocks(
  blocker INTEGER NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,
  blocked INTEGER NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,
  created_at INTEGER NOT NULL,
  PRIMARY KEY(blocker, blocked)
);
CREATE TABLE reports(id INTEGER PRIMARY KEY, reporter INTEGER, target INTEGER, reason TEXT NOT NULL, created_at INTEGER NOT NULL);
",
        // 3: game sessions, join tickets, members (history and kicks), invites, favourites
        @"
CREATE TABLE game_sessions(
  id TEXT PRIMARY KEY,
  host_id INTEGER NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,
  name TEXT NOT NULL,
  visibility TEXT NOT NULL,                      -- public | friends | private
  max_players INTEGER NOT NULL,
  address TEXT NOT NULL,
  port INTEGER NOT NULL,
  version INTEGER NOT NULL,
  has_password INTEGER NOT NULL DEFAULT 0,
  players INTEGER NOT NULL DEFAULT 0,
  created_at INTEGER NOT NULL,
  heartbeat_at INTEGER NOT NULL,
  closed_at INTEGER
);
CREATE TABLE session_members(
  session_id TEXT NOT NULL REFERENCES game_sessions(id) ON DELETE CASCADE,
  account_id INTEGER NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,
  joined_at INTEGER NOT NULL,
  left_at INTEGER,
  banned INTEGER NOT NULL DEFAULT 0,
  PRIMARY KEY(session_id, account_id)
);
CREATE TABLE join_tickets(
  ticket_hash TEXT PRIMARY KEY,
  session_id TEXT NOT NULL REFERENCES game_sessions(id) ON DELETE CASCADE,
  account_id INTEGER NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,
  expires_at INTEGER NOT NULL,
  used_at INTEGER
);
CREATE TABLE invites(
  id INTEGER PRIMARY KEY,
  session_id TEXT NOT NULL REFERENCES game_sessions(id) ON DELETE CASCADE,
  from_id INTEGER NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,
  to_id INTEGER NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,
  created_at INTEGER NOT NULL,
  expires_at INTEGER NOT NULL,
  status TEXT NOT NULL DEFAULT 'pending'         -- pending | accepted | declined
);
CREATE TABLE favorites(account_id INTEGER NOT NULL REFERENCES accounts(id) ON DELETE CASCADE, host_id INTEGER NOT NULL REFERENCES accounts(id) ON DELETE CASCADE, PRIMARY KEY(account_id, host_id));
",
        // 4: statistics, achievements, cloud saves
        @"
CREATE TABLE stats(
  account_id INTEGER NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,
  key TEXT NOT NULL,
  value INTEGER NOT NULL,
  PRIMARY KEY(account_id, key)
);
CREATE TABLE stat_batches(
  account_id INTEGER NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,
  batch_id TEXT NOT NULL,
  period_start INTEGER NOT NULL,
  period_end INTEGER NOT NULL,
  received_at INTEGER NOT NULL,
  PRIMARY KEY(account_id, batch_id)
);
CREATE TABLE achievements(account_id INTEGER NOT NULL REFERENCES accounts(id) ON DELETE CASCADE, achievement TEXT NOT NULL, unlocked_at INTEGER NOT NULL, PRIMARY KEY(account_id, achievement));
CREATE TABLE cloud_saves(
  account_id INTEGER NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,
  slot TEXT NOT NULL,
  version INTEGER NOT NULL,
  sha256 TEXT NOT NULL,
  size INTEGER NOT NULL,
  client_time INTEGER NOT NULL,
  server_time INTEGER NOT NULL,
  device TEXT NOT NULL,
  data BLOB NOT NULL,
  PRIMARY KEY(account_id, slot, version)
);
",
    };

    public static void Apply(Db db)
    {
        using var c = db.Open();
        Db.Exec(c, "PRAGMA journal_mode = WAL;");
        Db.Exec(c, "CREATE TABLE IF NOT EXISTS schema_version(version INTEGER NOT NULL)");
        long have = Convert.ToInt64(Db.Scalar(c, "SELECT COALESCE(MAX(version), 0) FROM schema_version") ?? 0L);
        for (int i = (int)have; i < All.Length; i++)
        {
            using var tx = c.BeginTransaction();
            using (var cmd = Db.Cmd(c, All[i])) { cmd.Transaction = tx; cmd.ExecuteNonQuery(); }
            using (var cmd = Db.Cmd(c, "INSERT INTO schema_version(version) VALUES($v)", ("$v", i + 1))) { cmd.Transaction = tx; cmd.ExecuteNonQuery(); }
            tx.Commit();
        }
    }
}
