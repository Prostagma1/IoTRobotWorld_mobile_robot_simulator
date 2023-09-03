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


        UdpClient udpClient;
        Thread thread;

        int portIN, portOUT;

        public Form1()
        {
            myDelegate = new ShowMessage(TestDelegate);
            InitializeComponent();
        }
        int a = 0;
        private void TestDelegate(string message)
        {
            a++;
            label3.Text = a.ToString() + " " + message;
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
                        Invoke(myDelegate, new object[] { message });
                    }
                }
                catch
                {
                    string errmessage = "RemoteHost lost";
                    Invoke(myDelegate, new object[] { errmessage });
                }
            }
        }
        private void SendMessage(string s)
        {
            if (udpClient != null)
            {
                IPAddress ip = IPAddress.Parse("127.0.0.1");
                IPEndPoint ipEndPoint = new IPEndPoint(ip, portOUT);
                byte[] content = Encoding.ASCII.GetBytes(s);
                try
                {
                    int count = udpClient.Send(content, content.Length, ipEndPoint);
                }
                catch
                {

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
