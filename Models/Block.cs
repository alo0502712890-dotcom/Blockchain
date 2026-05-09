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
        public Block(int index, List<Transaction> transactions, string prevHash, int diffculty) 
        { 
            Index= index;
            PrevHash= prevHash;
            Difficulty= diffculty;
            Transactions= transactions;
        }

        public int Index { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        //public string Data { get; set; } 
        public List<Transaction> Transactions { get; set; }

        public string Hash { get; set; }
        public string PrevHash { get; set; }

        //кількість спроб
        public long Attemps { get; set; }

        //час на майнінг блоку у секундах
        public double MiningDuration { get; set; }

        //складність
        public int Difficulty { get; set; }

        //для генерації потрібного хешу для майнінгу(починається з нуля)
        public long Nonce { get; set; }


    }
}
