using System.IO.IsolatedStorage;
using System.Security.Cryptography;
using System.Text;

namespace LapoLoanWebApi.EnAndDeHelper
{
    public static class LapoCipher1
    {
        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 256;

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;

        public static string Encrypt(string plainText, string passPhrase)
        {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var ivStringBytes = Generate256BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        public static string Decrypt(string cipherText, string passPhrase)
        {
            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            using (var streamReader = new StreamReader(cryptoStream, Encoding.UTF8))
                            {
                                return streamReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }

        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }

    public static class LapoCipher2
    {
        public static string Encrypt(string clearText)
        {
            string EncryptionKey = "abc123";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
        public static string Decrypt(string cipherText)
        {
            string EncryptionKey = "abc123";
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
    }

    //public static class LapoCipher10
    //{
    //    //While an app specific salt is not the best practice for
    //    //password based encryption, it's probably safe enough as long as
    //    //it is truly uncommon. Also too much work to alter this answer otherwise.
    //    private static byte[] _salt = __To_Do__("Add a app specific salt here");

    //    /// <summary>
    //    /// Encrypt the given string using AES.  The string can be decrypted using 
    //    /// DecryptStringAES().  The sharedSecret parameters must match.
    //    /// </summary>
    //    /// <param name="plainText">The text to encrypt.</param>
    //    /// <param name="sharedSecret">A password used to generate a key for encryption.</param>
    //    public static string EncryptStringAES(string plainText, string sharedSecret)
    //    {
    //        if (string.IsNullOrEmpty(plainText))
    //            throw new ArgumentNullException("plainText");
    //        if (string.IsNullOrEmpty(sharedSecret))
    //            throw new ArgumentNullException("sharedSecret");

    //        string outStr = null;                       // Encrypted string to return
    //        RijndaelManaged aesAlg = null;              // RijndaelManaged object used to encrypt the data.

    //        try
    //        {
    //            // generate the key from the shared secret and the salt
    //            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, _salt);

    //            // Create a RijndaelManaged object
    //            aesAlg = new RijndaelManaged();
    //            aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

    //            // Create a decryptor to perform the stream transform.
    //            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

    //            // Create the streams used for encryption.
    //            using (MemoryStream msEncrypt = new MemoryStream())
    //            {
    //                // prepend the IV
    //                msEncrypt.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int));
    //                msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
    //                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
    //                {
    //                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
    //                    {
    //                        //Write all data to the stream.
    //                        swEncrypt.Write(plainText);
    //                    }
    //                }
    //                outStr = Convert.ToBase64String(msEncrypt.ToArray());
    //            }
    //        }
    //        finally
    //        {
    //            // Clear the RijndaelManaged object.
    //            if (aesAlg != null)
    //                aesAlg.Clear();
    //        }

    //        // Return the encrypted bytes from the memory stream.
    //        return outStr;
    //    }

    //    /// <summary>
    //    /// Decrypt the given string.  Assumes the string was encrypted using 
    //    /// EncryptStringAES(), using an identical sharedSecret.
    //    /// </summary>
    //    /// <param name="cipherText">The text to decrypt.</param>
    //    /// <param name="sharedSecret">A password used to generate a key for decryption.</param>
    //    public static string DecryptStringAES(string cipherText, string sharedSecret)
    //    {
    //        if (string.IsNullOrEmpty(cipherText))
    //            throw new ArgumentNullException("cipherText");
    //        if (string.IsNullOrEmpty(sharedSecret))
    //            throw new ArgumentNullException("sharedSecret");

    //        // Declare the RijndaelManaged object
    //        // used to decrypt the data.
    //        RijndaelManaged aesAlg = null;

    //        // Declare the string used to hold
    //        // the decrypted text.
    //        string plaintext = null;

    //        try
    //        {
    //            // generate the key from the shared secret and the salt
    //            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, _salt);

    //            // Create the streams used for decryption.                
    //            byte[] bytes = Convert.FromBase64String(cipherText);
    //            using (MemoryStream msDecrypt = new MemoryStream(bytes))
    //            {
    //                // Create a RijndaelManaged object
    //                // with the specified key and IV.
    //                aesAlg = new RijndaelManaged();
    //                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
    //                // Get the initialization vector from the encrypted stream
    //                aesAlg.IV = ReadByteArray(msDecrypt);
    //                // Create a decrytor to perform the stream transform.
    //                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
    //                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
    //                {
    //                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))

    //                        // Read the decrypted bytes from the decrypting stream
    //                        // and place them in a string.
    //                        plaintext = srDecrypt.ReadToEnd();
    //                }
    //            }
    //        }
    //        finally
    //        {
    //            // Clear the RijndaelManaged object.
    //            if (aesAlg != null)
    //                aesAlg.Clear();
    //        }

    //        return plaintext;
    //    }

    //    private static byte[] ReadByteArray(Stream s)
    //    {
    //        byte[] rawLength = new byte[sizeof(int)];
    //        if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
    //        {
    //            throw new SystemException("Stream did not contain properly formatted byte array");
    //        }

    //        byte[] buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
    //        if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
    //        {
    //            throw new SystemException("Did not read byte array properly");
    //        }

    //        return buffer;
    //    }
    //}

    public  class LapoCipher3
    {
        //string message = "A very secret message here.";
        //DataEncryptor keys = new DataEncryptor();
        //string encr = keys.EncryptString(message);

        //// later
        //string actual = keys.DecryptString(encr);

        public TripleDESCryptoServiceProvider symm = null;

        #region Factory
        public LapoCipher3()
        {
            this.symm = new TripleDESCryptoServiceProvider();
            this.symm.Padding = PaddingMode.PKCS7;
        }

        public LapoCipher3(TripleDESCryptoServiceProvider keys)
        {
            this.symm = keys;
        }

        public LapoCipher3(byte[] key, byte[] iv)
        {
            this.symm = new TripleDESCryptoServiceProvider();
            this.symm.Padding = PaddingMode.PKCS7;
            this.symm.Key = key;
            this.symm.IV = iv;
        }

        #endregion

        #region Properties
        public TripleDESCryptoServiceProvider Algorithm
        {
            get { return symm; }
            set { symm = value; }
        }

        public byte[] Key
        {
            get { return symm.Key; }
            set { symm.Key = value; }
        }

        public byte[] IV
        {
            get { return symm.IV; }
            set { symm.IV = value; }
        }

        #endregion

        #region Crypto

        public byte[] Encrypt(byte[] data) { return Encrypt(data, data.Length); }
        public byte[] Encrypt(byte[] data, int length)
        {
            try
            {
                // Create a MemoryStream.
                var ms = new MemoryStream();

                // Create a CryptoStream using the MemoryStream 
                // and the passed key and initialization vector (IV).
                var cs = new CryptoStream(ms,
                    symm.CreateEncryptor(symm.Key, symm.IV),
                    CryptoStreamMode.Write);

                // Write the byte array to the crypto stream and flush it.
                cs.Write(data, 0, length);
                cs.FlushFinalBlock();

                // Get an array of bytes from the 
                // MemoryStream that holds the 
                // encrypted data.
                byte[] ret = ms.ToArray();

                // Close the streams.
                cs.Close();
                ms.Close();

                // Return the encrypted buffer.
                return ret;
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine("A cryptographic error occured: {0}", ex.Message);
            }
            return null;
        }

        public string EncryptString(string text)
        {
            return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(text)));
        }

        public byte[] Decrypt(byte[] data) { return Decrypt(data, data.Length); }
        public byte[] Decrypt(byte[] data, int length)
        {
            try
            {
                // Create a new MemoryStream using the passed 
                // array of encrypted data.
                MemoryStream ms = new MemoryStream(data);

                // Create a CryptoStream using the MemoryStream 
                // and the passed key and initialization vector (IV).
                CryptoStream cs = new CryptoStream(ms,
                    symm.CreateDecryptor(symm.Key, symm.IV),
                    CryptoStreamMode.Read);

                // Create buffer to hold the decrypted data.
                byte[] result = new byte[length];

                // Read the decrypted data out of the crypto stream
                // and place it into the temporary buffer.
                cs.Read(result, 0, result.Length);
                return result;
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine("A cryptographic error occured: {0}", ex.Message);
            }
            return null;
        }

        public string DecryptString(string data)
        {
            return Encoding.UTF8.GetString(Decrypt(Convert.FromBase64String(data))).TrimEnd('\0');
        }

        #endregion
    }

    public static class LapoCipherAESThenHMAC
    {
        private static readonly RandomNumberGenerator Random = RandomNumberGenerator.Create();

        //Preconfigured Encryption Parameters
        public static readonly int BlockBitSize = 128;
        public static readonly int KeyBitSize = 256;

        //Preconfigured Password Key Derivation Parameters
        public static readonly int SaltBitSize = 64;
        public static readonly int Iterations = 10000;
        public static readonly int MinPasswordLength = 12;

        /// <summary>
        /// Helper that generates a random key on each call.
        /// </summary>
        /// <returns></returns>
        public static byte[] NewKey()
        {
            var key = new byte[KeyBitSize / 8];
            Random.GetBytes(key);
            return key;
        }

        /// <summary>
        /// Simple Encryption (AES) then Authentication (HMAC) for a UTF8 Message.
        /// </summary>
        /// <param name="secretMessage">The secret message.</param>
        /// <param name="cryptKey">The crypt key.</param>
        /// <param name="authKey">The auth key.</param>
        /// <param name="nonSecretPayload">(Optional) Non-Secret Payload.</param>
        /// <returns>
        /// Encrypted Message
        /// </returns>
        /// <exception cref="System.ArgumentException">Secret Message Required!;secretMessage</exception>
        /// <remarks>
        /// Adds overhead of (Optional-Payload + BlockSize(16) + Message-Padded-To-Blocksize +  HMac-Tag(32)) * 1.33 Base64
        /// </remarks>
        public static string SimpleEncrypt(string secretMessage, byte[] cryptKey, byte[] authKey,
                           byte[] nonSecretPayload = null)
        {
            if (string.IsNullOrEmpty(secretMessage))
                throw new ArgumentException("Secret Message Required!", "secretMessage");

            var plainText = Encoding.UTF8.GetBytes(secretMessage);
            var cipherText = SimpleEncrypt(plainText, cryptKey, authKey, nonSecretPayload);
            return Convert.ToBase64String(cipherText);
        }

        /// <summary>
        /// Simple Authentication (HMAC) then Decryption (AES) for a secrets UTF8 Message.
        /// </summary>
        /// <param name="encryptedMessage">The encrypted message.</param>
        /// <param name="cryptKey">The crypt key.</param>
        /// <param name="authKey">The auth key.</param>
        /// <param name="nonSecretPayloadLength">Length of the non secret payload.</param>
        /// <returns>
        /// Decrypted Message
        /// </returns>
        /// <exception cref="System.ArgumentException">Encrypted Message Required!;encryptedMessage</exception>
        public static string SimpleDecrypt(string encryptedMessage, byte[] cryptKey, byte[] authKey,
                           int nonSecretPayloadLength = 0)
        {
            if (string.IsNullOrWhiteSpace(encryptedMessage))
                throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");

            var cipherText = Convert.FromBase64String(encryptedMessage);
            var plainText = SimpleDecrypt(cipherText, cryptKey, authKey, nonSecretPayloadLength);
            return plainText == null ? null : Encoding.UTF8.GetString(plainText);
        }

        /// <summary>
        /// Simple Encryption (AES) then Authentication (HMAC) of a UTF8 message
        /// using Keys derived from a Password (PBKDF2).
        /// </summary>
        /// <param name="secretMessage">The secret message.</param>
        /// <param name="password">The password.</param>
        /// <param name="nonSecretPayload">The non secret payload.</param>
        /// <returns>
        /// Encrypted Message
        /// </returns>
        /// <exception cref="System.ArgumentException">password</exception>
        /// <remarks>
        /// Significantly less secure than using random binary keys.
        /// Adds additional non secret payload for key generation parameters.
        /// </remarks>
        public static string SimpleEncryptWithPassword(string secretMessage, string password,
                                 byte[] nonSecretPayload = null)
        {
            if (string.IsNullOrEmpty(secretMessage))
                throw new ArgumentException("Secret Message Required!", "secretMessage");

            var plainText = Encoding.UTF8.GetBytes(secretMessage);
            var cipherText = SimpleEncryptWithPassword(plainText, password, nonSecretPayload);
            return Convert.ToBase64String(cipherText);
        }

        /// <summary>
        /// Simple Authentication (HMAC) and then Descryption (AES) of a UTF8 Message
        /// using keys derived from a password (PBKDF2). 
        /// </summary>
        /// <param name="encryptedMessage">The encrypted message.</param>
        /// <param name="password">The password.</param>
        /// <param name="nonSecretPayloadLength">Length of the non secret payload.</param>
        /// <returns>
        /// Decrypted Message
        /// </returns>
        /// <exception cref="System.ArgumentException">Encrypted Message Required!;encryptedMessage</exception>
        /// <remarks>
        /// Significantly less secure than using random binary keys.
        /// </remarks>
        public static string SimpleDecryptWithPassword(string encryptedMessage, string password,
                                 int nonSecretPayloadLength = 0)
        {
            if (string.IsNullOrWhiteSpace(encryptedMessage))
                throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");

            var cipherText = Convert.FromBase64String(encryptedMessage);
            var plainText = SimpleDecryptWithPassword(cipherText, password, nonSecretPayloadLength);
            return plainText == null ? null : Encoding.UTF8.GetString(plainText);
        }

        public static byte[] SimpleEncrypt(byte[] secretMessage, byte[] cryptKey, byte[] authKey, byte[] nonSecretPayload = null)
        {
            //User Error Checks
            if (cryptKey == null || cryptKey.Length != KeyBitSize / 8)
                throw new ArgumentException(String.Format("Key needs to be {0} bit!", KeyBitSize), "cryptKey");

            if (authKey == null || authKey.Length != KeyBitSize / 8)
                throw new ArgumentException(String.Format("Key needs to be {0} bit!", KeyBitSize), "authKey");

            if (secretMessage == null || secretMessage.Length < 1)
                throw new ArgumentException("Secret Message Required!", "secretMessage");

            //non-secret payload optional
            nonSecretPayload = nonSecretPayload ?? new byte[] { };

            byte[] cipherText;
            byte[] iv;

            using (var aes = new AesManaged
            {
                KeySize = KeyBitSize,
                BlockSize = BlockBitSize,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            })
            {

                //Use random IV
                aes.GenerateIV();
                iv = aes.IV;

                using (var encrypter = aes.CreateEncryptor(cryptKey, iv))
                using (var cipherStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(cipherStream, encrypter, CryptoStreamMode.Write))
                    using (var binaryWriter = new BinaryWriter(cryptoStream))
                    {
                        //Encrypt Data
                        binaryWriter.Write(secretMessage);
                    }

                    cipherText = cipherStream.ToArray();
                }

            }

            //Assemble encrypted message and add authentication
            using (var hmac = new HMACSHA256(authKey))
            using (var encryptedStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(encryptedStream))
                {
                    //Prepend non-secret payload if any
                    binaryWriter.Write(nonSecretPayload);
                    //Prepend IV
                    binaryWriter.Write(iv);
                    //Write Ciphertext
                    binaryWriter.Write(cipherText);
                    binaryWriter.Flush();

                    //Authenticate all data
                    var tag = hmac.ComputeHash(encryptedStream.ToArray());
                    //Postpend tag
                    binaryWriter.Write(tag);
                }
                return encryptedStream.ToArray();
            }

        }

