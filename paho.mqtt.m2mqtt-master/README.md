# M2Mqtt

![](images/M2Mqtt_Short_Logo.png)

MQTT Client Library for .Net and WinRT

*Project Description*

M2Mqtt is a MQTT client available for all .Net platforms (.Net Framework, .Net Compact Framework and .Net Micro Framework) and WinRT platforms (Windows 8.1, Windows Phone 8.1 and Windows 10) for Internet of Things and M2M communication.

MQTT, short for Message Queue Telemetry Transport, is a light weight messaging protocol that enables embedded devices with limited resources to perform asynchronous communication on a constrained network.

MQTT protocol is based on publish/subscribe pattern so that a client can subscribe to one or more topics and receive messages that other clients publish on these topics.

This sample is a library contains an MQTT client that you can use to connect to any MQTT broker. It is developed in C# language and works on all the following .Net platforms :

* .Net Framework (up to 4.5)
* .Net Compact Framework 3.5 & 3.9 (for Windows Embedded Compact 7 / 2013)
* .Net Micro Framework 4.2 & 4.3
* Mono (for Linux O.S.)

There is also the support for WinRT platforms :

* Windows 8.1
* Windows Phone 8.1
* Windows 10

It can be used on Windows O.S, Windows Embedded Compact 7 / 2013 and Linux O.S. (thanks to Mono Project).

The project has an official website here : https://m2mqtt.wordpress.com/

The binaries for all platforms are also available as package from Nuget web site  https://www.nuget.org/packages/M2Mqtt/

For all information about MQTT protocol, please visit official web site  http://mqtt.org/.

Follow the project on Twitter [@m2mqtt](https://twitter.com/M2Mqtt) and [Facebook](https://www.facebook.com/m2mqtt).

*Building the source code*

The library is available for the following solution and project files :

* M2Mqtt.sln : solution for Visual Studio that contains projects file for .Net Framework, .Net Compact Framework 3.9, .Net Micro Framework 4.2, .Net Micro Framework 4.3 and WinRT (a portable class library) for Windows 8.1, Window Phone 8.1 and Windows 10 applications
* M2MqttVS2008.sln : solution for Visual Studio 2008 that contains project file for .Net Compact Framework 3.5;

To build sample based on .Net Micro Framework (4.2 and 4.3) you need to download .Net Micro Framework SDK from the official CodePlex web site : https://netmf.codeplex.com/

To build sample based on .Net Compact Framework 3.9 you need to download Application Builder for Windows Embedded Compact 2013 from here : http://www.microsoft.com/en-us/download/details.aspx?id=38819

*SSL/TLS support*

For SSL/TLS feature, the definition of the symbol "SSL" is needed before compile the project.
On the repository, this symbol is already defined and all assemblies (needed for SSL/TLS) are referenced (for Debug and Release configuration).
If you want to disable SSL/TLS feature, so that you can reduce memory occupation, you can delete "SSL" symbol and remove all assemblies referenced for SSL/TLS.
However, you can leave the default project configuration and set "secure" parameter to false and "cacert" to null for MqttClient constructor (these are already default if you don't specify any values).

ATTENTION : .Net Micro Framework supports up to TLSV1

*Example*

The M2Mqtt library provides a main class MqttClient that represents the MQTT client to connect to a broker. You can connect to the broker providing its IP address or host name and optionally some parameters related to MQTT protocol.

After connecting to the broker you can use Publish() method to publish a message to a topic and Subscribe() method to subscribe to a topic and receive message published on it. The MqttClient class is events based so that you receive an event when a message is published to a topic you subscribed to. You can receive event when a message publishing is complete, you have subscribed or unsubscribed to a topic.

Following an example of client subscriber to a topic :

```
... 
 
// create client instance 
MqttClient client = new MqttClient(IPAddress.Parse(MQTT_BROKER_ADDRESS)); 
 
// register to message received 
client.MqttMsgPublishReceived += client_MqttMsgPublishReceived; 
 
string clientId = Guid.NewGuid().ToString(); 
client.Connect(clientId); 
 
// subscribe to the topic "/home/temperature" with QoS 2 
client.Subscribe(new string[] { "/home/temperature" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE }); 
 
... 
 
static void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e) 
{ 
// handle message received 
} 
```

Following an example of client publisher to a topic :

```
... 
 
// create client instance 
MqttClient client = new MqttClient(IPAddress.Parse(MQTT_BROKER_ADDRESS)); 
 
string clientId = Guid.NewGuid().ToString(); 
client.Connect(clientId); 
 
string strValue = Convert.ToString(value); 
 
// publish a message on "/home/temperature" topic with QoS 2 
client.Publish("/home/temperature", Encoding.UTF8.GetBytes(strValue), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false); 
 
...
```
