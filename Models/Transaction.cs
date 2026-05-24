using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockchain.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public decimal Amount { get; set; } = 0;
        public decimal Fee { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        //
        public string Memo { get; set; } = string.Empty;

        //підпис приватним ключем
        public byte[] Signature { get; set; }

        //публічний ключ
        public byte[] PublicKey { get; set; }


        public Transaction() { }
        public Transaction(string from, string to, decimal amount, decimal fee, byte[] publicKey, string memo = "")
        {
            From = from;
            To = to;
            Amount = amount;
            Fee = fee;
            PublicKey = publicKey;
            Memo = memo ?? string.Empty;
        }


        // формує данні для підпису = данні обєднуються та хешуються
        public byte[] GetDataSign()
        {
            string data = $"{From}:{To}:{Amount}:{Fee}:{Timestamp.ToString("O")}:{Memo}";
            return Encoding.UTF8.GetBytes(data);
        }


        // перетворює транзакцію в рядок, використовується для хешування блоку
        public string ToRawString()
        {
            string hexSignature = Signature != null
                ? BitConverter.ToString(Signature).Replace("-", "")
                : "null";

            return $"{From}:{To}:{Amount}:{Fee}:{Timestamp.ToString("O")}:{hexSignature}:{Memo}";
        }




    }
}
