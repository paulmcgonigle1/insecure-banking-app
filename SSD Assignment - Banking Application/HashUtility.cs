using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SSD_Assignment___Banking_Application
{
    public static class HashUtility
    {
        
        //  to generate a hash for a given string
        public static string GenerateHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            }
        }

      
        //  method to verify a hash
        public static bool VerifyHash(string input, string hash)
        {
            string computedHash = GenerateHash(input);
            return computedHash.Equals(hash, StringComparison.OrdinalIgnoreCase);
        }


    }
}
