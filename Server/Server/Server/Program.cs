﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Net;
using Serilog;
using System.Threading.Tasks;
using System.Text;

namespace Server
{
    public class Program
    {
        private static readonly int _port = 8888;
        private static DataBaseService dbService;

        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.File("logs.txt").WriteTo.Console().CreateLogger();

            TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), _port);

            using(var read = new StreamReader("../../setting.txt"))
            {
                string connectionString = read.ReadLine();

                if (string.IsNullOrEmpty(connectionString))
                {
                    Log.Error("Строка соединения пустая!");
                    Environment.Exit(1);
                }

                dbService = new DataBaseService(connectionString);
            }

            try
            {
                Log.Information("Инициализация базы данных...");

                try
                {
                    await dbService.InitializeAsync();
                }
                catch (Exception ex)
                {
                    Log.Error($"Ошибка DB: {ex.Message}");
                    throw;
                }

                Log.Information("База данных инициализирована успешно!");

                listener.Start();

                Log.Information("Сервер запущен и ожидает подключение...");

                while (true)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();

                    Log.Information("Клиент подключен!");

                    _ = HandleClientAsync(client);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Ошибка сервера: {ex.Message}");
            }
            finally
            {
                listener.Stop();
            }

            Console.ReadKey();
        }

        static async Task HandleClientAsync(TcpClient client)
        {
            if (dbService == null)
            {
                Log.Error("Сервис базы данных не инициализирован!");
                return;
            }

            try
            {
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead;

                    string[] line = new string[3];

                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                    {
                        string clientMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Log.Information($"Получено сообщение от клиента: {clientMessage}");

                        line = clientMessage.Split(' ');
                        byte[] response = new byte[1024];
                        int sum = 0;

                        if (line[0] == "add" || line[0] == "getall")
                        {
                            Log.Information("Данные пришли корректные.");

                            if (line[0] == "add")
                            {
                                sum = int.Parse(line[1]) + int.Parse(line[2]);

                                Item item = new Item()
                                {
                                    Command = line[0],
                                    Result = sum,
                                    DateOfTime = DateTime.Now
                                };

                                await dbService.AddItemAsync(item);

                                response = Encoding.UTF8.GetBytes($"Status: Ok, Response: {item.ToString()}");
                            }
                            else if(line[0] == "getall")
                            {
                                var items = await dbService.GetAllItemsAsync();

                                response = ConvertUsersToByteArray(items);
                            }
                        }
                        else
                        {
                            response = Encoding.UTF8.GetBytes("Error enter command!");
                            Log.Information("Error enter command!");
                        }

                        await stream.WriteAsync(response, 0, response.Length);
                        Log.Information("Ответ отправлен клиенту.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Ошибка обработки клиента: {ex.Message}");
            }
            finally
            {
                client.Close();
                Log.Information("Клиент закрыт");
            }
        }

        static byte[] ConvertUsersToByteArray(IEnumerable<Item> items)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in items)
            {
                sb.AppendLine($"Id={item.Id}, Command={item.Command}, Result={item.Result}, DateOfTime={item.DateOfTime}");
            }
            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}