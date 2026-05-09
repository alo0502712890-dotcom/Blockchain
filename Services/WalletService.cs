using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Blockchain.Models;

namespace Blockchain.Services
{
    public class WalletService
    {
        // Метод для створення нового гаманця з випадковим ключем
        public Wallet CreateWallet(string name)
        {
            // Використовуємо ECDSA для генерації ключів
            using var ecdsa = System.Security.Cryptography.ECDsa.Create(ECCurve.NamedCurves.nistP256);

            // Експортуємо приватний та публічний ключі
            byte[] privateKey = ecdsa.ExportECPrivateKey();
            byte[] publicKey = ecdsa.ExportSubjectPublicKeyInfo();

            // Створюємо адресу гаманця на основі публічного ключа 
            string address = Convert.ToBase64String(publicKey);

            // Повертаємо новий гаманець
            return new Wallet(name, address, publicKey, privateKey);
        }


        // Метод для перевірки підпису даних за допомогою публічного ключа
        public bool VerifySignature(byte[] data, byte[] signature, byte[] publicKey)
        {
            try
            {
                // Використовуємо ECDSA для перевірки підпису
                using var ecdsa = ECDsa.Create();

                // Імпортуємо публічний ключ для перевірки
                ecdsa.ImportSubjectPublicKeyInfo(publicKey, out _);

                // Перевіряємо підпис даних за допомогою публічного ключа
                return ecdsa.VerifyData(data, signature, HashAlgorithmName.SHA256);
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
