using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace login_thing
{
    /// <summary>
    /// This class is an industry standard method of encryption, I didn't write it but it basically generates a random bunch of bytes called a salt
    /// and uses that to encrypt the password 4096 times, it's one way, so to test if the user put in the right password you have to encrypt the
    /// password they give you and then compared that against the stored one.
    /// </summary>
    public class PasswordHash 
    {
        // The following constants may be changed without breaking existing hashes.
        public const int SALT_BYTE_SIZE = 24;
        public const int HASH_BYTE_SIZE = 24;
        public const int PBKDF2_ITERATIONS = 4096;

        public const int ITERATION_INDEX = 1;
        public const int SALT_INDEX = 2;
        public const int PBKDF2_INDEX = 3;

        // Creates a salted PBKDF2 hash of the password.
        public static string CreateHash(string password)
        {
            // Generate a random salt
            RNGCryptoServiceProvider csprng = new RNGCryptoServiceProvider();
            byte[] salt = new byte[SALT_BYTE_SIZE];
            csprng.GetBytes(salt);

            // Hash the password and encode the parameters
            byte[] hash = PBKDF2(password, salt, PBKDF2_ITERATIONS, HASH_BYTE_SIZE);
            return "PBKDF2:" + PBKDF2_ITERATIONS + ":" + Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }

        // Validates a password given a hash of the correct one.
        public static bool ValidatePassword(string password, string correctHash)
        {
            // Extract the parameters from the hash
            char[] delimiter = { ':' };
            string[] split = correctHash.Split(delimiter);
            int iterations = Int32.Parse(split[ITERATION_INDEX]);
            byte[] salt = Convert.FromBase64String(split[SALT_INDEX]);
            byte[] hash = Convert.FromBase64String(split[PBKDF2_INDEX]);
            byte[] testHash = PBKDF2(password, salt, iterations, hash.Length);
            return SlowEquals(hash, testHash);
        }

        // Compares two byte arrays in length-constant time. This comparison
        // method is used so that password hashes cannot be extracted from
        // on-line systems using a timing attack and then attacked off-line.
        private static bool SlowEquals(byte[] a, byte[] b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
            {
                diff |= (uint)(a[i] ^ b[i]);
            }
            return diff == 0;
        }

        // Computes the PBKDF2-SHA1 hash of a password.
        private static byte[] PBKDF2(string password, byte[] salt, int iterations, int outputBytes)
        {
            Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt);
            pbkdf2.IterationCount = iterations;
            return pbkdf2.GetBytes(outputBytes);
        }
    }
}
