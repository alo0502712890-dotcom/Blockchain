using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Blockchain.Models;

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


        //шлях до файлу для зберігання даних блокчейну
        private readonly string _storageFilePath = "blockchain_date.dat";
        //баланс користувачів
        public Dictionary<string, decimal> Balances { get; set; } = new Dictionary<string, decimal>();


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
            
            this.LoadChainFromFile();
            if (Chain.Count == 0)
            {
                //створюєм генезисблок якщо блокчейн пустий
                CreateGenesisBlock();
            }
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

            //підраховуєм баланс
            this.ApplyBlockToState(genesisBlock);
            //зберігаємо блокчейн у файл
            this.AppendBlockToFile(genesisBlock);
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
                //
                //decimal senderSalanse = GetBalance(transaction.From);
                //if (senderSalanse < transaction.Amount + transaction.Fee)
                //    return false;
                ////////////////////////////////////////
                

                decimal senderBalance = 0;

                // Якщо адреса є в словнику балансів — отримуємо її баланс
                if (Balances.ContainsKey(transaction.From))
                {
                    senderBalance = Balances[transaction.From];
                }

                decimal pendingAmount = 0;

                foreach (var tx in _pendingTransactions)
                {
                    // Шукаємо транзакції цього відправника
                    if (tx.From == transaction.From)
                    {
                        pendingAmount += tx.Amount + tx.Fee;
                    }
                }

                // Доступний баланс
                decimal availableBalance = senderBalance - pendingAmount;

                // Перевірка коштів
                if (availableBalance < transaction.Amount + transaction.Fee)
                {
                    return false;
                }

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

            //Вибір транзакцій з найбільшою комісією
            var transactionToInclude =
                _pendingTransactions
                    .OrderByDescending(t => t.Fee)
                    .Take(max)
                    .ToList();

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

            //підраховуєм баланс
            this.ApplyBlockToState(block);

            //зберігаємо новий блок у файл
            this.AppendBlockToFile(block);

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
            //decimal balance = 0;

            //// Підраховуємо баланс для вказаної адреси
            //foreach (var block in Chain)
            //{
            //    // оновлюємо баланс для вказаної адреси
            //    foreach (var transaction in block.Transactions)
            //    {
            //        if (transaction.To == address)
            //        {
            //            // Додаємо суму транзакції до балансу, якщо адреса є отримувачем
            //            balance += transaction.Amount;
            //        }

            //        if (transaction.From == address)
            //        {
            //            // Віднімаємо суму транзакції та комісію від балансу, якщо адреса є відправником
            //            balance -= transaction.Amount + transaction.Fee;
            //        }
            //    }
            //}

            //// Також враховуємо незавершені транзакції, які ще не включені в блоки,
            //// але можуть впливати на баланс
            //foreach (var transaction in _pendingTransactions)
            //{
            //    if (transaction.To == address)
            //    {
            //        // Додаємо суму транзакції до балансу, якщо адреса є отримувачем
            //        balance += transaction.Amount;
            //    }

            //    if (transaction.From == address)
            //    {
            //        // Віднімаємо суму транзакції та комісію від балансу, якщо адреса є відправником
            //        balance -= transaction.Amount + transaction.Fee;
            //    }
            //}

            //return balance;
            ////////////////////////////////////////////////////////////
            
            // Отримуємо поточний баланс для адреси
            // (або 0, якщо адреса ще не має запису в балансах)
            var balance = Balances.ContainsKey(address) ? Balances[address] : 0;

            // Проходимо по всіх транзакціях в блоках та враховуємо їх вплив на баланс
            foreach (var tx in _pendingTransactions)
            {
                // Якщо адреса є відправником, віднімаємо суму транзакції та комісію
                if (tx.From == address)
                {
                    balance -= tx.Amount + tx.Fee;
                }

                // Якщо адреса є отримувачем, додаємо суму транзакції
                if (tx.To == address)
                {
                    balance += tx.Amount;
                }
            }

            return balance;
        }

        // застосування змін до стану блокчейну після додавання нового блоку
        private void ApplyBlockToState(Block block)
        {
            // Проходимо по всіх транзакціях в блоці та оновлюємо баланс для кожної адреси
            foreach (var transaction in block.Transactions)
            {
                // Враховуємо транзакції, які не є винагородою майнера
                if (transaction.From != "COINBASE")
                {
                    // Віднімаємо суму транзакції та комісію від балансу відправника
                    if (Balances.ContainsKey(transaction.From))
                    {
                        Balances[transaction.From] -= transaction.Amount + transaction.Fee;
                    }
                    else
                    {
                        // Якщо відправник ще не має запису в балансах,
                        // створюємо його з негативним балансом
                        Balances[transaction.From] = -transaction.Amount - transaction.Fee;
                    }
                }

                // Додаємо суму транзакції до балансу отримувача
                if (Balances.ContainsKey(transaction.To))
                {
                    // Якщо отримувач вже має запис в балансах,
                    // додаємо суму транзакції до його балансу
                    Balances[transaction.To] += transaction.Amount;
                }
                else
                {
                    // Якщо отримувач ще не має запису в балансах,
                    // створюємо його з сумою транзакції
                    Balances[transaction.To] = transaction.Amount;
                }
            }
        }

        //для збереження блокчейну в файл
        public void AppendBlockToFile(Block block)
        {
            string jsonLine = JsonSerializer.Serialize(block);

            // Додаємо серіалізований блок у файл (по одному блоку на рядок)
            File.AppendAllLines( _storageFilePath, new[] { jsonLine });
        }


        //завантаження блокчейну з файла
        public void LoadChainFromFile()
        {
            // Якщо файл не існує — нічого не завантажуємо
            if (!File.Exists(_storageFilePath))
                return;

            // Читаємо всі рядки з файлу
            var lines = File.ReadAllLines(_storageFilePath);

            Chain.Clear();
            Balances.Clear();
            Block? previousBlock = null;

            foreach (var line in lines)
            {
                // Десеріалізуємо рядок в об’єкт Block
                var block = JsonSerializer.Deserialize<Block>(line);

                // Якщо блок успішно створений —додаємо його до блокчейну
                //if (block != null)
                //{
                //    Chain.Add(block);
                //    //Відновлення кешу балансів
                //    ApplyBlockToState(block);
                //}
                /////////////////////////////////
                
                if (block == null) continue;

                // Перевірка hash
                string computedHash = _hashingService.ComputeHash(block);

                // Якщо hash не співпадає — файл було змінено вручну
                if (computedHash != block.Hash)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine( "КРИТИЧНА ПОМИЛКА: Файл blocks.dat скомпрометовано!");
                    Console.ResetColor();

                    Chain.Clear();
                    Balances.Clear();
                    return;
                }

                // Перевіряємо всі блоки, крім genesis block
                if (previousBlock != null)
                {
                    if (block.PrevHash != previousBlock.Hash)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine( "КРИТИЧНА ПОМИЛКА: Файл blocks.dat скомпрометовано!");
                        Console.ResetColor();

                        Chain.Clear();
                        Balances.Clear();
                        return;
                    }
                }
                // Додаємо блок у blockchain
                Chain.Add(block);

                ApplyBlockToState(block);

                previousBlock = block;
            }
        }


        public int GetPendingCount()
        {
            return _pendingTransactions.Count;
        }
    }
}
