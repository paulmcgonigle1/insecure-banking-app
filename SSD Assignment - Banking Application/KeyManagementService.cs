using System;
using System.Security.Cryptography;

namespace SSD_Assignment___Banking_Application
{
    public class KeyManagementService
    {
        private readonly string keyFilePath = "/Users/paulmcgonigle/Documents/Year4/MyAppSecureStorage/keyfile.txt"; // File path for the key
        private readonly string ivFilePath = "/Users/paulmcgonigle/Documents/Year4/MyAppSecureStorage/ivfile.txt";  // File path for the IV
        public KeyManagementService(string keyFilePath, string ivFilePath)
        {
            this.keyFilePath = keyFilePath;
            this.ivFilePath = ivFilePath;
        }

        public Aes GetAesProvider()
        {
            Aes aes = Aes.Create();
            aes.Key = GetKey();
            aes.IV = GetIV();
            return aes;
        }

        private byte[] GetKey()
        {
            // Check if key file exists and read, else generate and store
            if (File.Exists(keyFilePath))
            {
                byte[] key = File.ReadAllBytes(keyFilePath);
                Console.WriteLine("Key read from file: " + BitConverter.ToString(key)); // Log the key read from file


                return key;
            }
            else
            {
                // Generate and store key
                using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                {
                    byte[] key = new byte[16]; // 128 bits for AES
                    rng.GetBytes(key);
                    File.WriteAllBytes(keyFilePath, key);
                    Console.WriteLine("Key not found. New key generated and stored: " + BitConverter.ToString(key)); // Log the newly generated key

                    return key;
                }
            }
        }

        private byte[] GetIV()
        {
            // Similar logic as GetKey
            // IVs are typically not reused across sessions, but here for simplicity
            if (File.Exists(ivFilePath))
            {
                byte[] iv = File.ReadAllBytes(ivFilePath);
                Console.WriteLine("IV read from file: " + BitConverter.ToString(iv)); // Log the IV read from file
                return iv;
            }
            else
            {
                using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                {
                    byte[] iv = new byte[16]; // 128 bits for AES
                    rng.GetBytes(iv);
                    File.WriteAllBytes(ivFilePath, iv);
                    Console.WriteLine("IV not found. New IV generated and stored: " + BitConverter.ToString(iv)); // Log the newly generated IV
                    return iv;
                }
            }
        }
    }
}

