using System;
using System.Collections.Generic;
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
        private const string SensorUri = "https://restsense.azurewebsites.net/api/Sensor/";
        private const string MotionUri = "https://restsense.azurewebsites.net/api/Motion/";

        static void Main(string[] args)
        {

            // JEPPE o.m6FEoL5WPlV6fzumAYNumMnzpNsNVuDJ
            // MIKE o.NWignlPelQZuZ0esmenixTsHLoafUmzr
            // MORTEN o.9e201FtNDk2kuCWcxxllrykGcND47xBT

            #region Pushbullet
            //Pushbullet notification target device
            var apiKey = "o.NWignlPelQZuZ0esmenixTsHLoafUmzr";
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

                while (true)
                {


                    Byte[] receiveBytes = udpServer.Receive(ref RemoteIpEndPoint);

                    //Receive Json broadcast
                    string receivedData = Encoding.ASCII.GetString(receiveBytes);
                    string[] data = receivedData.Split(' ');
                    string clientName = data[0];
                    Console.WriteLine(receivedData);

                    //Deserialize received Json + Split into MotionModel and SensorModel
                    MotionModel motionData = JsonConvert.DeserializeObject<MotionModel>(receivedData);
                    SensorModel sensorData = JsonConvert.DeserializeObject<SensorModel>(receivedData);

                    
                    #region post & notification
                    
                    //POST to REST if motion is detected
                    if (motionData.Status == "Motion Detected")
                    {
                        List<SensorModel> sensorModels = GetAllDataAsync<SensorModel>(SensorUri).Result;
                        
                        foreach (var x in sensorModels)
                        {
                            if (x.SensorId == motionData.SensorId && x.Active == true)
                            {
                                MotionModel newMotionData = motionData;
                                MotionModel p = Post<MotionModel, MotionModel>(MotionUri, newMotionData).Result;
                                Console.WriteLine("Added: " + p);

                                //Send notification to targeted devices
                                foreach (var device in targetDevices)
                                {
                                    PushNoteRequest request = new PushNoteRequest
                                    {
                                        DeviceIden = device.Iden,
                                        Title =
                                            $"Motion detected from sensor: {sensorData.Name}! Time of detection: {motionData.TimeOfDetection}",
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

                        }
                        


                        


                    }
                    #endregion
                    
                }





            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            #endregion

        }
    }
}
