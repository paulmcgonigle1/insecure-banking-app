using System;
using System.Security.Cryptography;
using System.Text;

namespace SSD_Assignment___Banking_Application
{
	public static class EncrpytionService
	{
        //i am using the aes options with static method to ensure that i am not reusing encrption providers across different calls, which is more secure


        //still need to think about securing my keys and IV'S properly and do exception handling
        private static KeyManagementService keyManagementService = new KeyManagementService("/Users/paulmcgonigle/Documents/Year4/MyAppSecureStorage/keyfile.txt", "/Users/paulmcgonigle/Documents/Year4/MyAppSecureStorage/ivfile.txt");

        //constructor that initializes the AES provider with a specific key and IV
        public static byte[] Encrypt(byte[] plaintextData)

        {
            try
            {

                using (Aes aes = keyManagementService.GetAesProvider())
                {

                    // aes.key and aes.iv already set by GetAesProvider


                    aes.KeySize = 128;
                    aes.Mode = CipherMode.CBC; //use cipher block chaining mode
                    aes.Padding = PaddingMode.PKCS7;//allows variable data lengths



                    using (var msEncrypt = new MemoryStream())//stream to hold encrypted data
                    using (var encryptor = aes.CreateEncryptor()) //creates a symetric aes encryptor
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            csEncrypt.Write(plaintextData, 0, plaintextData.Length);
                            csEncrypt.FlushFinalBlock();

                            byte[] encryptedData = msEncrypt.ToArray();
                            Console.WriteLine($"Encrypted Data: {Convert.ToBase64String(encryptedData)}");
                            return encryptedData;
                        }
                    }

                }
            }
            catch (CryptographicException ex)
            {

                Console.WriteLine($"Encryption failed: {ex.Message}");
                throw;
            }
           

           

        }
        public static byte[] Decrypt(byte[] ciphertextData)
        {
            //using statements to ensure that resources are freed once operations are complete
            try

            {

                using (Aes aes = keyManagementService.GetAesProvider())
                {

                    aes.KeySize = 128;
                    aes.Mode = CipherMode.CBC; //use cipher block chaining mode
                    aes.Padding = PaddingMode.PKCS7;//allows variable data lengths


                    using (var msDecrypt = new MemoryStream(ciphertextData))// stream for ciphertext
                    using (var decryptor = aes.CreateDecryptor())//new symetric AES decryptor
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        byte[] plaintextData = new byte[ciphertextData.Length];//prepare byte array to hold decryped data
                        int bytesRead = csDecrypt.Read(plaintextData, 0, plaintextData.Length);//decrpyts data
                        Array.Resize(ref plaintextData, bytesRead);//resise array to actual data lenght
                        Console.WriteLine($"Decrypted Data: {Encoding.UTF8.GetString(plaintextData)}");

                        return plaintextData; //return decrypted data as a byte array
                    }


                }
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine($"Cryptographic error during decryption: {ex.Message}");
                // Optionally, rethrow the exception if you want it to be handled further up the call stack
                throw;
            }
            catch (Exception ex)
            {
                // Handle other types of exceptions
                Console.WriteLine($"An error occurred during decryption: {ex.Message}");
                // Optionally, rethrow the exception
                throw;
            }


        }
        

     
        




        //private void InitializeAes()
        //{
        //          RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        //          aes = Aes.Create("AesManaged");
        //          aes.KeySize = 128;
        //          aes.Mode = CipherMode.CBC;
        //          aes.Padding = PaddingMode.PKCS7;


        //          aes.Key = new byte[16];
        //          rng.GetBytes(aes.Key);

        //          aes.IV = new byte[16];
        //          rng.GetBytes(aes.IV);


        //      }

        //      public byte[] Encrypt(byte[] plaintext_data)
        //      {
        //          byte[] ciphertext_data;//Byte Array Where Result Of Encryption Operation Will Be Stored.

        //          ICryptoTransform encryptor = aes.CreateEncryptor();//Object That Contains The AES Encryption Algorithm (Using The Key and IV Value Specified In The AES Object). 

        //          MemoryStream msEncrypt = new MemoryStream();//MemoryStream That Will Store The Output Of The Encryption Operation.

        //          /*
        //              Calling The Write() Method On The CryptoStream Object Declared Below Will Store/Write 
        //              The Result Of The Encryption Operation To The Memory Stream Object Specified In The Constructor.
        //          */
        //          CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        //          csEncrypt.Write(plaintext_data, 0, plaintext_data.Length);//Writes All Data Contained In plaintext_data Byte Array To CryptoStream (Which Then Gets Encrypted And Gets Written to the msEncrypt MemoryStream).
        //          csEncrypt.Dispose();//Closes CryptoStream

        //          ciphertext_data = msEncrypt.ToArray();//Output Result Of Encryption Operation In Byte Array Form.
        //          msEncrypt.Dispose();//Closes MemoryStream

        //          return ciphertext_data;
        //      }

        //      public byte[] Decrypt(byte[] ciphertext_data)
        //      {
        //          byte[] plaintext_data;//Byte Array Where Result Of Decryption Operation Will Be Stored.

        //          ICryptoTransform decryptor = aes.CreateDecryptor();//Object That Contains The AES Decryption Algorithm (Using The Key and IV Value Specified In The AES Object). 

        //          MemoryStream msDecrypt = new MemoryStream();//MemoryStream That Will Store The Output Of The Decryption Operation.

        //          /*
        //              Calling The Write() Method On The CryptoStream Object Declared Below Will Store/Write 
        //              The Result Of The Decryption Operation To The Memory Stream Object Specified In The Constructor.
        //          */
        //          CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write);//Writes All Data Contained In Byte Array To CryptoStream (Which Then Gets Decrypted).
        //          csDecrypt.Write(ciphertext_data, 0, ciphertext_data.Length);//Writes All Data Contained In ciphertext_data Byte Array To CryptoStream (Which Then Gets Decrypted And Gets Written to the msDecrypt MemoryStream).
        //          csDecrypt.Dispose();//Closes CryptoStream

        //          plaintext_data = msDecrypt.ToArray();//Output Result Of Decryption Operation In Byte Array Form.
        //          msDecrypt.Dispose();//Closes MemoryStream

        //          return plaintext_data;
        //      }
    }
}

