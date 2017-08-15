using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LFE.Application.Services.Helper
{
    public class EncryptionServices
    {

        private const string SECURE_KEY = "Zen1T-4emp10N!";

        private static byte[] _passwordBytes = new byte[0];

        private static byte[] PasswordBytes
        {
            get
            {
                if(_passwordBytes.Length > 0) return _passwordBytes;

                var passwordBytes = Encoding.UTF8.GetBytes(SECURE_KEY);

                // Hash the password with SHA256
                _passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                return _passwordBytes;
            }
        }
        
        public string EncryptText(string input)
        {
            // Get the bytes of the string
            var bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
         
            var bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, PasswordBytes);
            var result         = Convert.ToBase64String(bytesEncrypted);

            return result;
        }

        public string DecryptText(string input)
        {
            // Get the bytes of the string
            var bytesToBeDecrypted = Convert.FromBase64String(input);
            
            var bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, PasswordBytes);
            var result         = Encoding.UTF8.GetString(bytesDecrypted);

            return result;
        }

        private static byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (var ms = new MemoryStream())
            {
                using (var AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        private static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (var ms = new MemoryStream())
            {
                using (var AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }
    }
}