        public static byte[] SimpleDecrypt(byte[] encryptedMessage, byte[] cryptKey, byte[] authKey, int nonSecretPayloadLength = 0)
        {

            //Basic Usage Error Checks
            if (cryptKey == null || cryptKey.Length != KeyBitSize / 8)
                throw new ArgumentException(String.Format("CryptKey needs to be {0} bit!", KeyBitSize), "cryptKey");

            if (authKey == null || authKey.Length != KeyBitSize / 8)
                throw new ArgumentException(String.Format("AuthKey needs to be {0} bit!", KeyBitSize), "authKey");

            if (encryptedMessage == null || encryptedMessage.Length == 0)
                throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");

            using (var hmac = new HMACSHA256(authKey))
            {
                var sentTag = new byte[hmac.HashSize / 8];
                //Calculate Tag
                var calcTag = hmac.ComputeHash(encryptedMessage, 0, encryptedMessage.Length - sentTag.Length);
                var ivLength = (BlockBitSize / 8);

                //if message length is to small just return null
                if (encryptedMessage.Length < sentTag.Length + nonSecretPayloadLength + ivLength)
                    return null;

                //Grab Sent Tag
                Array.Copy(encryptedMessage, encryptedMessage.Length - sentTag.Length, sentTag, 0, sentTag.Length);

                //Compare Tag with constant time comparison
                var compare = 0;
                for (var i = 0; i < sentTag.Length; i++)
                    compare |= sentTag[i] ^ calcTag[i];

                //if message doesn't authenticate return null
                if (compare != 0)
                    return null;

                using (var aes = new AesManaged
                {
                    KeySize = KeyBitSize,
                    BlockSize = BlockBitSize,
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7
                })
                {

                    //Grab IV from message
                    var iv = new byte[ivLength];
                    Array.Copy(encryptedMessage, nonSecretPayloadLength, iv, 0, iv.Length);

                    using (var decrypter = aes.CreateDecryptor(cryptKey, iv))
                    using (var plainTextStream = new MemoryStream())
                    {
                        using (var decrypterStream = new CryptoStream(plainTextStream, decrypter, CryptoStreamMode.Write))
                        using (var binaryWriter = new BinaryWriter(decrypterStream))
                        {
                            //Decrypt Cipher Text from Message
                            binaryWriter.Write(
                              encryptedMessage,
                              nonSecretPayloadLength + iv.Length,
                              encryptedMessage.Length - nonSecretPayloadLength - iv.Length - sentTag.Length
                            );
                        }
                        //Return Plain Text
                        return plainTextStream.ToArray();
                    }
                }
            }
        }

