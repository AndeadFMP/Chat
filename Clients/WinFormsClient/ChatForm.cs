﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Andead.Chat.Client.Entities;
using Andead.Chat.Client.Interfaces;

namespace Andead.Chat.Client.WinForms
{
    public partial class ChatForm : Form
    {
        internal readonly IServiceClient Client;
        private Timer _onlineCountTimer;

        public ChatForm(IServiceClient client)
        {
            Client = client;

            client.MessageReceived += ClientOnMessageReceived;

            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _onlineCountTimer = new Timer
            {
                Interval = 1000,
                Enabled = true
            };
            _onlineCountTimer.Tick += OnTimerOnTick;
            _onlineCountTimer.Start();
        }

        private int? _onlineCount;

        private async void OnTimerOnTick(object sender, EventArgs args)
        {
            int? onlineCount = await Client.GetOnlineCountAsync();
            if (_onlineCount == onlineCount)
            {
                return;
            }

            _onlineCount = onlineCount;
            UpdateOnlineCount(onlineCount);

            string[] names = await Client.GetNamesOnlineAsync();
            UpdateNames(names.ToArray<object>());
        }

        private void UpdateNames(object[] names)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string[]>(UpdateNames), names);
                return;
            }

            namesListBox.Items.Clear();
            namesListBox.Items.AddRange(names);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Client.MessageReceived -= ClientOnMessageReceived;

            _onlineCountTimer?.Stop();

            base.OnClosing(e);
        }

        private void UpdateOnlineCount(int? onlineCount)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int?>(UpdateOnlineCount), onlineCount);
                return;
            }

            Text = onlineCount.HasValue ? $"Chat ({onlineCount} users)" : "Chat";
        }

        private void ClientOnMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ClientOnMessageReceived(sender, args)));
                return;
            }

            string message = args.Message;
            chatTextBox.AppendText(message + Environment.NewLine);
        }

        private async void button1_Click(Object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(messageTextBox.Text))
            {
                return;
            }

            string message = messageTextBox.Text;
            messageTextBox.Clear();

            await Client.SendAsync(message);
        }

        private void textBox1_TextChanged(Object sender, EventArgs e)
        {
            sendButton.Enabled = ((TextBox) sender).Text.Length > 0;
        }

        private void namesListBox_Click(Object sender, EventArgs e)
        {
            var selectedName = ((ListBox)sender).SelectedItem as string;
            if (!string.IsNullOrEmpty(selectedName))
            {
                messageTextBox.AppendText($"{selectedName}, ");
            }
        }
    }
}