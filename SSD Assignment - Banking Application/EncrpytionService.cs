using System;
using System.Security.Cryptography;
namespace SSD_Assignment___Banking_Application
{
	public static class EncrpytionService
	{
        //i am using the aes options with static method to ensure that i am not reusing encrption providers across different calls, which is more secure


        //still need to think about securing my keys and IV'S properly and do exception handling
       

        //constructor that initializes the AES provider with a specific key and IV
        public static byte[] Encrypt(byte[] plaintextData, byte[] key, byte[] iv)
        {
            using(Aes aes = Aes.Create()){//create a new instance of the AES provider
              
                aes.KeySize = 128;
                aes.Mode = CipherMode.CBC; //use cipher block chaining mode
                aes.Padding = PaddingMode.PKCS7;//allows variable data lengths
                aes.Key = key;
                aes.IV = iv;

                using (var msEncrypt = new MemoryStream())//stream to hold encrypted data
                using (var encryptor = aes.CreateEncryptor()) //creates a symetric aes encryptor
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))// linking encryptor with memory stream
                {
                    csEncrypt.Write(plaintextData, 0, plaintextData.Length);// encrpyts
                    csEncrypt.FlushFinalBlock();//finalises(disposed at the end of the using block)
                    return msEncrypt.ToArray();//converts encrypted data from stream to a byte array
                }
            }
           

        }
        public static byte[] Decrypt(byte[] ciphertextData, byte[] key, byte[] iv)
        {
            //using statements to ensure that resources are freed once operations are complete

            using (Aes aes= Aes.Create())
            {
                aes.KeySize = 128;
                aes.Mode = CipherMode.CBC; //use cipher block chaining mode
                aes.Padding = PaddingMode.PKCS7;//allows variable data lengths
                aes.Key = key;
                aes.IV = iv;

                using (var msDecrypt = new MemoryStream(ciphertextData))// stream
                using (var decryptor = aes.CreateDecryptor())//new symetric AES decryptor
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    byte[] plaintextData = new byte[ciphertextData.Length];//prepare byte array to hold decryped data
                    int bytesRead = csDecrypt.Read(plaintextData, 0, plaintextData.Length);//decrpyts data
                    Array.Resize(ref plaintextData, bytesRead);//resise array to actual data lenght
                    return plaintextData; //return decrypted data as a byte array
                }


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

