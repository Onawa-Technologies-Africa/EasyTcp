using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EasyTcp3.ClientUtils;
using EasyTcp3.ClientUtils.Async;
using EasyTcp3.Server;
using EasyTcp3.Server.ServerUtils;
using NUnit.Framework;

namespace EasyTcp3.Test.Client
{
    public class StreamAsync
    {
        [Test]
        public async Task Stream1() //Client -> -(Stream)> Server     (Client sends message to server)
        {
            ushort port = TestHelper.GetPort();
            using var server = new EasyTcpServer();
            server.Start(port);
            
            using var client = new EasyTcpClient();
            Assert.IsTrue(client.Connect(IPAddress.Any, port));
            
            string testData = "123", data = null;

            server.OnDataReceive += async (sender, message) => //Receive stream from client
            {
                await using var stream = new MemoryStream();
                await message.ReceiveStreamAsync(stream);
                data = Encoding.UTF8.GetString(stream.ToArray());
            };

            //Send stream to server
            await using var dataStream = new MemoryStream(Encoding.UTF8.GetBytes(testData));
            client.Send("Stream");
            await client.SendStreamAsync(dataStream);

            TestHelper.WaitWhileTrue(() => data == null);
            Assert.AreEqual(testData, data);
        }

        [Test]
        public void Stream2()//Client -> Server -(Stream)> Client     (Client requests stream from server)
        {
            ushort port = TestHelper.GetPort();
            using var server = new EasyTcpServer();
            server.Start(port);
            
            string testData = "123", data = null;

            using var client = new EasyTcpClient();
            Assert.IsTrue(client.Connect(IPAddress.Any, port));

            server.OnDataReceive += async (sender, message) => //Send stream if client requests
            {
                await using var dataStream = new MemoryStream(Encoding.UTF8.GetBytes(testData));
                message.Client.Send("Stream");
                await message.Client.SendStreamAsync(dataStream);
            };
            
            client.OnDataReceive += async (sender, message) => //Receive stream from server
            {
                await using var stream = new MemoryStream();
                await message.ReceiveStreamAsync(stream);
                data = Encoding.UTF8.GetString(stream.ToArray());
            };
            client.Send("GetStream");//Request stream

            TestHelper.WaitWhileTrue(() => data == null);
            Assert.AreEqual(testData, data);
        }
    }
}