        public static byte[] SimpleEncryptWithPassword(byte[] secretMessage, string password, byte[] nonSecretPayload = null)
        {
            nonSecretPayload = nonSecretPayload ?? new byte[] { };

            //User Error Checks
            if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
                throw new ArgumentException(String.Format("Must have a password of at least {0} characters!", MinPasswordLength), "password");

            if (secretMessage == null || secretMessage.Length == 0)
                throw new ArgumentException("Secret Message Required!", "secretMessage");

            var payload = new byte[((SaltBitSize / 8) * 2) + nonSecretPayload.Length];

            Array.Copy(nonSecretPayload, payload, nonSecretPayload.Length);
            int payloadIndex = nonSecretPayload.Length;

            byte[] cryptKey;
            byte[] authKey;
            //Use Random Salt to prevent pre-generated weak password attacks.
            using (var generator = new Rfc2898DeriveBytes(password, SaltBitSize / 8, Iterations))
            {
                var salt = generator.Salt;

                //Generate Keys
                cryptKey = generator.GetBytes(KeyBitSize / 8);

                //Create Non Secret Payload
                Array.Copy(salt, 0, payload, payloadIndex, salt.Length);
                payloadIndex += salt.Length;
            }

            //Deriving separate key, might be less efficient than using HKDF, 
            //but now compatible with RNEncryptor which had a very similar wireformat and requires less code than HKDF.
            using (var generator = new Rfc2898DeriveBytes(password, SaltBitSize / 8, Iterations))
            {
                var salt = generator.Salt;

                //Generate Keys
                authKey = generator.GetBytes(KeyBitSize / 8);

                //Create Rest of Non Secret Payload
                Array.Copy(salt, 0, payload, payloadIndex, salt.Length);
            }

            return SimpleEncrypt(secretMessage, cryptKey, authKey, payload);
        }

