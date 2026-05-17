using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockchain.Models
{
    public class P2PMessage
    {
        public string Data { get; set; }
        public MessageType Type { get; set; }
    }

    public enum MessageType
    {
        //повідомлення про новий блок
        BroadcastBlock,

        //повідомлення про нову транзакцію
        BroadcastTransaction,

        //отримання ланцюга
        RequestCgain,

        //передача ланцюга
        SendCain,
    }
}
