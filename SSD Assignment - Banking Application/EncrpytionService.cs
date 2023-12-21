using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;

namespace SSD_Assignment___Banking_Application
{
    public class EncryptionService
    {
        private readonly KeyManagementService keyManagementService;

        public EncryptionService(KeyManagementService keyManagementService)
        {
            this.keyManagementService = keyManagementService;
        }

        // encrypts the plaintext data using the AES instance from KeyManagementService.
        public byte[] Encrypt(byte[] plaintextData)
        {
            using (var aes = keyManagementService.InitializeKey())
            {
                aes.GenerateIV();
                var iv = aes.IV;

                aes.Mode = CipherMode.CBC; // Set Cipher Mode
                aes.Padding = PaddingMode.PKCS7; // Set Padding Mode
                
                using (var msEncrypt = new MemoryStream())
                {
                    msEncrypt.Write(iv, 0, iv.Length);
                    using (var encryptor = aes.CreateEncryptor())
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(plaintextData, 0, plaintextData.Length);
                        csEncrypt.FlushFinalBlock();
                        return msEncrypt.ToArray();
                    }
                }
               
            }
        }

        // decryopts ciphertext data using the AES instance from KeyManagementService.
        public byte[] Decrypt(byte[] ciphertextData)
        {
            using (var aes = keyManagementService.InitializeKey())
            {
                // Extract the IV from the beginning of the data
                var iv = new byte[16]; // AES block size is 16 bytes
                Array.Copy(ciphertextData, 0, iv, 0, iv.Length);
                aes.IV = iv;

                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                using (var msDecrypt = new MemoryStream(ciphertextData, iv.Length, ciphertextData.Length - iv.Length))
                using (var decryptor = aes.CreateDecryptor())
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    var plaintextData = new byte[ciphertextData.Length - iv.Length];
                    var bytesRead = csDecrypt.Read(plaintextData, 0, plaintextData.Length);
                    Array.Resize(ref plaintextData, bytesRead);
                    return plaintextData;
                }
            }
           
        }
    }
}
