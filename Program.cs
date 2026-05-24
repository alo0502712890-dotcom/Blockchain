using System.Text;
using System.Xml.Linq;
using Blockchain.Models;
using Blockchain.Services;

public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        //TestColdHack();
        //TestMerkleAttack();
        //TestConsensusSecurity();
        //TestConsensus();
        //TestRestoreBlockchain();
        //TestFee();
        //RunSecurityAudit();
        //TestTransactionPriority();


        int port = 5000;
        if (args.Length > 0)
        {
            port = int.Parse(args[0]);
        }


        //initialservices
        var walletService = new WalletService();
        var blockChain = new BlockChain(1);
        var transactionService = new TransactionService(walletService);
        var p2pService = new P2PService(blockChain);
        var displayService = new BlockChainDisplayService();

        //створення гаманців
        var aliceWallet = walletService.CreateWallet("Alice");
        var bobWallet = walletService.CreateWallet("Bob");
        var myWallet = walletService.CreateWallet("Vlad");

        //запуск Р2Р сервера
        p2pService.StartServer(port);

        if (args.Length > 1)
        {
            {
                int peerPort = int.Parse(args[1]);
                p2pService.ConnectToPeer("127.0.0.1", peerPort);
            }
        }

        while (true)
        {
            Console.WriteLine($"Нода порт {port}");
            Console.WriteLine("=============================");
            Console.WriteLine("1.Створити транзакцію");
            Console.WriteLine("2.Майнити блок");
            Console.WriteLine("3.Показати блокчен");
            Console.WriteLine("4.Підключитися до іншої ноди вручну");
            Console.WriteLine("5.Перевірити валідацію блокчейн");
            Console.WriteLine("Оберіть дію: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.WriteLine("Введіть суму: ");
                    if (decimal.TryParse(Console.ReadLine(), out decimal amount))
                    {
                        //комісія
                        Console.WriteLine("Введіть комісію: ");

                        if (decimal.TryParse(Console.ReadLine(), out decimal fee))
                        {
                            Console.WriteLine("Введіть повідомлення: ");
                            string memo = Console.ReadLine();

                            //Створення транзакції
                            var transaction = transactionService.CreateTransaction(
                                myWallet,
                                bobWallet.Address,
                                amount,
                                fee,
                                memo
                            );

                            //Додавання в mempool
                            if (blockChain.AddTransaction(transaction))
                            {
                                Console.WriteLine("Транзакція додана до черги");

                                // Розсилка всім пірам
                                p2pService.BroadCast(MessageType.BroadcastTransaction, transaction);
                            }
                            else
                            {
                                Console.WriteLine("Помилка при додаванні транзакції");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Невірна комісія");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Невірна сума");
                    }
                    break;

                case "2":
                    Console.WriteLine("Майнінг блоку,,,");
                    blockChain.MinePendingTransactions(myWallet, 5);
                    var latestBlock = blockChain.Chain.Last();
                    p2pService.BroadCast(MessageType.BroadcastBlock, latestBlock);
                    break;

                case "3":
                    displayService.PrintBlockChain(blockChain.Chain);
                    break;

                case "4":
                    Console.WriteLine("Введіть IP: ");
                    string ip = Console.ReadLine();

                    Console.WriteLine("Введіть порт іншої ноди: ");
                    if (int.TryParse(Console.ReadLine(), out int pearPort))
                    {
                        p2pService.ConnectToPeer(ip, pearPort);
                        Console.WriteLine("Підключення виконано");
                    }
                    break;

                case "5":
                    bool isValid = blockChain.isValid(blockChain.Chain);
                    Console.WriteLine(isValid ? "Блокчейн валідний" : "Блокчейн невалідний!");
                    break;

            }
        }
    }

    //public static void TestFee()
    //{
    //    // Initialize services
    //    var walletService = new WalletService();
    //    var blockChain = new BlockChain(1);
    //    var transactionService = new TransactionService(walletService);
    //    var p2pService = new P2PService(blockChain);
    //    var displayService = new BlockChainDisplayService();

    //    // Створення гаманців
    //    var aliceWallet = walletService.CreateWallet("Alice");

    //    var bobWallet = walletService.CreateWallet("Bob");

    //    // Майнінг початкового блоку для отримання нагороди
    //    blockChain.MinePendingTransactions(aliceWallet, 5);
    //    blockChain.MinePendingTransactions(aliceWallet, 5);

    //    // Перевірка балансу після майнінгу
    //    Console.WriteLine(
    //        "Alice wallet balance: " +
    //        blockChain.GetBalance(aliceWallet.Address));

    //    Console.WriteLine(
    //        "Bob wallet balance: " +
    //        blockChain.GetBalance(bobWallet.Address));

    //    // Створення транзакцій з різними комісіями
    //    var transaction1 = transactionService.CreateTransaction(aliceWallet, bobWallet.Address, 4, 1.01m);
    //    var transaction2 = transactionService.CreateTransaction(aliceWallet, bobWallet.Address, 2, 0.8m);
    //    var transaction3 = transactionService.CreateTransaction(aliceWallet, bobWallet.Address, 9, 2.0m);

    //    // Додавання транзакцій до блокчейну
    //    blockChain.AddTransaction(transaction1);
    //    blockChain.AddTransaction(transaction2);
    //    blockChain.AddTransaction(transaction3);

    //    // Майнінг блоку для обробки транзакцій
    //    blockChain.MinePendingTransactions(bobWallet, 5);

    //    // Перевірка балансу після майнінгу
    //    Console.WriteLine(
    //        "Bob wallet balance: " +
    //        blockChain.GetBalance(bobWallet.Address));

    //    Console.WriteLine(
    //        "Alice wallet balance: " +
    //        blockChain.GetBalance(aliceWallet.Address));



    //    //displayService.PrintBlockChain(blockChain.Chain);
    //}

    //public static void RunSecurityAudit()
    //{
    //    Console.WriteLine("SecurityAudit");

    //    // Ініціалізація сервісів
    //    var walletService = new WalletService();
    //    var blockChain = new BlockChain(1);
    //    var transactionService = new TransactionService(walletService);

    //    // Створення гаманців
    //    var attackerWallet = walletService.CreateWallet("Attacker");
    //    var bobWallet = walletService.CreateWallet("Bob");

    //    // 1
    //    Console.WriteLine("Сценарій 1: Гроші з повітря");

    //    var fakeTransaction = transactionService.CreateTransaction(attackerWallet, bobWallet.Address, 100, 0.1m);
    //    bool result = blockChain.AddTransaction(fakeTransaction);
    //    Console.WriteLine(result ? "транзакція пройшла" : "транзакція відхилена");

    //    //2
    //    Console.WriteLine("Сценарій 2: Фейковий блок");

    //    var lastBlock = blockChain.Chain.Last();
    //    var fakeBlock = new Block(lastBlock.Index + 1, new List<Transaction>(), lastBlock.Hash, 1);

    //    fakeBlock.Hash = "FAKE_HASH_123";
    //    blockChain.Chain.Add(fakeBlock);
    //    //bool isValid = blockChain.isValid();
    //    //Console.WriteLine(isValid ? "блок прийнятий" : "блок відхилений");

    //    //3
    //    Console.WriteLine("Сценарій 3: Легальна операція");

    //    blockChain = new BlockChain(1);
    //    var minerWallet = walletService.CreateWallet("Miner");
    //    blockChain.MinePendingTransactions(minerWallet, 5);

    //    Console.WriteLine("Баланс після майнінгу: " + blockChain.GetBalance(minerWallet.Address));

    //    var legalTransaction = transactionService.CreateTransaction(minerWallet, bobWallet.Address, 20, 0.1m);
    //    bool tResult = blockChain.AddTransaction(legalTransaction);

    //    Console.WriteLine(tResult ? "транзакція пройшла" : "транзакція відхилена");


    //    blockChain.MinePendingTransactions(minerWallet, 5);

    //    Console.WriteLine("Баланс Bob: " + blockChain.GetBalance(bobWallet.Address));
    //    Console.WriteLine("Баланс Miner: " + blockChain.GetBalance(minerWallet.Address));

    //}

    //public static void TestTransactionPriority()
    //{

    //    //Сервіси
    //    var walletService = new WalletService();
    //    var blockChain = new BlockChain(1);
    //    var transactionService = new TransactionService(walletService);
    //    var displayService = new BlockChainDisplayService();

    //    //Гаманці
    //    var aliceWallet = walletService.CreateWallet("Alice");

    //    var bobWallet = walletService.CreateWallet("Bob");

    //    var minerWallet = walletService.CreateWallet("Miner");

    //    //Даємо Alice баланс через майнінг
    //    blockChain.MinePendingTransactions(aliceWallet, 5);
    //    blockChain.MinePendingTransactions(aliceWallet, 5);

    //    Console.WriteLine("Alice balance: " + blockChain.GetBalance(aliceWallet.Address));

    //    //Транзакції з різними fee
    //    var tx1 = transactionService.CreateTransaction(aliceWallet, bobWallet.Address, 5, 0.1m);

    //    var tx2 = transactionService.CreateTransaction(aliceWallet, bobWallet.Address, 5, 2.0m);

    //    var tx3 = transactionService.CreateTransaction(aliceWallet, bobWallet.Address, 5, 1.5m);

    //    //Додаємо в mempool
    //    blockChain.AddTransaction(tx1);
    //    blockChain.AddTransaction(tx2);
    //    blockChain.AddTransaction(tx3);

    //    //Майнимо тільки 2 транзакції
    //    blockChain.MinePendingTransactions(minerWallet, 2);

    //    displayService.PrintBlockChain(blockChain.Chain);
    //}

    //public static void TestRestoreBlockchain()
    //{

    //    // Перед запуском вручну видалити blockchain_date.dat

    //    // Сервіси
    //    var walletService = new WalletService();
    //    var transactionService = new TransactionService(walletService);

    //    var blockChain = new BlockChain(1);

    //    var minerWallet = walletService.CreateWallet("Miner");
    //    var aliceWallet = walletService.CreateWallet("Alice");

    //    // Майнінг 3 блоків
    //    blockChain.MinePendingTransactions(minerWallet, 5);
    //    blockChain.MinePendingTransactions(minerWallet, 5);
    //    blockChain.MinePendingTransactions(minerWallet, 5);

    //    Console.WriteLine("Miner balance after mining: " +
    //        blockChain.GetBalance(minerWallet.Address));

    //    // Транзакція Miner -> Alice
    //    var tx = transactionService.CreateTransaction(minerWallet, aliceWallet.Address, 20, 1.0m);
    //    blockChain.AddTransaction(tx);

    //    // Майнінг блоку з транзакцією
    //    blockChain.MinePendingTransactions(minerWallet, 5);

    //    Console.WriteLine("Alice balance before restart: " +
    //        blockChain.GetBalance(aliceWallet.Address));

    //    Console.WriteLine("RESTART");
    //    var restoredChain = new BlockChain(1);



    //    // Перевірка 1 (Цілісність файлу): Вивести кількість рядків у файлі blocks.dat
    //    // Очікується: 5

    //    int linesCount = File.ReadLines("blockchain_date.dat").Count();

    //    Console.WriteLine("Lines in file: " + linesCount);



    //    //Перевірка 2 (Відновлення Кешу/State):
    //    //Вивести баланс Аліси, звертаючись безпосередньо до словника State,
    //    // Очікується: 20

    //    Console.WriteLine("Alice balance from State: " +
    //        restoredChain.Balances[aliceWallet.Address]);


    //    //Перевірка 3 (Очищення мемпулу): Вивести кількість транзакцій у пулі
    //    // Очікується: 0
    //    Console.WriteLine("Pending transactions count: " +
    //        restoredChain.GetPendingCount());


    //}

    //public static void TestConsensus()
    //{
    //    var walletService = new WalletService();

    //    var node1 = new BlockChain(1);
    //    var node2 = new BlockChain(2);

    //    var satoshi = walletService.CreateWallet("Satoshi");
    //    var vitalic = walletService.CreateWallet("Vitalic");

    //    node1.MinePendingTransactions(satoshi, 5);
    //    node1.MinePendingTransactions(satoshi, 5);
    //    Console.WriteLine("Node1 count" + node1.Chain.Count);

    //    node2.MinePendingTransactions(vitalic, 5);
    //    node2.MinePendingTransactions(vitalic, 5);
    //    node2.MinePendingTransactions(vitalic, 5);
    //    node2.MinePendingTransactions(vitalic, 5);
    //    Console.WriteLine("Node2 count" + node2.Chain.Count);

    //    node1.ReplaceChain(node2.Chain);

    //    //1 Вивести поточну кількість блоків у nodeA.Chain.Count. Очікується: 5.
    //    Console.WriteLine();
    //    Console.WriteLine("Node1 count" + node1.Chain.Count);

    //    //2 Вивести баланси Satoshi та Vitalik на nodeA,
    //    //звертаючись безпосередньо до миттєвого словника
    //    decimal satoshiBalanse = 0;
    //    if (node1.Balances.ContainsKey(satoshi.Address))
    //    {
    //        satoshiBalanse = node1.Balances[satoshi.Address];
    //    }

    //    decimal vitalicBalanse = 0;
    //    if (node1.Balances.ContainsKey(vitalic.Address))
    //    {
    //        vitalicBalanse = node1.Balances[vitalic.Address];
    //    }
    //    Console.WriteLine();
    //    Console.WriteLine("Satoshi balanse: " + satoshiBalanse);

    //    Console.WriteLine();
    //    Console.WriteLine("Vitalic Balanse: " + vitalicBalanse);

    //    //3 Вивести кількість рядків у локальному файлі сховища blocks.dat
    //    int count = File.ReadLines("blockchain_date.dat").Count();

    //    Console.WriteLine();
    //    Console.WriteLine("Lines in file: " + count);


    //}

    //public static void TestConsensusSecurity()
    //{
    //    var walletService = new WalletService();

    //    // Ноди
    //    //наша нода
    //    var localNode = new BlockChain(1);
    //    //нода зловмисника
    //    var hackerNode = new BlockChain(1);
    //    //чесна потужна мережа
    //    var honestNetwork = new BlockChain(1);


    //    // Гаманці
    //    var minerWallet = walletService.CreateWallet("Miner");
    //    var hackerWallet = walletService.CreateWallet("Hacker");
    //    var poolWallet = walletService.CreateWallet("Pool");

    //    // Майнимо 2 валідні блоки. Очікується: 3
    //    localNode.MinePendingTransactions(minerWallet, 5);
    //    localNode.MinePendingTransactions(minerWallet, 5);

    //    Console.WriteLine("Кількість блоків localNode: " + localNode.Chain.Count);

    //    // чесний блок
    //    hackerNode.MinePendingTransactions(hackerWallet, 5);

    //    // ФЕЙКОВІ блоки
    //    for (int i = 0; i < 5; i++)
    //    {
    //        var lastBlock = hackerNode.Chain.Last();
    //        var fakeBlock = new Block(lastBlock.Index + 1, new List<Transaction>(), lastBlock.Hash, 1);

    //        // ФЕЙКОВИЙ HASH
    //        fakeBlock.Hash = "HACKED_HASH";
    //        hackerNode.Chain.Add(fakeBlock);
    //    }
    //    Console.WriteLine("Кількість блоків hackerNode: " + hackerNode.Chain.Count);
    //    // Очікується: 7


    //    // Майнимо 4 валідні блоки         // Очікується: 5
    //    honestNetwork.MinePendingTransactions(poolWallet, 5);
    //    honestNetwork.MinePendingTransactions(poolWallet, 5); 
    //    honestNetwork.MinePendingTransactions(poolWallet, 5); 
    //    honestNetwork.MinePendingTransactions(poolWallet, 5);
    //    Console.WriteLine("Кількість блоків honestNetwork: " + honestNetwork.Chain.Count);


    //    // 1         // очікується - False, 3
    //    Console.WriteLine();
    //    Console.WriteLine("CHECK 1");

    //    bool hackerResult = localNode.ReplaceChain(hackerNode.Chain);

    //    Console.WriteLine("Чи прийнято chain хакера: " + hackerResult);
    //    Console.WriteLine("Поточна довжина localNode: " + localNode.Chain.Count);


    //    // 2         // очікується - true, 5
    //    Console.WriteLine();
    //    Console.WriteLine("CHECK 2");

    //    bool honestResult =localNode.ReplaceChain(honestNetwork.Chain);

    //    Console.WriteLine("Чи прийнято чесний chain: " + honestResult);
    //    Console.WriteLine("Нова довжина localNode: " + localNode.Chain.Count);


    //    // 3        // очікується 200
    //    Console.WriteLine();
    //    Console.WriteLine("CHECK 3");

    //    decimal poolBalance = 0;

    //    if (localNode.Balances.ContainsKey(poolWallet.Address))
    //    {
    //        poolBalance = localNode.Balances[poolWallet.Address];
    //    }
    //    Console.WriteLine("Баланс Pool після консенсусу: " + poolBalance);

    //}

    //public static void TestMerkleAttack()
    //{
    //    var walletService = new WalletService();
    //    var hashingService = new HashingService();
    //    var transactionService = new TransactionService(walletService);
    //    var blockChain = new BlockChain(1);

    //    var aliceWallet = walletService.CreateWallet("Alice");
    //    var hackerWallet = walletService.CreateWallet("Hacker");

    //    blockChain.MinePendingTransactions(aliceWallet, 5);

    //    var tx1 = transactionService.CreateTransaction(aliceWallet, hackerWallet.Address, 5, 0.1m);
    //    var tx2 = transactionService.CreateTransaction(aliceWallet, hackerWallet.Address, 10, 0.1m);
    //    var tx3 = transactionService.CreateTransaction(aliceWallet, hackerWallet.Address, 15, 0.1m);
    //    var tx4 = transactionService.CreateTransaction(aliceWallet, hackerWallet.Address, 20, 0.1m);

    //    blockChain.AddTransaction(tx1);
    //    blockChain.AddTransaction(tx2);
    //    blockChain.AddTransaction(tx3);
    //    blockChain.AddTransaction(tx4);

    //    blockChain.MinePendingTransactions(aliceWallet, 10);

    //    var lastBlock = blockChain.Chain.Last();

    //    string originalMerkleRoot = hashingService.GetMerkleRoot(lastBlock.Transactions);
    //    Console.WriteLine("оригінальний Корінь Меркла " + originalMerkleRoot);

    //    //атака
    //    lastBlock.Transactions[1].Amount = 999999;

    //    //1 перевірка
    //    Console.WriteLine();
    //    Console.WriteLine("CHECK 1");

    //    int hackedId = lastBlock.Transactions[1].Id;

    //    var hackedHash = hashingService.ComputeSHA256(lastBlock.Transactions[1].ToRawString());
    //    var hackedTransaction =blockChain.GetTransactionByHash(hackedHash);

    //    if (hackedTransaction != null)
    //    {
    //        Console.WriteLine("Сума зламаної транзакції" + hackedTransaction.Amount);
    //    }
        


    //    //2 перевірка
    //    Console.WriteLine();
    //    Console.WriteLine("CHECK 2");

    //    string hackedMerkleRoot = hashingService.GetMerkleRoot(lastBlock.Transactions);

    //    Console.WriteLine("оригінальний Корінь Меркла " + originalMerkleRoot);
    //    Console.WriteLine("хакнутий Корінь Меркла " + hackedMerkleRoot);




    //    //3 перевірка
    //    Console.WriteLine();
    //    Console.WriteLine("CHECK 3");

    //    bool isValid = blockChain.isValid(blockChain.Chain);
    //    Console.WriteLine("Чи валідний блокчейн" + isValid);

    //}

    //public static void TestColdHack()
    //{
    //    // Сервіси
    //    var walletService = new WalletService();

    //    var transactionService = new TransactionService(walletService);

    //    // Blockchain
    //    var blockChain = new BlockChain(1);

    //    // Гаманці
    //    var aliceWallet = walletService.CreateWallet("Alice");
    //    var hackerWallet = walletService.CreateWallet("Hacker");

    //    // майнинг
    //    blockChain.MinePendingTransactions(aliceWallet, 5);

    //    // Транзакції
    //    var tx1 = transactionService.CreateTransaction(aliceWallet, hackerWallet.Address, 5, 0.1m);
    //    var tx2 = transactionService.CreateTransaction(aliceWallet, hackerWallet.Address, 10, 0.1m);

    //    blockChain.AddTransaction(tx1);
    //    blockChain.AddTransaction(tx2);

    //    // Новий блок
    //    blockChain.MinePendingTransactions(aliceWallet, 5);

    //    Console.WriteLine("Blockchain saved.");

    //    // імітація холодного взлому
    //    Console.WriteLine();

    //    string fileContent = File.ReadAllText("blockchain_date.dat");

    //    // Підробка суми
    //    fileContent = fileContent.Replace("\"Amount\":5", "\"Amount\":50000");

    //    File.WriteAllText( "blockchain_data.dat", fileContent);

    //    Console.WriteLine("File compromised!");

    //    // Перезапуск

    //    Console.WriteLine();
    //    Console.WriteLine("Перезапуск");

    //    var newBlockChain = new BlockChain(1);

    //    newBlockChain.LoadChainFromFile();

    //    // Перевірка 1

    //    Console.WriteLine();
    //    Console.WriteLine("Перевірка 1");

    //    Console.WriteLine("Перевірка компрометації ");

    //    // Перевірка 2

    //    Console.WriteLine();
    //    Console.WriteLine("Перевірка 2");

    //    Console.WriteLine("Довжина chain: " + newBlockChain.Chain.Count);

    //    // Очікується:0 або 1

    //    // Перевірка 3

    //    Console.WriteLine();
    //    Console.WriteLine("Перевірка 3");

    //    decimal hackerBalance = newBlockChain.GetBalance( hackerWallet.Address);

    //    Console.WriteLine("Баланс Hacker: " + hackerBalance);

    //    // Очікується: 0
    //}
}
