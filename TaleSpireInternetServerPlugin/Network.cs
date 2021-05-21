using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Network
{
    public class Client
    {
        /// <summary>
        /// IP address of the internet server being used to communicate
        /// </summary>
        private string url = "";

        /// <summary>
        /// Unique identifier for the session to allow different campaigns/boards to use the same server
        /// </summary>
        private string session = "";

        /// <summary>
        /// Unique identifier for the user. This allows ignoring of own messages
        /// </summary>
        private string userName = "";

        /// <summary>
        /// Holds the web Navigator
        /// </summary>
        private Navigator client = null;

        /// <summary>
        /// Last processed transaction (passed to server to obtain only new transactions)
        /// </summary>
        private UInt64 trans = 0;

        /// <summary>
        /// Unique identifier for the user. This allows ignoring of own messages
        /// </summary>
        private bool ignoreOwn = true;

        /// <summary>
        /// Holds the ignoreFirst setting. When true, all of the transactions obtained in the first server request will be ignored
        /// (this means that only new transactions will be processed). Otherwise all historical tranactions will be processed.
        /// </summary>
        private bool ignoreFirst = false;

        /// <summary>
        /// Holds the status if this is the first client request (used to implement the ignoreFirst functionality)
        /// </summary>
        private bool isFirst = true;

        /// <summary>
        /// Holds the setting for the number of miliseconds that the client pauses (without blocking) after each poll of the server.
        /// Defaults is 0. Lower values place larger CPU stress on the plugin and the internet server. Higher values demand less CPU
        /// but cause less frequent updates. Note: when using higher values, updates are not lost. There is just more updates processed
        /// in each poll so it can produce a stutter effect if the plugin is being used for things like movement.
        /// </summary>
        private int backOff = 0;

        /// <summary>
        /// Holds the callback function
        /// </summary>
        private Action<string[]> callback = null;

        /// <summary>
        /// Holds an indication if the client is "connected". This is a fake status since the architecture is connectionless
        /// (i.e. a new HTTP request is made for each poll). This status indicates if the client is making polls and passing
        /// the results to the callback or not.
        /// </summary>
        private bool isConnected = false;

        /// <summary>
        /// Constructor for creating a client
        /// </summary>
        /// <param name="ip">Ip address of the internet server used for communication</param>
        /// <param name="ignoreFirst">Indicates if the first set of transactions (i.e. historical transactions) should be ignored</param>
        /// <param name="backOff">Indicates the amount of wait time in ms between polls of the server for new transactions</param>
        public Client(string url, string session, string user, bool ignoreFirst = false, bool ignoreOwn=true, int backOff = 0)
        {
            this.url = url;
            this.session = session;
            this.userName = user;
            this.ignoreFirst = ignoreFirst;
            this.ignoreOwn = ignoreOwn;
            this.backOff = backOff;
            client = new Navigator(ProcessResponse);
        }

        /// <summary>
        /// Method to connect a client to the Sync Mod Server
        /// </summary>
        /// <param name="ip">IP address to connect to</param>
        /// <param name="callback">Callback function that receives any messages sent from the server</param>
        public void Connect(Action<string[]> callback)
        {
            isConnected = true;
            this.callback = callback;
            this.SendClean();
        }

        /// <summary>
        /// Method to stop the client from polling the server for updates. Any poll in progress will finish.
        /// </summary>
        public void Disconnect()
        {
            isConnected = false;
            this.callback = null;
        }

        /// <summary>
        /// Method to initiate a internet server maintenance clean 
        /// </summary>
        /// <param name="message">String representation of the message</param>
        public void SendClean()
        {
            string content = url;
            SendContent(content);
        }

        /// <summary>
        /// Method to send messages to the server (for others to obtain)
        /// </summary>
        /// <param name="message">String representation of the message</param>
        public void SendMessage(string message)
        {
            string encoded = System.Web.HttpUtility.UrlEncode(message.Replace("|",""));
            string content = url + "?session=" + session + "&user=" + userName + "&content=" + encoded;
            SendContent(content);
        }

        /// <summary>
        /// Method to poll the server for new transactions (messages) 
        /// </summary>
        private void GetMessages()
        {
            string excludeClause = "";
            if (ignoreOwn) { excludeClause = "&exclude=" + userName; }
            string content = url + "?session=" + this.session + excludeClause + "&trans=" + this.trans;
            SendContent(content);
        }

        /// <summary>
        /// Method to carry out communication with the Internet Server
        /// </summary>
        /// <param name="content"></param>
        private void SendContent(string content)
        {
            Task task = MessageLoopWorker.Run(client.Navigate, content);
            task.Wait();
        }

        /// <summary>
        /// Download progress event used to determine when the download is compelte
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Event arguments for the progress</param>
        private void ProcessResponse(string content)
        {
            if (content != null)
            {
                if (content != "")
                {
                    // Update the transaction number. This wil always be the first line of the server content followed by \r\n
                    this.trans = UInt64.Parse(content.Substring(0, content.IndexOf("|")));
                    // Remove the first line thus leaving any server transactions (messages)
                    content = content.Substring(content.IndexOf("|") + "|".Length).Trim();
                    // Push the transactions (messages) to the callback 
                    if ((!isFirst || !ignoreFirst) && content != "") { this.callback(content.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries)); }
                    isFirst = false;
                    // Execute the backoff non-blocking wait (this will wait the indicated time but free the CPU to do other tasks)
                    if (this.backOff != 0) { Task.Run(() => { Console.ReadLine(); }).Wait(backOff); }
                    // If the client is not disconnected, perform next server poll
                    if (isConnected) { GetMessages(); }
                }
            }
        }
    }
}
