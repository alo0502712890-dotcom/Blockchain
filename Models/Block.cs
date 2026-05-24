using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockchain.Models
{
    public class Block
    {
        public Block() { }
        public Block(int index, List<Transaction> transactions, string prevHash, string author = "", int difficulty = 4)
        {
            Index = index;

            PrevHash = prevHash;
            Timestamp = DateTime.Now;
            Transactions = transactions;
            Hash = "";
            Difficulty = difficulty;
            Author = author;

        }

        public int Index { get; set; }
        public DateTime Timestamp { get; set; }

        public string Author { get; set; }

        public List<Transaction> Transactions { get; set; }

        public string Hash { get; set; }
        public string PrevHash { get; set; }

        //кількість спроб
        public long Attempts { get; set; }

        //час на майнінг блоку у секундах
        public double MiningDuration { get; set; } = 0;

        //складність
        public int Difficulty { get; set; }

        //для генерації потрібного хешу для майнінгу(починається з нуля)
        public long Nonce { get; set; }


    }
}
