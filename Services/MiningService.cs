using Blockchain.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockchain.Services
{
    //Сервіс для майнінгу блоків
    public class MiningService
    {
        private readonly HashingService _hashingService;

        public MiningService(HashingService hashingService) 
        {
            _hashingService = new HashingService();
        }


        //Метод для майнінгу блоку
        public long MineBlock(Block block, int difficulty)
        {
            //Створення значення залежно від складності
            string target = new string('0', difficulty); //"0000"

            var stopwatch = Stopwatch.StartNew();

            //Нескінченний цикл підбору nonce
            while (true) 
            {
                block.Hash = _hashingService.ComputeHash(block);

                //Перевірка чи відповідає хеш складності
                if (block.Hash.Substring(0, difficulty) == target)
                {
                    break;
                }
                block.Nonce++;
            }
            stopwatch.Stop();

            //Збереження часу майнінгу
            block.MiningDuration = stopwatch.Elapsed.TotalSeconds;

            //Збереження кількості спроб
            block.Attemps = block.Nonce;

            return block.Nonce;
        }
    }
}
