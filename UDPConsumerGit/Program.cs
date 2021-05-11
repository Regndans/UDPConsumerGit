using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using static UDPConsumerGit.Consumer;
using PushbulletSharp;
using PushbulletSharp.Models.Requests;
using PushbulletSharp.Models.Responses;

namespace UDPConsumerGit
{
    //Class for receiving broadcast and posting to rest + notification for user
    public class Program
    {

        //API uri
        private const string BaseUri = "https://restsense.azurewebsites.net/api/pir/";

        static void Main(string[] args)
        {

            // JEPPE o.m6FEoL5WPlV6fzumAYNumMnzpNsNVuDJ
            // MIKE o.NWignlPelQZuZ0esmenixTsHLoafUmzr
            // MORTEN o.9e201FtNDk2kuCWcxxllrykGcND47xBT

            #region Pushbullet
            //Pushbullet notification target device
            var apiKey = "o.m6FEoL5WPlV6fzumAYNumMnzpNsNVuDJ";
            PushbulletClient client = new PushbulletClient(apiKey);
            var devices = client.CurrentUsersDevices();
            var targetDevices = devices.Devices;


            //If secondary device wanted 
            //var apiKey2 = "o.9e201FtNDk2kuCWcxxllrykGcND47xBT";
            //PushbulletClient client2 = new PushbulletClient(apiKey2);
            //var devices2 = client2.CurrentUsersDevices();
            //var targetDevices2 = devices2.Devices;
            #endregion

            #region broadcastreceiving
            //Receive broadcast
            UdpClient udpServer = new UdpClient(7000);
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Broadcast, 7000);

            try
            {
                Console.WriteLine("Server is listening");


                Byte[] receiveBytes = udpServer.Receive(ref RemoteIpEndPoint);

                //Receive Json broadcast
                string receivedData = Encoding.ASCII.GetString(receiveBytes);
                string[] data = receivedData.Split(' ');
                string clientName = data[0];
                Console.WriteLine(receivedData);

                //Deserialize received Json
                SensorModel conData = JsonConvert.DeserializeObject<SensorModel>(receivedData);

                #region post & notification
                //POST to REST if motion is detected
                if (conData.Status == "Motion Detected")
                {
                    Console.WriteLine(conData.Name);
                    SensorModel newPirSensor = conData;
                    SensorModel p = Post<SensorModel, SensorModel>(BaseUri, newPirSensor).Result;
                    Console.WriteLine("Added: " + p);

                    //Send notification to targeted devices
                    foreach (var device in targetDevices)
                    {
                        PushNoteRequest request = new PushNoteRequest
                        {
                            DeviceIden = device.Iden,
                            Title = $"Motion detected from sensor: {conData.Name}! Time of detection: {conData.TimeOfDetection}",
                            Body = $"Message for: {device.Model}"
                        };

                        client.PushNote(request);

                    }

                    //Send notification to targeted secondary user
                    //foreach (var device in targetDevices2)
                    //{
                    //    PushNoteRequest request = new PushNoteRequest
                    //    {
                    //        DeviceIden = device.Iden,
                    //        Title = $"Motion detected from sensor: {conData.Name}! Time of detection: {conData.TimeOfDetection}",
                    //        Body = $"Message for: {device.Model}"
                    //    };
                    //    client2.PushNote(request);
                    //}


                }

                #endregion

            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            #endregion
        }
    }
}
