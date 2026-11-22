using System;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace Codebound.Core
{
    /// <summary>
    /// Encryption of the multiplayer connection. Each connection does a Diffie-Hellman exchange (RFC 3526 group 14, 2048 bits)
    /// and mixes the shared secret with the world password (PBKDF2), so a listener learns nothing and a man in the middle
    /// needs the password. The password itself never travels. Messages: AES-256-CBC + HMAC-SHA256 (encrypt-then-MAC) over a
    /// per-direction sequence number, so messages can't be read, changed, replayed or reordered.
    /// </summary>
    public sealed class SecureChannel
    {
        const string GroupHex =
            "00FFFFFFFFFFFFFFFFC90FDAA22168C234C4C6628B80DC1CD129024E088A67CC74020BBEA63B139B22514A08798E3404DD" +
            "EF9519B3CD3A431B302B0A6DF25F14374FE1356D6D51C245E485B576625E7EC6F44C42E9A637ED6B0BFF5CB6F406B7ED" +
            "EE386BFB5A899FA5AE9F24117C4B1FE649286651ECE45B3DC2007CB8A163BF0598DA48361C55D39A69163FA8FD24CF5F" +
            "83655D23DCA3AD961C62F356208552BB9ED529077096966D670C354E4ABC9804F1746C08CA18217C32905E462E36CE3B" +
            "E39E772C180E86039B2783A2EC07A28FB5C55DF06F4C52C9DE2BCBF6955817183995497CEA956AE515D2261898FA0510" +
            "15728E5A8AACAA68FFFFFFFFFFFFFFFF";
        static readonly BigInteger P = BigInteger.Parse(GroupHex, NumberStyles.HexNumber);
        public const int KeyBytes = 256, Iterations = 20000, Version = 3;

        readonly BigInteger priv;
        public readonly byte[] Public;
        byte[] encOut, macOut, encIn, macIn;
        ulong seqOut, seqIn;
        public bool Ready => encOut != null;

        public SecureChannel()
        {
            var r = new byte[32];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(r);
            priv = new BigInteger(r, isUnsigned: true, isBigEndian: true);
            Public = Fixed(BigInteger.ModPow(2, priv, P));
        }

        public static byte[] NewSalt() { var s = new byte[16]; using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(s); return s; }

        static byte[] Fixed(BigInteger v)
        {
            var b = v.ToByteArray(isUnsigned: true, isBigEndian: true);
            var o = new byte[KeyBytes];
            Buffer.BlockCopy(b, 0, o, KeyBytes - b.Length, b.Length);
            return o;
        }

        /// <summary>Derives the keys once the other side's public value is known. The host and the guest get mirrored keys.</summary>
        public void Complete(byte[] other, string password, byte[] salt, bool isHost, byte[] hostPublic, byte[] guestPublic)
        {
            if (other == null || other.Length != KeyBytes || salt == null || salt.Length != 16) throw new CryptographicException("bad handshake");
            var o = new BigInteger(other, isUnsigned: true, isBigEndian: true);
            if (o <= 1 || o >= P - 1) throw new CryptographicException("bad public value");   // rejects the trivial subgroup
            byte[] shared = Fixed(BigInteger.ModPow(o, priv, P));
            byte[] pw;
            using (var kdf = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(password ?? ""), salt, Iterations, HashAlgorithmName.SHA256)) pw = kdf.GetBytes(32);
            byte[] master;
            using (var h = new HMACSHA256(pw))
            {
                var t = new byte[shared.Length + hostPublic.Length + guestPublic.Length];
                Buffer.BlockCopy(shared, 0, t, 0, shared.Length);
                Buffer.BlockCopy(hostPublic, 0, t, shared.Length, hostPublic.Length);
                Buffer.BlockCopy(guestPublic, 0, t, shared.Length + hostPublic.Length, guestPublic.Length);
                master = h.ComputeHash(t);
            }
            byte[] K(string label) { using (var h = new HMACSHA256(master)) return h.ComputeHash(Encoding.ASCII.GetBytes(label)); }
            byte[] eh = K("codebound enc host->guest"), mh = K("codebound mac host->guest"), eg = K("codebound enc guest->host"), mg = K("codebound mac guest->host");
            if (isHost) { encOut = eh; macOut = mh; encIn = eg; macIn = mg; } else { encOut = eg; macOut = mg; encIn = eh; macIn = mh; }
        }

        public string Seal(string text)
        {
            var iv = new byte[16];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(iv);
            byte[] ct;
            using (var aes = Aes.Create())
            {
                aes.Key = encOut; aes.IV = iv; aes.Mode = CipherMode.CBC; aes.Padding = PaddingMode.PKCS7;
                using (var e = aes.CreateEncryptor()) { var p = Encoding.UTF8.GetBytes(text); ct = e.TransformFinalBlock(p, 0, p.Length); }
            }
            var mac = Mac(macOut, seqOut++, iv, ct);
            var o = new byte[16 + ct.Length + 32];
            Buffer.BlockCopy(iv, 0, o, 0, 16); Buffer.BlockCopy(ct, 0, o, 16, ct.Length); Buffer.BlockCopy(mac, 0, o, 16 + ct.Length, 32);
            return Convert.ToBase64String(o);
        }

        /// <summary>Decrypts one line. Throws CryptographicException if it was not sealed by the other side with the same keys, in order.</summary>
        public string Open(string line)
        {
            byte[] b;
            try { b = Convert.FromBase64String(line); } catch (FormatException) { throw new CryptographicException("not an encrypted message"); }
            if (b.Length < 16 + 16 + 32 || (b.Length - 48) % 16 != 0) throw new CryptographicException("bad length");
            int ctLen = b.Length - 48;
            var iv = new byte[16]; var ct = new byte[ctLen];
            Buffer.BlockCopy(b, 0, iv, 0, 16); Buffer.BlockCopy(b, 16, ct, 0, ctLen);
            var mac = Mac(macIn, seqIn, iv, ct);
            int diff = 0;
            for (int i = 0; i < 32; i++) diff |= mac[i] ^ b[16 + ctLen + i];   // constant time
            if (diff != 0) throw new CryptographicException("message rejected (wrong world password or tampering)");
            seqIn++;
            using (var aes = Aes.Create())
            {
                aes.Key = encIn; aes.IV = iv; aes.Mode = CipherMode.CBC; aes.Padding = PaddingMode.PKCS7;
                using (var d = aes.CreateDecryptor()) return Encoding.UTF8.GetString(d.TransformFinalBlock(ct, 0, ct.Length));
            }
        }

        static byte[] Mac(byte[] key, ulong seq, byte[] iv, byte[] ct)
        {
            using (var h = new HMACSHA256(key))
            {
                var s = BitConverter.GetBytes(seq);
                if (BitConverter.IsLittleEndian) Array.Reverse(s);
                h.TransformBlock(s, 0, 8, null, 0);
                h.TransformBlock(iv, 0, 16, null, 0);
                h.TransformFinalBlock(ct, 0, ct.Length);
                return h.Hash;
            }
        }
    }
}
