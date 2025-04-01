using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Http;
using System.IO;
using System.Net.Sockets;
using Serilog;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient _tcpClient = new TcpClient();
        private readonly string _connectionString; //connectionString - host:port

        public MainWindow()
        {
            InitializeComponent();

            Log.Logger = new LoggerConfiguration().CreateLogger();

            using(var read = new StreamReader("setting.txt"))
            {
                _connectionString = read.ReadLine().Split('=')[1];
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var requestData = inputBox.Text;

            outputBox.Text+=requestData;
            inputBox.Clear();
            outputBox.Text += "\n";

            int index = _connectionString.IndexOf(':');
            string host = "";

            for (int i = 0; i < index; i++)
            {
                host += _connectionString[i];
            }

            int port = 0;
            string _port = "";

            for (int i = index + 1; i < _connectionString.Length; i++)
            {
                _port += _connectionString[i];
            }

            port = int.Parse(_port);

            using (TcpClient client = new TcpClient(host, port))
            {
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] data = Encoding.UTF8.GetBytes(requestData);

                    await stream.WriteAsync(data, 0, data.Length);
                    Log.Information("Данные отправлены");

                    byte[] buffer = new byte[1024];
                    int bytesRead;

                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                    {
                        string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Log.Information($"Получено: {receivedData}");
                        outputBox.Text += $"Получено: {receivedData} \t DateTime: {DateTime.Now} \n";
                    }
                }
            }
        }
    }
}
