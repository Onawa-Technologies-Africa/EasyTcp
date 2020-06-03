using System;
using System.Net;
using System.Text;
using EasyEncrypt2;
using EasyTcp.Encryption;
using EasyTcp.Encryption.Protocols.Tcp;
using EasyTcp3.ClientUtils;
using EasyTcp3.EasyTcpPacketUtils;
using EasyTcp3.Server;
using EasyTcp3.Server.ServerUtils;
using EasyTcp3.Test.EasyTcp;
using NUnit.Framework;

namespace EasyTcp3.Test.Encryption
{
    /// <summary>
    /// Tests for EasyTcp.Encryption
    /// </summary>
    public class EncryptAndDecrypt
    {
        [Test]
        public void TestEncryptionAndDecryption()
        {
            var encryption = new EasyEncrypt();

            var testData = "testData";
            var encrypted = encryption.Encrypt(testData);
            var decrypted = encryption.Decrypt(encrypted);
            Assert.AreEqual(testData, decrypted);
        }

        [Test]
        public void TestGenerateKey()
        {
            var key = EasyEncrypt.CreateKey("password", "test12345678", 256);
            var key2 = EasyEncrypt.CreateKey("password2", "test12345678", 256);
            Console.WriteLine(Convert.ToBase64String(key));
            Assert.AreEqual("criGtQT7ZrS7SoRMvaXK4yuRm0XtTUJA7937nIzXa9Q=", Convert.ToBase64String(key));
            Assert.AreNotEqual("criGtQT7ZrS7SoRMvaXK4yuRm0XtTUJA7937nIzXa9Q=", Convert.ToBase64String(key2));
        }

        [Test]
        public void TestGenerateKeyConstructor()
        {
            var encryption = new EasyEncrypt("password", "salt12345678");

            var testData = "testData";
            var encrypted = encryption.Encrypt(testData);
            var decrypted = encryption.Decrypt(encrypted);
            Assert.AreEqual(32, encryption.GetKey().Length);
            Assert.AreEqual(testData, decrypted);
        }

        [Test]
        public void TestEncryptingAndDecryptingMessage()
        {
            var encryption = new EasyEncrypt();

            var testData = "testData";
            var message = IEasyTcpPacket.From<Message>(Encoding.UTF8.GetBytes(testData));
            var encryptedMessage = IEasyTcpPacket.From<Message>(message.Data).Encrypt(encryption);
            var decryptedMessage = IEasyTcpPacket.From<Message>(encryptedMessage.Data).Decrypt(encryption);

            Assert.AreEqual(message.ToString(), decryptedMessage.ToString());
            Assert.AreNotEqual(message.ToString(), encryptedMessage.ToString());
        }
    }
}