        public static byte[] SimpleDecryptWithPassword(byte[] encryptedMessage, string password, int nonSecretPayloadLength = 0)
        {
            //User Error Checks
            if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
                throw new ArgumentException(String.Format("Must have a password of at least {0} characters!", MinPasswordLength), "password");

            if (encryptedMessage == null || encryptedMessage.Length == 0)
                throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");

            var cryptSalt = new byte[SaltBitSize / 8];
            var authSalt = new byte[SaltBitSize / 8];

            //Grab Salt from Non-Secret Payload
            Array.Copy(encryptedMessage, nonSecretPayloadLength, cryptSalt, 0, cryptSalt.Length);
            Array.Copy(encryptedMessage, nonSecretPayloadLength + cryptSalt.Length, authSalt, 0, authSalt.Length);

            byte[] cryptKey;
            byte[] authKey;

            //Generate crypt key
            using (var generator = new Rfc2898DeriveBytes(password, cryptSalt, Iterations))
            {
                cryptKey = generator.GetBytes(KeyBitSize / 8);
            }
            //Gen

            return cryptKey;
        }
    }

    public class LapoCipher15
    {
        private static string encryptedFiles = "";
        private static IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForSite();
        private const string PasswordSalt = "PasswordSalt";

        public LapoCipher15()
        {
            store.CreateDirectory("EncryptedFiles");

            // Create subdirectory under MyFiles.
            encryptedFiles = System.IO.Path.Combine("EncryptedFiles", "EncryptedFiles");

            store.CreateDirectory(encryptedFiles);
        }

