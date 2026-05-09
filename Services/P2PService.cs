using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Blockchain.Models;

namespace Blockchain.Services
{
    //Сервіс для P2P (peer-to-peer) взаємодії між вузлами блокчейну
    public class P2PService
    {
        private readonly BlockChain _blockchain;

        //Список підключених пірів
        private readonly List<TcpClient> _pears = new List<TcpClient>();

        //Порт сервера за замовчуванням
        public int Port { get; private set; } = 5000;

        public P2PService(BlockChain blockChain)
        {
            _blockchain = blockChain;
        }


        //запуск Р2Р сервера на вказаном порту
        public void StartServer(int port)
        {
            //Запуск прослуховування
            Port = port;
            var listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();
            Console.WriteLine("Server Start");

            //Окремий потік для прийому нових підключень
            Task.Run(() =>
            {
                while (true)
                {
                    //Очікування нового клієнта
                    var client = listener.AcceptTcpClient();
                    Console.WriteLine("New peer connected");

                    //Додавання піра до списку
                    _pears.Add(client);

                    //Запуск обробки клієнта
                    Task.Run(() => HendleClient(client));
                }
            });
        }

        //метод для підключення до іншого пір-сервера за вказаною ір-адресою та портом
        public void ConnectToPeer(string ip, int port)
        {
            var client = new TcpClient();
            client.Connect(ip, port);
            Console.WriteLine($"Connect to pear {ip}:{port}");
            _pears.Add(client);
            Task.Run(() => HendleClient(client));
        }

        //метод для обробки взаємодії з клієнтом (піром)
        private void HendleClient(TcpClient client)
        {
            //Отримання мережевого потоку
            var stream = client.GetStream();

            //Створення reader для читання повідомлень
            var reader = new StreamReader(stream);

            while (client.Connected)
            {
                try
                {
                    //Читання JSON повідомлення
                    string json = reader.ReadLine();
                    if (!string.IsNullOrEmpty(json))
                    {
                        //Десеріалізація повідомлення
                        var message = JsonSerializer.Deserialize<P2PMessage>(json);

                        //Обробка повідомлення
                        ProcessMessage(message);
                    }
                }
                catch (Exception ex)
                {
                    //Видалення клієнта при помилці
                    _pears.Remove(client);
                    Console.WriteLine($"Error handling client: {ex.Message}");
                    break;
                }
            }
        }

        //метод для обробки отриманого повідомлення від клієнта
        private void ProcessMessage(P2PMessage? message)
        {
            //обробляєм повідомлення в залежності від його типу
            if (message.Type == MessageType.BroadcastBlock)
            {
                //Перетворюємо JSON назад у блок
                var newBlock = JsonSerializer.Deserialize<Block>(message.Data);

                //обчислюєм хеш
                var hashingService = new HashingService();
                var calculatedHash = hashingService.ComputeHash(newBlock);

                //створюєм рядок з нулями зі складності блоку
                var targetHash = new string('0', newBlock.Difficulty);

                //Перевірка хешу чи він співпадає з обчисленим та вимогам складності
                if (calculatedHash == newBlock.Hash && calculatedHash.StartsWith(targetHash))
                {
                    _blockchain.Chain.Add(newBlock);
                    Console.WriteLine($"New block added: {newBlock.Index}");
                }
            }
            //Обробка нової транзакції
            else if (message.Type == MessageType.BroadcastTransaction)
            {
                //Перетворюємо JSON назад у Transaction
                var newTransaction = JsonSerializer.Deserialize<Transaction>(message.Data);

                //Додаємо транзакцію у пул очікування
                _blockchain.AddTransaction(newTransaction);
            }
        }

        //метод для широкомовної відправки повідомлення всім підключеним пірам
        public void BroadCast(MessageType messageType, object data)
        {
            //Створення P2P повідомлення
            var message = new P2PMessage
            {
                Type = messageType,
                Data = JsonSerializer.Serialize(data)
            };

            string json = JsonSerializer.Serialize(message);

            //Відправка всім підключеним пірам
            foreach (var peer in _pears)
            {
                try
                {
                    var stream = peer.GetStream();
                    var writer = new StreamWriter(stream) { AutoFlush = true };

                    //Відправка повідомлення
                    writer.WriteLine(json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error broadcasting to peer: {ex.Message}");
                }
            }
        }
    }
}
