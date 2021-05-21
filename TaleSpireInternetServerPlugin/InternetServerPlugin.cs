using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using TMPro;
using UnityEngine;

namespace InternetServerPlugin
{
    [BepInPlugin("org.d20armyknife.plugins.InternetServer", "Internet Server Plug-In", "1.0.0.0")]
    public class InternetServerPlugin : BaseUnityPlugin
    {
        // Internet server client
        private Network.Client client = null;

        /// <summary>
        /// This function is called once by TaleSpire
        /// </summary>
        void Awake()
        {
            UnityEngine.Debug.Log("Internet Server Plugin is now available");
        }

        /// <summary>
        /// This function is called periodically by TaleSpire.
        /// </summary>
        void Update()
        {
        }

        /// <summary>
        /// Method to start receiving messages from the InternetServer
        /// </summary>
        /// <param name="messageReceivedCallback">Calback function which gets received messages</param>
        /// <param name="session">Unique identification for all related messages (e.g. CampaignId.BoardId)</param>
        /// <param name="user">Unique user identification if using ignoreOwn</param>
        /// <param name="url">URL of the Internet Server providing messaging</param>
        /// <param name="ignoreFirst">When set all historical messages will be ignored. Only new messages will be push to the callback</param>
        /// <param name="ignoreOwn">When set all messages matching the specified user will be ignored</param>
        public void Connect(Action<string[]> messageReceivedCallback, string session, string user ="", string url = "http://talespirepipe.byethost18.com/MessageServerSQL.php", bool ignoreFirst = false, bool ignoreOwn = true)
        {
            // Make a client
            client = new Network.Client(url, session, user, ignoreFirst, ignoreOwn);
            // Start message processing
            client.Connect(messageReceivedCallback);
        }

        /// <summary>
        /// Method to stop mesage processing from the Internet Server
        /// </summary>
        public void Disconnect()
        {
            // Stop message processing
            client.Disconnect();
        }

        /// <summary>
        /// Method used to send a message to the Internet Server
        /// </summary>
        /// <param name="msg">String to be sent (cannot contain | character)</param>
        public void Send(string msg)
        {
            client.SendMessage(msg);
        }

        /// <summary>
        /// Method used to tell the internet Server to check for old messages and remove them
        /// </summary>
        public void Clean()
        {
            client.SendClean();
        }
    }
}
