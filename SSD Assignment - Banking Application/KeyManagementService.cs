using System;
using System.Security.Cryptography;

namespace SSD_Assignment___Banking_Application
{
	public class KeyManagementService //still need to add error handling
	{
        private readonly string cryptoKeyName;
        private readonly CngProvider keyStorageProvider;

        public KeyManagementService(string keyName, string provider)
        {
            cryptoKeyName = keyName;
            keyStorageProvider = new CngProvider(provider);
        }

        public Aes GetAesProvider()
        {
            if (!CngKey.Exists(cryptoKeyName, keyStorageProvider))
            {
                CreateKey();
            }

            return new AesCng(cryptoKeyName, keyStorageProvider);
        }

        private void CreateKey()
        {
            var keyCreationParameters = new CngKeyCreationParameters()
            {
                Provider = keyStorageProvider
                // Add other parameters and logic as required.
            };

            CngKey.Create(new CngAlgorithm("AES"), cryptoKeyName, keyCreationParameters);
        }
    }
}

