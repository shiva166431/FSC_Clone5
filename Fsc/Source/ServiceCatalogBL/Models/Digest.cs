using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using ServiceCatalog.BL;
using System.Text;

namespace ServiceCatalog.BL.Models
{
    //Reference: http://wiki.level3.com/wiki/HowToBuildTheHMACDigest
    public class HmacDigest
    {
        private string SecretKey { get; set; }
        public string PublicKey { get; private set; }
        public long EpochTime { get; private set; }
        public string HashedString { get; private set; }
        public string HashedStringBase64 { get; private set; }
        #region Encode Key
        public HmacDigest()
        {
            var Keys = new AppKeys();
            CryptoAlgorithm secretDecryptor = new CryptoAlgorithm("ENCRYPT", "FSCS", new AesManaged());
            PublicKey = Keys.Key;
            SecretKey = secretDecryptor.Decrypt(Keys.Secret);
            EpochTime = Convert.ToInt64((DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds);
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(SecretKey);
            HMACSHA256 sha = new HMACSHA256(keyByte);
            byte[] byteData = encoding.GetBytes(EpochTime.ToString());
            byte[] hashedDataOutput = sha.ComputeHash(byteData);

            HashedString = ByteToString(hashedDataOutput);
            HashedStringBase64 = Convert.ToBase64String(hashedDataOutput);
        }

        private string ByteToString(byte[] buff)
        {
            var sbinary = new StringBuilder();

            foreach (var b in buff)
                sbinary.Append(b.ToString("X2"));

            return (sbinary.ToString());
        }
        #endregion
    }
}
