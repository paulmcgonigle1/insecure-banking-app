using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;
namespace SSD_Assignment___Banking_Application
{
	public static class EncrpytionService
	{
        // Encrypts plaintext data using the provided AES instance.
        public static byte[] Encrypt(byte[] plaintextData, Aes aes)
        {
            using (var msEncrypt = new MemoryStream())
            using (var encryptor = aes.CreateEncryptor())
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                csEncrypt.Write(plaintextData, 0, plaintextData.Length);
                csEncrypt.FlushFinalBlock();
                return msEncrypt.ToArray();
            }
        }

        // Decrypts ciphertext data using the provided AES instance.
        public static byte[] Decrypt(byte[] ciphertextData, Aes aes)
        {
            using (var msDecrypt = new MemoryStream(ciphertextData))
            using (var decryptor = aes.CreateDecryptor())
            using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            {
                var plaintextData = new byte[ciphertextData.Length];
                var bytesRead = csDecrypt.Read(plaintextData, 0, plaintextData.Length);
                Array.Resize(ref plaintextData, bytesRead);
                return plaintextData;
            }
        }

        // Creates and configures an AES instance with a random key and IV.
        public static Aes CreateAesInstance()
        {
            var aes = Aes.Create("AesManaged");
            aes.KeySize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (var rng = new RNGCryptoServiceProvider())
            {
                var key = new byte[16];
                rng.GetBytes(key);
                aes.Key = key;

                var iv = new byte[16];
                rng.GetBytes(iv);
                aes.IV = iv;
            }

            return aes;
        }

    }
}


