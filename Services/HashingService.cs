using Blockchain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockchain.Services
{
    //Сервіс для обчислення хешу блоку
    public class HashingService
    {
        //Метод для генерації хешу блоку
        public string ComputeHash(Block block)
        {
            var transactionsData = "";

            //Об’єднання всіх транзакцій у один рядок
            foreach (var transaction in block.Transactions)
            {
                transactionsData += transaction.ToRawString();
            }
            //Формування даних блоку для хешування
            string blockData =
                $"{block.Index}" +
                $"{block.Timestamp.ToString("O")}" +
                $"{transactionsData}" +
                $"{block.PrevHash}" +
                $"{block.Nonce}";

            //Створення SHA-256 алгоритму
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(blockData);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                
            }
        }

    }
}
