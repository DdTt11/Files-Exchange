using System.Net;
using System.Net.Sockets;
// ReSharper disable FunctionNeverReturns

namespace ClientServer
{
    public partial class Form1 : Form
    {
        private ServerObject _serverObject;
        public Form1()
        {
            InitializeComponent();
            _serverObject = new ServerObject(listBox1, comboBox1, textBox3, textBox2, textBox1);
            var th = new Thread(_serverObject.Listen);
            textBox3.Text = $@"Сервер включён {DateTime.Now}{Environment.NewLine}";
            th.Start();
        }

        class ServerObject
        {
            public ListBox ListBox1 { get; }
            private ComboBox ComboBox1 { get; }
            public TextBox TextBox1 { get; }
            public TextBox TextBox3 { get; }
            private TextBox TextBox2 { get; }
            public TcpClient? Tcpclient { get; set; }
            protected internal ServerObject(ListBox listBox1, ComboBox comboBox1, TextBox textBox3, TextBox textBox2, TextBox textBox1)
            {
                ListBox1 = listBox1;
                ComboBox1 = comboBox1;
                TextBox3 = textBox3;
                TextBox2 = textBox2;
                TextBox1 = textBox1;
            }

            private readonly TcpListener _tcpListener = new(IPAddress.Loopback, 8888);
            protected internal void Listen()
            {
                _tcpListener.Start();
                while (true)
                {
                    Tcpclient = _tcpListener.AcceptTcpClient();
                    TextBox3.Text += @"Клиент соединился "
                                     + DateTime.Now
                                     + @" с адреса "
                                     + IPAddress.Parse(((IPEndPoint)Tcpclient.Client.RemoteEndPoint!).Address.ToString())
                                     + @":"
                                     + ((IPEndPoint)Tcpclient.Client.RemoteEndPoint).Port + Environment.NewLine;
                    TextBox1.Text += @"" + IPAddress.Parse(((IPEndPoint)Tcpclient.Client.RemoteEndPoint!).Address.ToString()) 
                                        + @":"
                                        + ((IPEndPoint)Tcpclient.Client.RemoteEndPoint).Port;
                    GetDirectories();
                    ComboBox1.Text = ComboBox1.Items[0].ToString();
                    GetFiles(ComboBox1.Items[0].ToString()!);
                }
            }
            
            public void GetDirectories()
            {
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                TextBox2.Text += @"Клиент получил от сервера диски: " + Environment.NewLine;
                foreach (DriveInfo d in allDrives)
                {
                    ComboBox1.Items.Add(d.Name);
                    TextBox2.Text += d + Environment.NewLine;
                }
            }

            public void GetFiles(string dir)
            {
                try
                {
                    var dirs = Directory.GetDirectories(dir);
                    TextBox2.Text += @"Клиент получил от сервера директории: " + Environment.NewLine; 
                    foreach (string s in dirs)
                    {
                        ListBox1.Items.Add(s);
                        TextBox2.Text += s + Environment.NewLine;
                    }
                    var files = Directory.GetFiles(dir);
                    TextBox2.Text += @"Клиент получил от сервера файлы: " + Environment.NewLine;
                    foreach (string s in files)
                    {
                        ListBox1.Items.Add(s);
                        TextBox2.Text += s + Environment.NewLine;
                    } 
                }
                catch
                {
                    MessageBox.Show(@"Директория или файл недоступны из-за настроек доступа");
                    GetFiles(ComboBox1.SelectedItem.ToString()!);
                }
            }

            public void GetFilesChanged(string dir)
            {
                try
                {
                    if (Path.GetExtension(ListBox1.SelectedItem.ToString()) == "")
                    {
                        ListBox1.Items.Clear();
                        GetFiles(dir);
                    }
                    else if (Path.GetExtension(ListBox1.SelectedItem.ToString()) == ".txt" &&
                             Path.GetExtension(ListBox1.SelectedItem.ToString()) != "")
                    {
                        FileInfo file = new FileInfo(ListBox1.SelectedItem.ToString()!);
                        long size = file.Length;
                        MessageBox.Show(File.ReadAllText(ListBox1.SelectedItem.ToString()!) 
                                        + Environment.NewLine 
                                        + @"Размер: " 
                                        + size 
                                        + @" байт");
                    }
                    else if (Path.GetExtension(ListBox1.SelectedItem.ToString()) != ".txt" &&
                             Path.GetExtension(ListBox1.SelectedItem.ToString()) != "")
                    {
                        FileInfo file = new FileInfo(ListBox1.SelectedItem.ToString()!);
                        long size = file.Length;
                        MessageBox.Show(@"Файл: " 
                                        + ListBox1.SelectedItem 
                                        + Environment.NewLine 
                                        + @"Размер: " 
                                        + size 
                                        + @" байт");
                    }

                    if (ListBox1.Items.Count == 0)
                    {
                        MessageBox.Show(@"В директории нет элементов");
                    }
                }
                catch
                {
                    MessageBox.Show(@"Файл недоступен из-за настроек доступа");
                }
            }
        }
        
        class ClientObject
        {
            private TextBox Textbox { get; }
            private ListBox? Listbox { get; }
            protected internal ClientObject(TextBox textBox, ListBox listbox)
            {
                Textbox = textBox;
                Listbox = listbox;
            }

            public void Process()
            {
                try 
                {
                        var message = Listbox?.SelectedItem?.ToString();
                        Textbox.Text += @"Сервер получил " + DateTime.Now + @": " + message + Environment.NewLine;
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.ToString());
                        Textbox.Text += @"Клиент разорвал соединение";
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            string? dir = comboBox1.SelectedItem.ToString();
            _serverObject.GetFiles(dir!);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClientObject clientObject = new ClientObject(_serverObject.TextBox3, _serverObject.ListBox1);
            clientObject.Process();
            string? dir = listBox1.SelectedItem.ToString();
            _serverObject.GetFilesChanged(dir!);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var host = IPAddress.Loopback;
            const int port = 8888;
            using var client = new TcpClient();

            try
            {
                client.Connect(host, port);
                button2.Enabled = false;
            }
            catch (Exception)
            {
                MessageBox.Show(@"Не удалось подключиться");
            }
        }
    }
}