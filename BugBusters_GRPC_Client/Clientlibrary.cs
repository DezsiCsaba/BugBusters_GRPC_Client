using BugBusters_GRPC_Client.Models;
using Google.Protobuf;
using Grpc.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace BugBusters_GRPC_Client {
    public class Clientlibrary {

        #region props
        //theese will stay the same all the way trough
        PdService.PdServiceClient client;
        AsyncDuplexStreamingCall<CommandMessage, CommandMessage> call;
        IClientStreamWriter<CommandMessage> stream;

        //values will be changed depending on the command and the commands output
        string currentCommandId;
        Func<Task> currentEvent;
        //theese guys will change all the time
        ByteString mapImagePNG;
        int cmdCounter;
        public List<MotorBike> Bikes = new List<MotorBike>();
        public List<ItemLocationModel> Items = new List<ItemLocationModel>();
        private int currentBikeID;
        private int currentPacketID;
        public int currentMoney;
        #endregion

        public async Task DESTROY_THIS_SHIT()
        {
            await stream.CompleteAsync();
        }
        public async Task ReadTask()
        {
            await foreach (var res in this.call.ResponseStream.ReadAllAsync())
            {
                var cmdID = res.CommandId;
                string data = res.CommandData;
                if (cmdID != "ItemLocations")
                {
                    Console.WriteLine("\n>>>CommandID:" + res.CommandId + ", " + data);
                    await currentEvent.Invoke();
                    if (cmdID == "CheatResponse") {
                        Console.WriteLine("more moneeey");
                    }
                    if (cmdID == "BuyBikeResponse") {
                        if (data == "0") {
                            await Console.Out.WriteLineAsync("Could not buy new bike");
                        }
                        else {
                            MotorBike bike = new MotorBike(Convert.ToInt32(data), 2, true, 0);
                            Bikes.Add(bike);

                            await Console.Out.WriteLineAsync("\t> new bike added with id:" + data);

                            await BuyMine(bike.id, async () => {
                                await PlaceMine(bike.id, async () => {
                                    //await SteerBike(bike.id, true, 0, async() => {});
                                });
                            });
                        }
                    }
                    else if( cmdID == "BuyMineResponse") {
                        if (data == "NOP") {
                            await Console.Out.WriteLineAsync("\t> Something went horribly wrong i'm a teapot and i can make coffe" + data);
                        }
                        else if (data == "-1") {
                            await Console.Out.WriteLineAsync("\tSomething broke..");
                        }
                        else {
                            int index = Bikes.FindIndex(b => b.id == currentBikeID);
                            Bikes[index].mineCount = Bikes[index].mineCount+3;
                        }                        
                    }
                    else if (cmdID == "PlaceMineResponse") {
                        if(data == "-1") {
                            await Console.Out.WriteLineAsync("\tcould not place mine");
                        }
                        else {
                            Console.WriteLine("mineId: " + data);
                            int index = Bikes.FindIndex(b => b.id == currentBikeID);
                            Bikes[index].mineCount--;
                            await Console.Out.WriteLineAsync("remaining mines in bike(" + currentBikeID + "): " + Bikes[index].mineCount);
                        }
                    }
                    else if (cmdID == "PickupPacketResponse")
                    {
                        if (data == "FAIL")
                        {
                            await Console.Out.WriteLineAsync("\t> PickupPacket: Something wen horribly wrong i'm a teapot and i can make coffe" + data);
                        }
                        else
                        {
                            int index = Bikes.FindIndex(b => b.id == currentBikeID);
                            Bikes[index].Packets.Add(new Packet { Id = currentPacketID });
                        }
                    }
                    else if (cmdID == "DropPacketResponse")
                    {
                        if (data == "FAIL")
                        {
                            await Console.Out.WriteLineAsync("\t> DropPacket: Something wen horribly wrong i'm a teapot and i can make coffe" + data);
                        }
                        else
                        {
                            int bikeindex = Bikes.FindIndex(b => b.Packets.Where(p=> p.Id==currentPacketID).Any());
                            int packetIndex = Bikes[bikeindex].Packets.FindIndex(p => p.Id==currentPacketID);
                            Bikes[bikeindex].Packets.RemoveAt(packetIndex);
                        }
                    }
                    else if (cmdID == "SteerBikeResponse") {
                        if (data == "OK") {
                            await Console.Out.WriteLineAsync("Bike steered successfully");
                        }
                        else {
                            await Console.Out.WriteLineAsync("No bike movement happened");
                        }
                    }

                }
                else
                {
                    //await Console.Out.WriteAsync("[UPDATE] > map updated");
                    Items = JsonConvert.DeserializeObject<ItemLocationModel[]>(data).ToList();
                    //foreach(ItemLocationModel item in Items)
                    //{
                    //    await Console.Out.WriteLineAsync("\n");
                    //    foreach (PropertyInfo prop in typeof(ItemLocationModel).GetProperties()) {
                    //        Console.WriteLine($"\t " + prop.Name + " : " + prop.GetValue(item));
                    //    }                        
                    //} 
                }
            }
        }


        public Clientlibrary(
            PdService.PdServiceClient client, 
            AsyncDuplexStreamingCall<CommandMessage, CommandMessage> call, 
            int cmdCounter,
            ByteString mapImagePng
            ){
                this.client = client;
                this.call = call;
                this.cmdCounter = cmdCounter;
                mapImagePNG = mapImagePng;
                stream = call.RequestStream;
                currentMoney = 1100;
        }


        public async Task Login(string teamName, string password, Func<Task> callback)
        {
            currentEvent = callback;
            Console.WriteLine(">>> Login");
            //AsyncDuplexStreamingCall<CommandMessage, CommandMessage> call = client.CommunicateWithStreams();

            LoginInputModel model = new LoginInputModel { teamName = teamName, password = password };
            var modelAsJSON = JsonConvert.SerializeObject(model);
            cmdCounter++;

            await stream.WriteAsync(new CommandMessage
            {
                CmdCounter = cmdCounter,
                CommandId = "Login",
                CommandData = modelAsJSON
            });
        }

        public async Task BuyBike(Func<Task> callback) {
            currentEvent = callback;
            Console.WriteLine("\n>>> Buying new bike");
            cmdCounter++;
            currentCommandId = "BuyBike";
            var modelAsJSON = JsonConvert.SerializeObject("");

            await stream.WriteAsync(new CommandMessage {
                CmdCounter = cmdCounter,
                CommandId = currentCommandId,
                CommandData = modelAsJSON
            });

        }

        public async Task BuyMine(int bikeId, Func<Task> callback)
        {
            currentEvent = callback;
            Console.WriteLine(">>> BuyMine for bike with bikeId:" + bikeId);
            currentBikeID = bikeId;
            cmdCounter++;
            currentCommandId = "BuyMine";

            var modelAsJSON = JsonConvert.SerializeObject(new {bikeId = bikeId});

            await stream.WriteAsync(new CommandMessage
            {
                CmdCounter = cmdCounter,
                CommandId = currentCommandId,
                CommandData = modelAsJSON
            });
        }

        public async Task PlaceMine(int bikeId, Func<Task> callback) {
            currentEvent = callback;
            Console.WriteLine("\n>>> Placing mine");
            cmdCounter++;
            currentCommandId = "PlaceMine";
            currentBikeID = bikeId;
            var modelAsJSON = JsonConvert.SerializeObject(new {bikeId = bikeId});

            await stream.WriteAsync(new CommandMessage {
                CmdCounter = cmdCounter,
                CommandId = currentCommandId,
                CommandData = modelAsJSON
            });
        }

        public async Task PickupPacket(int bikeId, int packetId, Func<Task> callback)
        {
            currentEvent = callback;
            Console.WriteLine("\n>>> Picking up packet");
            cmdCounter++;
            var payload= new PickupPacketInputModel { bikeId=bikeId, packetId=packetId };
            currentCommandId = "PickupPacket";
            currentPacketID= packetId;
            currentBikeID= bikeId;
            var modelAsJSON = JsonConvert.SerializeObject(payload);

            await stream.WriteAsync(new CommandMessage
            {
                CmdCounter = cmdCounter,
                CommandId = currentCommandId,
                CommandData = modelAsJSON
            });
        }

        public async Task DropPacket(int packetId, Func<Task> callback) {
            currentEvent = callback;
            Console.WriteLine("\n>>> Dropping packet");
            cmdCounter++;
            currentCommandId = "DropPacket";
            currentPacketID = packetId;
            var modelAsJSON = JsonConvert.SerializeObject(new {packetId = packetId});

            await stream.WriteAsync(new CommandMessage {
                CmdCounter = cmdCounter,
                CommandId = currentCommandId,
                CommandData = modelAsJSON
            });
        }

        public async Task SteerBike(int bikeId, bool isActive, int degree, Func<Task> callback) {
            currentEvent = callback;
            Console.WriteLine("\n>>> Steering bike");
            cmdCounter++;
            currentCommandId = "SteerBike";
            currentBikeID = bikeId;
            SteerBikeInputModel model = new SteerBikeInputModel { 
                bikeId = bikeId, isActive = Convert.ToInt16(isActive), degree = degree
            };
            var modelAsJSON = JsonConvert.SerializeObject(model);

            await stream.WriteAsync(new CommandMessage {
                CmdCounter = cmdCounter,
                CommandId = currentCommandId,
                CommandData = modelAsJSON
            });
        }

        public async Task Cheat(Func<Task> callback) {
            currentEvent = callback;
            Console.WriteLine("\n>>> cheat activated :D");
            cmdCounter++;
            currentCommandId = "Cheat";

            var modelAsJSON = JsonConvert.SerializeObject("");

            await stream.WriteAsync(new CommandMessage {
                CmdCounter = cmdCounter,
                CommandId = currentCommandId,
                CommandData = modelAsJSON
            });
        }
    }
}
