using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Blockchain.Models;
using Transaction = Blockchain.Models.Transaction;


namespace Blockchain.Services
{
    //сервіс шо відповідає за створення, підпис  та обробку транзакції
    public class TransactionService
    {
        private readonly WalletService walletService;

        public TransactionService(WalletService walletService)
        {
            this.walletService = walletService;
        }

        //метод для створення нової транзакції з підписом
        public Transaction CreateTransaction(Wallet wallet, string to, decimal amount, decimal fee)
        {
            //формуємо обєкт, заповнюємо поля
            var transaction = new Transaction(wallet.Address, to, amount, fee, wallet.PublicKey);

            //готуємо данні для підпису
            byte[] dataToSign = transaction.GetDataSign();
            using var ecdsa = System.Security.Cryptography.ECDsa.Create();

            //використовуємо приватний ключ
            ecdsa.ImportECPrivateKey(wallet.PrivateKey, out _);

            //піписуєм транзакцію
            transaction.Signature = ecdsa.SignData(dataToSign, HashAlgorithmName.SHA256);
            return transaction;
        }
    }
}
