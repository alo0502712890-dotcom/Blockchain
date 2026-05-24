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

            foreach (var transaction in block.Transactions)
            {
                transactionsData += transaction.ToRawString();
            }

            string blockData =
                $"{block.Index}{block.Timestamp.ToString("O")}{transactionsData}{block.PrevHash}{block.Nonce}";

            using var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(blockData);
            byte[] hashBytes = sha256.ComputeHash(inputBytes);

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        public string ComputeSHA256(string rowData)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(rowData);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
            
        //хушування методом дерева маркла
        public string GetMerkleRoot(List<Transaction> transactions)
        {
            // Якщо список порожній —повертаємо пустий рядок
            if (transactions == null || transactions.Count == 0)
                return string.Empty;

            // Хешуємо кожну транзакцію окремо
            List<string> merkleLeaves = transactions.Select(t => ComputeSHA256(t.ToRawString())).ToList();

            // Будуємо дерево Меркла поки не залишиться один hash
            while (merkleLeaves.Count > 1)
            {
                // Новий рівень дерева
                List<string> newLevel = new List<string>();
                // Об’єднання hash попарно
                for (int i = 0; i < merkleLeaves.Count; i+=2)
                {
                    string left = merkleLeaves[i];
                    string right = ( i + 1 < merkleLeaves.Count) ?  merkleLeaves[i+1] : left;
                    // Хешування пари hash
                    newLevel.Add(ComputeSHA256(left + right));
                }

                //if (merkleLeaves.Count%2 != 0)
                //{
                //    newLevel.Add(merkleLeaves.Last());
                //}
                // Переходимо на новий рівень дерева
                merkleLeaves = newLevel;
            }
            // Повертаємо фінальний hash — Merkle Root
            return merkleLeaves[0];
        }

    }
}
