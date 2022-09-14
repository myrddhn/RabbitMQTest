using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace RabbitMQTest
{
    public partial class frmRabbitMQTest : Form
    {
        IConnection conn;
        IModel channel;
        EventingBasicConsumer consumer;

        public frmRabbitMQTest()
        {
            InitializeComponent();
        }

        private void frmRabbitMQTest_Load(object sender, EventArgs e)
        {
            Log.Logger = new LoggerConfiguration().CreateLogger();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger.Information("App start");

            ConnectionFactory factory = new ConnectionFactory();
            factory.UserName = "andi";
            factory.Password = "idna";
            factory.VirtualHost = "artest";
            factory.HostName = "darwinistic.com";

            conn = factory.CreateConnection();

            if (conn.IsOpen)
            {
                Log.Logger.Information("Connection opened successfully");
                channel = conn.CreateModel();

                var rc = channel.QueueDeclarePassive("testqueue");
                Log.Logger.Information("msg count: {0}", rc.MessageCount);

                consumer = new EventingBasicConsumer(channel);
                string consumerTag = channel.BasicConsume("testqueue", false, consumer);
                consumer.Received += Consumer_Received;
            }
            else
            {
                Log.Logger.Information("Failed");
            }
        }

        private void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            Log.Logger.Information("msg received: {0}", Encoding.UTF8.GetString(e.Body.ToArray()));
            channel.BasicAck(e.DeliveryTag, false);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            channel.BasicPublish("", "testqueue", null, Encoding.UTF8.GetBytes("Hello world :-) " + new Random().Next().ToString() + " " + txtPayload.Text));
            Log.Logger.Information("Message published");
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            channel.Close();
            conn.Close();

            Application.Exit();
        }
    }
}
