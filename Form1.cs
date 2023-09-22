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

        public delegate void StartPassing();
        public StartPassing myDelegateStartPassing;

        UdpClient udpClient;
        Thread thread, threadPassingLab;

        int portIN, portOUT;

        int[] NLeRe = new int[3];
        bool automaticSlalom = false;
        public Form1()
        {
            myDelegate = new ShowMessage(ParseDelegate);
            myDelegateLog = new ShowMessage(LogDelegate);
            myDelegateStartPassing = new StartPassing(PassingLabyrinth);
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
            message = message.Replace("\"", "").Replace(" ", "");
            if (String.IsNullOrWhiteSpace(message)) return;
            for (int i = 31; i > 15; i--)
            {
                int indexStart = message.IndexOf(':');
                int indexEnd = message.IndexOfAny(new char[] { ',', '}' });
                if (indexEnd == -1 || indexStart == -1) break;
                Controls[i].Text = message.Substring(indexStart + 1, indexEnd - indexStart - 1);
                message = message.Remove(0, indexEnd + 1);
            }

            NLeRe[0] = int.Parse(Controls[31].Text);
            NLeRe[1] = int.Parse(Controls[27].Text);
            NLeRe[2] = int.Parse(Controls[26].Text);

            if (automaticSlalom)
            {
                automaticSlalom = false;
                threadPassingLab?.Abort();
                threadPassingLab = new Thread(new ThreadStart(PassingLabyrinth));
                threadPassingLab.Start();
            }

        }
        private void PassingLabyrinth()
        {
            TurnLeft();
            Forward(1500);
            TurnRight();
            Forward(1600);
            TurnRight();

            Forward(3000);
            TurnLeft();
            Forward(1600);
            TurnLeft();

            Forward(3200);
            TurnRight();
            Forward(1000);
            TurnRight();

            Forward(3000);
            TurnLeft();
            Forward(1600);
            TurnLeft();

            Forward(3000);
            TurnRight();
            Forward(1000);
            TurnRight();

            Forward(3000);
            TurnLeft();
            Forward(1200);
            TurnLeft();
            Forward(1500);

            JSONSendMessage();
        }
        private void Forward(int goal)
        {
            int current = NLeRe[1];
            for (byte i = 0; i < 2; i++)
                JSONSendMessage(100);

            while (Math.Abs(NLeRe[1] - current) <= goal)
            {

            }
        }
        private void TurnLeft()
        {
            int goal = 700;
            int current = NLeRe[1];
            for (byte i = 0; i < 2; i++)
                JSONSendMessage(0, 100);

            while (Math.Abs(NLeRe[1] - current) <= goal)
            {

            }
        }
        private void TurnRight()
        {
            int goal = 750;
            int current = NLeRe[1];
            for (byte i = 0; i < 2; i++)
                JSONSendMessage(0, -100);

            while (Math.Abs(NLeRe[1] - current) <= goal)
            {

            }
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
                        Invoke(myDelegate, new object[] { message });

                    }
                }
                catch
                {
                    Invoke(myDelegateLog, new object[] { "Нет подключения" });
                }
            }
        }
        private void SendMessage(string s)
        {
            if (udpClient != null)
            {
                IPAddress ip = IPAddress.Parse("127.0.0.1");
                IPEndPoint ipEndPoint = new IPEndPoint(ip, portOUT);
                byte[] content = Encoding.ASCII.GetBytes(s + "\n");
                try
                {
                    udpClient.Send(content, content.Length, ipEndPoint);
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
        private void JSONSendMessage(int F = 0, int B = 0)
        {
            string str = $"{{\"N\":{NLeRe[0] + 1},\"F\":{F},\"B\":{B}}}";
            SendMessage(str);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            automaticSlalom = checkBox1.Checked;
        }

        private void CloseAllThreads()
        {
            thread?.Abort();
            threadPassingLab?.Abort();
            udpClient?.Close();
        }
    }
}
