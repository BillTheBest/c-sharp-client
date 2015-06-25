using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Dynamic;

using Newtonsoft.Json.Linq;

using flowthings;

namespace flowthingsWindowsDemo
{
    public partial class FlowWSForm : Form
    {

        API api;
        Token MY_TOKEN = new Token("youraccount", "yourtoken");
        const string FLOW_ID = "yourflow";

        public FlowWSForm()
        {
            InitializeComponent();

            this.api = new API(MY_TOKEN);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            this.api.websocket.OnOpen += websocket_OnOpen;
            this.api.websocket.OnClose += websocket_onClose;
            this.api.websocket.OnError += websocket_onError;
            this.api.websocket.OnMessage += websocket_onMessage;

            this.api.websocket.Connect();
        }

        void websocket_onMessage(string resource, dynamic value)
        {
            this.BeginInvoke((Action)(() => { this.txtOutput.AppendText("Message received: " + value.ToString() + "\n"); }));
        }

        void websocket_onError(string message)
        {
            this.BeginInvoke((Action)(() => { this.txtOutput.AppendText("Websocket error: " + message + "\n"); }));
        }

        void websocket_onClose(string reason, bool wasClean)
        {
            this.BeginInvoke((Action)(() => { 
                this.txtOutput.AppendText("Websocket closed...\n");
                this.timer1.Stop();
            }));
        }

        void websocket_OnOpen()
        {
            this.BeginInvoke((Action)(() => { 
                this.txtOutput.AppendText("Websocket opened...\n");
                this.timer1.Start();
            }));
            this.api.websocket.Subscribe(FLOW_ID);
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            this.BeginInvoke((Action)(() => { this.txtOutput.AppendText("Creating drop...\n"); }));

            dynamic value = new ExpandoObject();
            value.name = "new drop";
            value.price = 33.02;

            this.api.websocket.CreateDrop(FLOW_ID, value);
        }

        private void FlowWSForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.api.websocket.Dispose();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            this.api.websocket.Delete("drop", new string[] { this.txtDropId.Text });
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.api.websocket.connected) this.api.websocket.SendHeartbeat();
        }
    }
}
