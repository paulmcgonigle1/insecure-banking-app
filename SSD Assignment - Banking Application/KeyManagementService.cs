using System;
using System.Security.Cryptography;

namespace SSD_Assignment___Banking_Application
{
    public class KeyManagementService
    {
        private readonly string crypto_key_name = "key name";
        private readonly CngProvider key_storage_provider = CngProvider.MicrosoftSoftwareKeyStorageProvider;

        public Aes InitializeKey()
        {
            if (!CngKey.Exists(crypto_key_name, key_storage_provider))
            {
                CngKeyCreationParameters key_creation_parameters = new CngKeyCreationParameters
                {
                    Provider = key_storage_provider,
                    // Additional parameters can be set here
                };

                // Create a new key
                CngKey.Create(new CngAlgorithm("AES"), crypto_key_name, key_creation_parameters);
            }

            // Return a new AesCng instance using the key
            return new AesCng(crypto_key_name, key_storage_provider);
        }

        // Additional methods for key management can be added here
    }
}
