using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace IoTRobotWorld_mobile_robot_simulator
{
    public partial class Form1 : Form
    {
        public delegate void ShowMessage(string message);
        public ShowMessage myDelegate;
        public ShowMessage myDelegateLog;

        UdpClient udpClient;
        Thread thread;

        int portIN, portOUT;

        public Form1()
        {
            myDelegate = new ShowMessage(ParseDelegate);
            myDelegateLog = new ShowMessage(LogDelegate);

            InitializeComponent();
        }
        private void LogDelegate(string s)
        {
            if (listBox1.Items.Count > 200) listBox1.Items.Clear();

            listBox1.Items.Add(s);
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
            listBox1.SelectedIndex = -1;
        }
        private void ParseDelegate(string message)
        {

        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Старт")
            {
                if (int.TryParse(textBox1.Text, out portOUT) && int.TryParse(textBox2.Text, out portIN))
                {
                    button1.Text = "Стоп";
                    panel1.Enabled = false;
                    CloseAllThreads();

                    udpClient = new UdpClient(portIN);
                    thread = new Thread(new ThreadStart(ReceiveMessage));
                    thread.Start();
                    myDelegateLog("Сервер запущен");
                }
                else
                {
                    MessageBox.Show("Порт(ы) введен(ы) не правильно!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                button1.Text = "Старт";
                panel1.Enabled = true;
                CloseAllThreads();
            }
        }
        private void ReceiveMessage()
        {
            while (true)
            {
                try
                {

                    IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Any, portIN);
                    byte[] content = udpClient.Receive(ref remoteIPEndPoint);
                    if (content.Length > 0)
                    {
                        string message = Encoding.ASCII.GetString(content);
                        Invoke(myDelegateLog, new object[] { ">" + message });
                    }
                }
                catch
                {
                    Invoke(myDelegateLog, new object[] { "Нет подключения" });
                }
            }
        }
        private void SendMessage(string s)  // Где-то добавть \n для отправки json
        {
            if (udpClient != null)
            {
                IPAddress ip = IPAddress.Parse("127.0.0.1");
                IPEndPoint ipEndPoint = new IPEndPoint(ip, portOUT);
                byte[] content = Encoding.ASCII.GetBytes(s);
                try
                {
                    int count = udpClient.Send(content, content.Length, ipEndPoint);
                    Invoke(myDelegateLog, new object[] { "<" + s });
                }
                catch
                {
                    Invoke(myDelegateLog, new object[] { "Ошибка отправки" });
                }

            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseAllThreads();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SendMessage(textBox3.Text);
        }

        private void CloseAllThreads()
        {
            thread?.Abort();
            udpClient?.Close();
        }
    }
}
