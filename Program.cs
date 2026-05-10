using Blockchain.Models;
using Blockchain.Services;
using System.Text;

public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        TestRestoreBlockchain();
        //TestFee();
        //RunSecurityAudit();
        //TestTransactionPriority();


        //int port = 5000;
        //if (args.Length > 0)
        //{
        //    port = int.Parse(args[0]);
        //}


        ////initialservices
        //var walletService = new WalletService();
        //var blockChain = new BlockChain(1);
        //var transactionService = new TransactionService(walletService);
        //var p2pService = new P2PService(blockChain);
        //var displayService = new BlockChainDisplayService();

        ////створення гаманців
        //var aliceWallet = walletService.CreateWallet("Alice");
        //var bobWallet = walletService.CreateWallet("Bob");
        //var myWallet = walletService.CreateWallet("Vlad");

        ////запуск Р2Р сервера
        //p2pService.StartServer(port);

        //if (args.Length > 1)
        //{
        //    {
        //        int peerPort = int.Parse(args[1]);
        //        p2pService.ConnectToPeer("127.0.0.1", peerPort);
        //    }
        //}

        //while (true)
        //{
        //    Console.WriteLine($"Нода порт {port}");
        //    Console.WriteLine("=============================");
        //    Console.WriteLine("1.Створити транзакцію");
        //    Console.WriteLine("2.Майнити блок");
        //    Console.WriteLine("3.Показати блокчен");
        //    Console.WriteLine("4.Підключитися до іншої ноди вручну");
        //    Console.WriteLine("5.Перевірити валідацію блокчейн");
        //    Console.WriteLine("Оберіть дію: ");
        //    string choice = Console.ReadLine();

        //    switch (choice)
        //    {
        //        case "1":
        //            Console.WriteLine("Введіть суму: ");
        //            if (decimal.TryParse(Console.ReadLine(), out decimal amount))
        //            {
        //                //Створення транзакції
        //                var transaction = transactionService.CreateTransaction(aliceWallet, bobWallet.Address, amount, 0.01m);

        //                //Додавання в mempool
        //                if (blockChain.AddTransaction(transaction))
        //                {
        //                    Console.WriteLine("Транзакція додана до черги");

        //                    // Розсилка всім пірам
        //                    p2pService.BroadCast(MessageType.BroadcastTransaction, transaction);
        //                }
        //                else
        //                {
        //                    Console.WriteLine("Помилка при додаванні транзакції");
        //                }
        //            }
        //            else
        //            {
        //                Console.WriteLine("Невірна сума");
        //            }
        //            break;

        //        case "2":
        //            Console.WriteLine("Майнінг блоку,,,");
        //            blockChain.MinePendingTransactions(myWallet, 5);
        //            var latestBlock = blockChain.Chain.Last();
        //            p2pService.BroadCast(MessageType.BroadcastBlock, latestBlock);
        //            break;

        //        case "3":
        //            displayService.PrintBlockChain(blockChain.Chain);
        //            break;

        //        case "4":
        //            Console.WriteLine("Введіть порт іншої ноди: ");
        //            if (int.TryParse(Console.ReadLine(), out int pearPort))
        //            {
        //                p2pService.ConnectToPeer("127.0.0.1", pearPort);

        //            }
        //            break;

        //        case "5":
        //            bool isValid = blockChain.isValid();
        //            Console.WriteLine(isValid ? "Блокчейн валідний" : "Блокчейн невалідний!");
        //            break;

        //    }
        //}
    }

    public static void TestFee()
    {
        // Initialize services
        var walletService = new WalletService();
        var blockChain = new BlockChain(1);
        var transactionService = new TransactionService(walletService);
        var p2pService = new P2PService(blockChain);
        var displayService = new BlockChainDisplayService();

        // Створення гаманців
        var aliceWallet = walletService.CreateWallet("Alice");

        var bobWallet = walletService.CreateWallet("Bob");

        // Майнінг початкового блоку для отримання нагороди
        blockChain.MinePendingTransactions(aliceWallet, 5);
        blockChain.MinePendingTransactions(aliceWallet, 5);

        // Перевірка балансу після майнінгу
        Console.WriteLine(
            "Alice wallet balance: " +
            blockChain.GetBalance(aliceWallet.Address));

        Console.WriteLine(
            "Bob wallet balance: " +
            blockChain.GetBalance(bobWallet.Address));

        // Створення транзакцій з різними комісіями
        var transaction1 = transactionService.CreateTransaction(aliceWallet, bobWallet.Address, 4, 1.01m);
        var transaction2 = transactionService.CreateTransaction(aliceWallet, bobWallet.Address, 2, 0.8m);
        var transaction3 = transactionService.CreateTransaction(aliceWallet, bobWallet.Address, 9, 2.0m);

        // Додавання транзакцій до блокчейну
        blockChain.AddTransaction(transaction1);
        blockChain.AddTransaction(transaction2);
        blockChain.AddTransaction(transaction3);

        // Майнінг блоку для обробки транзакцій
        blockChain.MinePendingTransactions(bobWallet, 5);

        // Перевірка балансу після майнінгу
        Console.WriteLine(
            "Bob wallet balance: " +
            blockChain.GetBalance(bobWallet.Address));

        Console.WriteLine(
            "Alice wallet balance: " +
            blockChain.GetBalance(aliceWallet.Address));



        //displayService.PrintBlockChain(blockChain.Chain);
    }

    public static void RunSecurityAudit()
    {
        Console.WriteLine("SecurityAudit");

        // Ініціалізація сервісів
        var walletService = new WalletService();
        var blockChain = new BlockChain(1);
        var transactionService = new TransactionService(walletService);

        // Створення гаманців
        var attackerWallet = walletService.CreateWallet("Attacker");
        var bobWallet = walletService.CreateWallet("Bob");

        // 1
        Console.WriteLine("Сценарій 1: Гроші з повітря");

        var fakeTransaction = transactionService.CreateTransaction(attackerWallet, bobWallet.Address, 100, 0.1m);
        bool result = blockChain.AddTransaction(fakeTransaction);
        Console.WriteLine(result ? "транзакція пройшла" : "транзакція відхилена");

        //2
        Console.WriteLine("Сценарій 2: Фейковий блок");

        var lastBlock = blockChain.Chain.Last();
        var fakeBlock = new Block( lastBlock.Index + 1, new List<Transaction>(), lastBlock.Hash, 1);

        fakeBlock.Hash = "FAKE_HASH_123";
        blockChain.Chain.Add(fakeBlock);
        bool isValid = blockChain.isValid();
        Console.WriteLine( isValid ? "блок прийнятий" : "блок відхилений");

        //3
        Console.WriteLine("Сценарій 3: Легальна операція");

        blockChain = new BlockChain(1);
        var minerWallet = walletService.CreateWallet("Miner");
        blockChain.MinePendingTransactions( minerWallet, 5);

        Console.WriteLine("Баланс після майнінгу: " + blockChain.GetBalance(minerWallet.Address));

        var legalTransaction = transactionService.CreateTransaction( minerWallet, bobWallet.Address, 20, 0.1m);
        bool tResult = blockChain.AddTransaction(legalTransaction);

        Console.WriteLine(tResult ? "транзакція пройшла" : "транзакція відхилена");


        blockChain.MinePendingTransactions( minerWallet, 5);

        Console.WriteLine("Баланс Bob: " + blockChain.GetBalance(bobWallet.Address));
        Console.WriteLine("Баланс Miner: " + blockChain.GetBalance(minerWallet.Address));

    }

    public static void TestTransactionPriority()
    {

        //Сервіси
        var walletService = new WalletService();
        var blockChain = new BlockChain(1);
        var transactionService = new TransactionService(walletService);
        var displayService = new BlockChainDisplayService();

        //Гаманці
        var aliceWallet = walletService.CreateWallet("Alice");

        var bobWallet = walletService.CreateWallet("Bob");

        var minerWallet = walletService.CreateWallet("Miner");

        //Даємо Alice баланс через майнінг
        blockChain.MinePendingTransactions( aliceWallet, 5);
        blockChain.MinePendingTransactions( aliceWallet, 5);

        Console.WriteLine( "Alice balance: " + blockChain.GetBalance(aliceWallet.Address));

        //Транзакції з різними fee
        var tx1 = transactionService.CreateTransaction( aliceWallet, bobWallet.Address, 5, 0.1m);

        var tx2 = transactionService.CreateTransaction( aliceWallet, bobWallet.Address, 5, 2.0m);

        var tx3 = transactionService.CreateTransaction( aliceWallet, bobWallet.Address, 5, 1.5m);

        //Додаємо в mempool
        blockChain.AddTransaction(tx1);
        blockChain.AddTransaction(tx2);
        blockChain.AddTransaction(tx3);

        //Майнимо тільки 2 транзакції
        blockChain.MinePendingTransactions( minerWallet, 2);

        displayService.PrintBlockChain( blockChain.Chain);
    }

    public static void TestRestoreBlockchain()
    {

        // Перед запуском вручну видалити blockchain_date.dat

        // Сервіси
        var walletService = new WalletService();
        var transactionService = new TransactionService(walletService);

        var blockChain = new BlockChain(1);

        var minerWallet = walletService.CreateWallet("Miner");
        var aliceWallet = walletService.CreateWallet("Alice");

        // Майнінг 3 блоків
        blockChain.MinePendingTransactions( minerWallet, 5);
        blockChain.MinePendingTransactions( minerWallet, 5);
        blockChain.MinePendingTransactions( minerWallet, 5);

        Console.WriteLine( "Miner balance after mining: " +
            blockChain.GetBalance(minerWallet.Address));

        // Транзакція Miner -> Alice
        var tx = transactionService.CreateTransaction( minerWallet, aliceWallet.Address, 20, 1.0m);
        blockChain.AddTransaction(tx);

        // Майнінг блоку з транзакцією
        blockChain.MinePendingTransactions( minerWallet, 5);

        Console.WriteLine( "Alice balance before restart: " +
            blockChain.GetBalance(aliceWallet.Address));

        Console.WriteLine("RESTART");
        var restoredChain = new BlockChain(1);
        if (restoredChain.Chain.Count == 0)
        {
            Console.WriteLine(
                "Blockchain loading failed!");

            return;
        }


        // Перевірка 1 (Цілісність файлу): Вивести кількість рядків у файлі blocks.dat
        // Очікується: 5

        int linesCount = File.ReadLines("blockchain_date.dat").Count();

        Console.WriteLine("Lines in file: " + linesCount);



        //Перевірка 2 (Відновлення Кешу/State):
        //Вивести баланс Аліси, звертаючись безпосередньо до словника State,
        // Очікується: 20

        Console.WriteLine( "Alice balance from State: " +
            restoredChain.Balances[aliceWallet.Address]);


        //Перевірка 3 (Очищення мемпулу): Вивести кількість транзакцій у пулі
        // Очікується: 0
        Console.WriteLine("Pending transactions count: " +
            restoredChain.GetPendingCount());

        
    }
}
