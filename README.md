# TaleSpire-InternetServerPlugin
Plugin which can be used by other plugins to allow communication between clients. Based on a Internet Server solution.

## Internet Server Dependency

The plugin has a default address for a free hosting site which is hosting a copy of the needed Internet Server.
Eventually I will post the code for such a server (it is really small) so that peolpe can host their own server
using the same free hosting site or a different site.

You can use the default server for testing but if you are going to use it heavily, please host your own copy.

## Usage

The InternetServer plugin can be started at any time. It does not required all clients to be connected because
there is an option for client to read historical data if they wish. This will typically depend on the plugin
implementation that this InternetServer plugin is used in.

##.Connect(Action<string[]> messageReceivedCallback, string session, string user, string url, bool ignoreFirst, bool ignoreOwn)

The messageReceivedCllback is a method taking in a string[] parameter which is called each time messages are received from the Internet Server.
Since the communication is polling,  multiple messages may have been posted since the last communication check and thus the parameter is an
array of messages.

The session is a unique identifier that groups messages being used for the same purpose. This allows the server to be used
by multiple groups of players at once without posting messages from one group to another. Suggested value for this parameter
is a concatenation of the CampaignId and the BoardId.

The user is a unique identification for the client. It is not exposed to other clients but is used to eliminate messages
posted by the client if the ignoreOwn parameter is true.

The url is the host and page of the Internet Server used to distribute messages. The default value is my free hosted server
which users can use for testing.

The ignoreFirst parameter indicates if all messages read in the first poll should be processed or ignored. If this value
is false then when a client connects all historical messages will be processed. If this vlaue is true, then all the
historical messages will be ignored and only new mesages will be process. Which setting is correct depends on the plugin
in which this Internet Server is being used. Default false;

The ignoreOwn parameter indicates if messages posted by the client should be received in the callback. If this value is
true then any messages posted by the client will not appear in the received messages callback. If the vlaue is false then
all messages, including those posted by the client, will be received in the callback.


##.Send(message)

The message parameter is a string representing the message to be posted to the Internet Server.
The connect method must have been used in order to use this method.

##.Clean(e)

The message triggers the Internet Server to perform database cleaning maintenance.
Typically this method does not need to be called because it is automatically called on connection.

##.Disconnect(e)

Stops the client from checking for Internet Server messages.

