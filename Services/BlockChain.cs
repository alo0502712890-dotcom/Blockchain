using Blockchain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockchain.Services
{
    public  class BlockChain
    {
        //Ланцюг блоків
        public List<Block> Chain { get; set; }

        //складність майнінгу
        public int Difficulty { get; set; } = 1;

        //цільовий час майнінгу блоку
        private readonly double _targetBlockTime = 1;

        //інтервал для коригування складності
        private readonly int _adjustmentInterval = 5;

        //сервіс для обчислення хешу блоків
        private readonly HashingService _hashingService;

        //Список непідтверджених транзакцій
        private readonly List<Transaction> _pendingTransactions = new List<Transaction>();

        //Сервіс для перевірки підписів гаманця
        private readonly WalletService _walletService = new WalletService();

        //винагорода за майнінг блоку
        private readonly int minerReward = 50;

        //Сервіс для майнінгу блоків
        private readonly MiningService _miningService;


        public BlockChain(int difficulty)
        {
            Chain = new List<Block>();
            _hashingService = new HashingService();
            _miningService = new MiningService(_hashingService);
            this.Difficulty = difficulty;
            CreateGenesisBlock();
        }

        //Створення genesis-блоку (першого блоку в мережі)
        private void CreateGenesisBlock() 
        {
            var genesisBlock = new Block(0, new List<Transaction>(), "0", 0) 
            { 
                Timestamp = DateTime.Parse("2024-01-01T00:00:00Z"),
                Nonce = 0,
            };
            
            _miningService.MineBlock(genesisBlock, Difficulty);

            Chain.Add(genesisBlock);
        }


        //Додавання транзакції до списку очікування
        public bool AddTransaction(Transaction transaction)
        {
            //Перевірка цифрового підпису транзакції
            bool isValid = _walletService.VerifySignature(transaction.GetDataSign(), transaction.Signature, transaction.PublicKey);
            if (!isValid)
                return false;

            //чи достатньо коштів у відправника
            if (transaction.From != "COINBASE")
            {
                decimal senderSalanse = GetBalance(transaction.From);
                if (senderSalanse < transaction.Amount + transaction.Fee)
                    return false;
            }


            //Додавання транзакції до пулу
            _pendingTransactions.Add(transaction);
            return true;
        }


        //Майнінг нового блоку з транзакціями
        public void MinePendingTransactions(Wallet minerWallet, int max)
        {
            //Отримання останнього блоку
            var lastBlock = Chain.Last();

            //Вибір транзакцій для нового блоку
            var transactionToInclude = _pendingTransactions.Take(max).ToList();

            //загальна сума комісії з транзакцій
            var totalFees = transactionToInclude.Sum(t => t.Fee);

            // Створення нового блоку
            var block = new Block(
                lastBlock.Index + 1, 
                transactionToInclude, 
                lastBlock.Hash, Difficulty);

            //Створення винагороди майнеру
            var minerRewardTx = new Transaction
            {
                From = "COINBASE",
                To = minerWallet.Address,
                Amount = minerReward + totalFees,
                Timestamp = DateTime.UtcNow,
            };

            block.Transactions.Add(minerRewardTx);
            _miningService.MineBlock(block, Difficulty);
            Chain.Add(block);

            //Видалення підтверджених транзакцій
            _pendingTransactions.RemoveAll(t => transactionToInclude.Contains(t));

            //Коригування складності
            if (block.Index % _adjustmentInterval == 0)
            {
                AdjustDiffuculty();
            }
        }


        //Автоматичне коригування складності майнінгу
        private void AdjustDiffuculty()
        {
            //Отримання останніх блоків
            var recentBlock = Chain.Where(b => b.Index > 0).TakeLast(_adjustmentInterval).ToList();

            //Перевірка кількості блоків
            if (recentBlock.Count < _adjustmentInterval) 
            {
                return;
            }
            //Обчислення середнього часу майнінгу
            double averageTime = recentBlock.Average(b => b.MiningDuration);

            //Якщо блоки майняться занадто швидко — збільшуємо складність
            if (averageTime < _targetBlockTime) 
            {
                Difficulty++;
            }
            //Якщо занадто повільно — зменшуємо
            else if (averageTime > _targetBlockTime)
            {
                Difficulty = Math.Max(1, Difficulty - 1);
            }
        }

        //Перевірка цілісності блокчейну
        public bool isValid()
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                var currentBlock = Chain[i];

                var prevBlock = Chain[i - 1];

                //Перевірка правильності хешу блоку
                if (currentBlock.Hash != _hashingService.ComputeHash(currentBlock))
                    return false;

                //Перевірка зв’язку між блоками
                if (currentBlock.PrevHash != prevBlock.Hash)
                    return false;

                //Перевірка складності майнінгу
                if (!currentBlock.Hash.StartsWith(new string('0', currentBlock.Difficulty)))
                    return false;

                //перевірка транзакцій в блоці
                foreach (var transaction in currentBlock.Transactions)
                {
                    if (transaction.From != "COINBASE")
                    {
                        bool isValid = _walletService.VerifySignature(transaction.GetDataSign(), transaction.Signature, transaction.PublicKey);
                        if (!isValid) return false;
                    }
                }
            }
            return true;
        }




        public decimal GetBalance(string address)
        {
            decimal balance = 0;

            // Підраховуємо баланс для вказаної адреси
            foreach (var block in Chain)
            {
                // оновлюємо баланс для вказаної адреси
                foreach (var transaction in block.Transactions)
                {
                    if (transaction.To == address)
                    {
                        // Додаємо суму транзакції до балансу, якщо адреса є отримувачем
                        balance += transaction.Amount;
                    }

                    if (transaction.From == address)
                    {
                        // Віднімаємо суму транзакції та комісію від балансу, якщо адреса є відправником
                        balance -= transaction.Amount + transaction.Fee;
                    }
                }
            }

            // Також враховуємо незавершені транзакції, які ще не включені в блоки,
            // але можуть впливати на баланс
            foreach (var transaction in _pendingTransactions)
            {
                if (transaction.To == address)
                {
                    // Додаємо суму транзакції до балансу, якщо адреса є отримувачем
                    balance += transaction.Amount;
                }

                if (transaction.From == address)
                {
                    // Віднімаємо суму транзакції та комісію від балансу, якщо адреса є відправником
                    balance -= transaction.Amount + transaction.Fee;
                }
            }

            return balance;
        }


    }
}
