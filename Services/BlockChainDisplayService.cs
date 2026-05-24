using Blockchain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockchain.Services
{
    public class BlockChainDisplayService
    {
        //відображення блоку
        private void PrintBlock(Block block)
        {
            var randomColor = (ConsoleColor)(new Random().Next(11, 15));

            Console.ForegroundColor = randomColor;
            Console.WriteLine($"Index: {block.Index}");
            Console.WriteLine($"Timestamp: {block.Timestamp}");
            Console.WriteLine($"Hash: {block.Hash}");
            Console.WriteLine($"Previous Hash: {block.PrevHash}");
            Console.WriteLine($"Nonce: {block.Nonce}");
            Console.WriteLine($"Difficulty: {block.Difficulty}");
            Console.WriteLine($"Mining Duration: {block.MiningDuration}");
            Console.WriteLine($"Mining Attemps: {block.Attempts}");
            Console.WriteLine(
                $"Hash rate: " +
                $"{(block.MiningDuration > 0
                    ? block.Attempts / block.MiningDuration
                    : 0)} hashes/seconds");
            Console.WriteLine(new string('-', 40));
            Console.ForegroundColor = ConsoleColor.Gray;

            foreach (var tr in block.Transactions)
            {
                Console.WriteLine($"  Transaction: {tr.From} -> {tr.To}, Amount: {tr.Amount}");
            }

            Console.WriteLine(new string('-', 40));
        }

        //для відображення результату валідації
        public void PrintValidationResult(bool isValid)
        {
            if ( isValid )
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Blockchain is valid.");
                Console.ForegroundColor= ConsoleColor.Gray;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Blockchain is invalid!");
                Console.ForegroundColor = ConsoleColor.Gray;
            }

        }

        //для відображення всієї інформації
        public void PrintBlockChain(List<Block> chain)
        {
            foreach (var block in chain)
            {
                PrintBlock(block); 
            }
        }

        public void PrintBenchmarkResult(Block block)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Mining attemps: {block.Attempts}");
            Console.WriteLine($"Time taken: {block.MiningDuration} seconds");
            Console.WriteLine($"Difficulty: {block.Difficulty}");
            Console.WriteLine($"Hashrate: {block.Attempts / block.MiningDuration} hashes/seconds ");
            Console.WriteLine($"Duration {block.MiningDuration}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
