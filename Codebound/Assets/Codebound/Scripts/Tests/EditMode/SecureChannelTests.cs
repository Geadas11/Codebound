using System;
using System.Security.Cryptography;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class SecureChannelTests
    {
        static (SecureChannel host, SecureChannel guest) Pair(string hostPassword, string guestPassword)
        {
            var host = new SecureChannel(); var guest = new SecureChannel(); var salt = SecureChannel.NewSalt();
            host.Complete(guest.Public, hostPassword, salt, true, host.Public, guest.Public);
            guest.Complete(host.Public, guestPassword, salt, false, host.Public, guest.Public);
            return (host, guest);
        }

        [Test]
        public void SamePassword_TalksBothWays_AndHidesTheText()
        {
            var (h, g) = Pair("sesame", "sesame");
            string sealedMsg = g.Seal("{\"t\":\"chat\",\"text\":\"segredo\"}");
            StringAssert.DoesNotContain("segredo", sealedMsg);
            Assert.AreEqual("{\"t\":\"chat\",\"text\":\"segredo\"}", h.Open(sealedMsg));
            Assert.AreEqual("olá ✓", g.Open(h.Seal("olá ✓")));
            Assert.AreNotEqual(g.Seal("x"), g.Seal("x"), "random IV per message");
        }

        [Test]
        public void WrongPassword_TamperingReplayAndReflection_AreRejected()
        {
            var (h, g) = Pair("sesame", "nope");
            Assert.Throws<CryptographicException>(() => h.Open(g.Seal("hello")), "wrong world password");

            (h, g) = Pair("", "");
            string m1 = g.Seal("one"), m2 = g.Seal("two");
            var bytes = Convert.FromBase64String(m1); bytes[20] ^= 1;
            Assert.Throws<CryptographicException>(() => h.Open(Convert.ToBase64String(bytes)), "changed byte");
            Assert.Throws<CryptographicException>(() => h.Open(m2), "out of order");
            Assert.AreEqual("one", h.Open(m1));
            Assert.Throws<CryptographicException>(() => h.Open(m1), "replayed");
            Assert.AreEqual("two", h.Open(m2));
            Assert.Throws<CryptographicException>(() => h.Open(h.Seal("mine")), "own message sent back");
            Assert.Throws<CryptographicException>(() => h.Open("{\"t\":\"chat\"}"), "plain text");
        }

        [Test]
        public void TrivialPublicValues_AreRefused()
        {
            var c = new SecureChannel();
            var one = new byte[SecureChannel.KeyBytes]; one[one.Length - 1] = 1;
            Assert.Throws<CryptographicException>(() => c.Complete(one, "", SecureChannel.NewSalt(), true, c.Public, one));
            Assert.Throws<CryptographicException>(() => c.Complete(new byte[10], "", SecureChannel.NewSalt(), true, c.Public, new byte[10]));
        }
    }
}
