using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockchain.Models
{
    public class Wallet
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] PrivateKey { get; set; }
        public Wallet(string name, string address, byte[] publicKey, byte[] privateKey) 
        {
            Name = name;
            Address = address;
            PublicKey = publicKey;
            PrivateKey = privateKey;
        }

        //метод для підпису даних за допомогою приватного ключа
        public byte[ ] Sign(byte[] data)
        {
            using (var rsa = new System.Security.Cryptography.RSACryptoServiceProvider())
            {
                rsa.ImportRSAPrivateKey(PrivateKey, out _);
                return rsa.SignData(data, System.Security.Cryptography.HashAlgorithmName.SHA256, System.Security.Cryptography.RSASignaturePadding.Pkcs1);
            }
        }
    }
}