        private void ListFiles()
        {
            string searchpath = System.IO.Path.Combine(encryptedFiles, "*.*");
            string[] filesInSubDirs = store.GetFileNames(searchpath);
            StringBuilder sb = new StringBuilder();
            // List files in MyFiles\EncryptedFiles.
            sb.AppendLine(@"Files in EncryptedFiles\EncryptedFiles:");
            foreach (string fileName in filesInSubDirs)
            {
                sb.AppendLine(" - " + fileName);
            }

            sb.AppendLine();

            var outputBlock = sb.ToString();
        }

        private string Decrypt(string Password)
        {
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
            string encryptedFiles = System.IO.Path.Combine("EncryptedFiles", "EncryptedFiles");
            store.CreateDirectory(encryptedFiles);
            string xFilePath = System.IO.Path.Combine(encryptedFiles, "encryptedFile.txt");

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            using (IsolatedStorageFileStream isoStoreStream = isoStore.OpenFile(xFilePath, FileMode.Open))
            {
                using (Aes aes = new AesManaged())
                {
                    Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(Password, Encoding.UTF8.GetBytes(PasswordSalt));
                    aes.Key = deriveBytes.GetBytes(128 / 8);
                    // Get the initialization vector from the encrypted stream
                    aes.IV = ReadByteArray(isoStoreStream);
                    CryptoStream cs = new CryptoStream(isoStoreStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
                    StreamReader reader = new StreamReader(cs, Encoding.Unicode);
                    try
                    {
                        string retval = reader.ReadToEnd();
                        reader.Dispose();
                        cs.Dispose();
                        return retval;
                    }
                    catch (Exception e)
                    {
                        return e.ToString();
                    }

                }
            }
        }
        private string Encrypt(string Password)
        {
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
            string encryptedFiles = System.IO.Path.Combine("EncryptedFiles", "EncryptedFiles");
            store.CreateDirectory(encryptedFiles);
            string xFilePath = System.IO.Path.Combine(encryptedFiles, "encryptedFile.txt");
            IsolatedStorageFileStream xStream = store.CreateFile(xFilePath);
            xStream.Close();
           // decryptBox.Text = "encryptedFile.txt";

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            using (IsolatedStorageFileStream isoStoreStream = isoStore.OpenFile(xFilePath, FileMode.Create))
            {
                using (Aes aes = new AesManaged())
                {
                    Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(Password, Encoding.UTF8.GetBytes(PasswordSalt));
                    aes.Key = deriveBytes.GetBytes(128 / 8);
                    isoStoreStream.Write(BitConverter.GetBytes(aes.IV.Length), 0, sizeof(int));
                    isoStoreStream.Write(aes.IV, 0, aes.IV.Length);

                    using (CryptoStream cs = new CryptoStream(isoStoreStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] rawPlaintext = Encoding.Unicode.GetBytes(""/*inputBox.Text*/);
                        cs.Write(rawPlaintext, 0, rawPlaintext.Length);
                        cs.FlushFinalBlock();
                    }
                }
            }
            xStream.Close();
            xStream =
                store.OpenFile(System.IO.Path.Combine(encryptedFiles, "encryptedFile.txt"), FileMode.Open);
            StreamReader reader = new StreamReader(xStream);
            // Read the data.

            reader.Close();
            xStream.Close();
            return reader.ReadToEnd();
        }
        private static byte[] ReadByteArray(Stream s)
        {
            byte[] rawLength = new byte[sizeof(int)];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
            {
                throw new SystemException("Stream did not contain properly formatted byte array");
            }

            byte[] buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new SystemException("Did not read byte array properly");
            }

            return buffer;
        }

    }

    public class LapoCipher02
    {
        private const string EncryptionKey = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public  string EncryptString( string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(EncryptionKey);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public  string DecryptString(string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(EncryptionKey);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }

    public class LapoCipher01
    {
        public  string DecryptString(string encrString)
        {
            byte[] b;
            string decrypted;
            try
            {
                b = Convert.FromBase64String(encrString);
                decrypted = System.Text.ASCIIEncoding.ASCII.GetString(b);
            }
            catch (FormatException fe)
            {
                decrypted = "";
            }

            return decrypted;
        }

        public string EnryptString(object strEncrypted)
        {
            byte[] b = System.Text.ASCIIEncoding.ASCII.GetBytes(strEncrypted.ToString());
            string encrypted = Convert.ToBase64String(b);
            return encrypted;
        }
    }

    public class LapoCipher00
    {
        public LapoCipher00() { }

        public string encrypt(string encryptString)
        {
            string EncryptionKey = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            byte[] clearBytes = Encoding.Unicode.GetBytes(encryptString);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
        });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    encryptString = Convert.ToBase64String(ms.ToArray());
                }
            }
            return encryptString;
        }

        public string Decrypt(string cipherText)
        {
            string EncryptionKey = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76

            });

                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
    }

}