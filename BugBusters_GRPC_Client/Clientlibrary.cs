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
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Globalization;
using System.Drawing;
using System.Linq.Expressions;

namespace BugBusters_GRPC_Client
{
    public class Clientlibrary
    {

        #region props
        //theese will stay the same all the way trough
        PdService.PdServiceClient client;
        AsyncDuplexStreamingCall<CommandMessage, CommandMessage> call;
        IClientStreamWriter<CommandMessage> stream;

        //values will be changed depending on the command and the commands output
        string currentCommandId;
        Func<Task> currentEvent;
        //theese guys will change all the time
        Bitmap image;
        int cmdCounter;
        public List<MotorBike> Bikes = new List<MotorBike>();
        public List<ItemLocationModel> Items = new List<ItemLocationModel>();
        private int currentBikeID;
        private int currentPacketID;
        public int currentMoney;
        public List<List<Node>> grid;
        bool stopItems = false;
        int ownerId;
        #endregion




        public async Task DESTROY_THIS_SHIT()
        {
            await stream.CompleteAsync();
            call.Dispose();
            Console.WriteLine("FUCK THIS SHIT IM OUT,FUCK THIS SHIT IM OUT yeeeea");
        }

        #region server->client
        public async Task ReadTask()
        {
            await foreach (var res in this.call.ResponseStream.ReadAllAsync())
            {
                var cmdID = res.CommandId;
                string data = res.CommandData;
                if (cmdID != "ItemLocations")
                {
                    Console.WriteLine("\n>>>CommandID:" + res.CommandId + ", " + data);

                    if (cmdID == "BuyBikeResponse")
                    {
                        if (data == "0")
                        {
                            await Console.Out.WriteLineAsync("Could not buy new bike");
                        }
                        else if (data == "-1")
                        {
                            await Console.Out.WriteLineAsync("Could not buy new bike");
                            await DESTROY_THIS_SHIT();
                        }
                        else
                        {
                            MotorBike bike = new MotorBike(Convert.ToInt32(data), 2, true, 0);
                            Bikes.Add(bike);

                            await Console.Out.WriteLineAsync("\t> new bike added with id:" + data);
                            await currentEvent.Invoke();


                            //await BuyMine(bike.id, async () => {
                            //    await PlaceMine(bike.id, async () => {
                            //        //await SteerBike(bike.id, true, 0, async() => {});
                            //    });
                            //});
                        }
                    }
                    else if (cmdID == "BuyMineResponse")
                    {
                        if (data == "NOP")
                        {
                            await Console.Out.WriteLineAsync("\t> Something went horribly wrong i'm a teapot and i can make coffe" + data);
                        }
                        else if (data == "-1")
                        {
                            await Console.Out.WriteLineAsync("\tSomething broke..");
                        }
                        else
                        {
                            int index = Bikes.FindIndex(b => b.id == currentBikeID);
                            Bikes[index].mineCount = Bikes[index].mineCount + 3;
                            await currentEvent.Invoke();
                        }
                    }
                    else if (cmdID == "PlaceMineResponse")
                    {
                        if (data == "-1")
                        {
                            await Console.Out.WriteLineAsync("\tcould not place mine");
                        }
                        else
                        {
                            Console.WriteLine("mineId: " + data);
                            int index = Bikes.FindIndex(b => b.id == currentBikeID);
                            Bikes[index].mineCount--;
                            await Console.Out.WriteLineAsync("remaining mines in bike(" + currentBikeID + "): " + Bikes[index].mineCount);
                            await currentEvent.Invoke();
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
                            await currentEvent.Invoke();
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
                            int bikeindex = Bikes.FindIndex(b => b.Packets.Where(p => p.Id == currentPacketID).Any());
                            int packetIndex = Bikes[bikeindex].Packets.FindIndex(p => p.Id == currentPacketID);
                            Bikes[bikeindex].Packets.RemoveAt(packetIndex);
                            await currentEvent.Invoke();
                        }
                    }
                    else if (cmdID == "SteerBikeResponse")
                    {
                        if (data == "OK")
                        {
                            await Console.Out.WriteLineAsync("Bike steered successfully");
                            await currentEvent.Invoke();
                        }
                        else
                        {
                            await Console.Out.WriteLineAsync("No bike movement happened");
                        }
                    }


                }
                else
                {
                    //await Console.Out.WriteAsync("[UPDATE] > map updated");

                    //entity types: "DeliveryBike" "Packet" "Mine"
                    if (true)
                    {
                        Items = JsonConvert.DeserializeObject<ItemLocationModel[]>(data).ToList();

                        if (Bikes.Count() != 0)
                        {
                            foreach (MotorBike bike in Bikes)
                            {
                                async Task move()
                                {
                                    int degree = 0;

                                    ItemLocationModel bikeLocation = Items.FirstOrDefault(t => t.Id == bike.id);
                                    if (image.GetPixel(bikeLocation.X + 1, bikeLocation.Y) != System.Drawing.Color.Black) {
                                        degree = getDegreeFromNextNode(
                                            new Vector2(bikeLocation.X, bikeLocation.Y),
                                            new Vector2(bikeLocation.X + 1, bikeLocation.Y)
                                        );
                                        await Console.Out.WriteLineAsync($"{degree}");
                                    }
                                    else if (image.GetPixel(bikeLocation.X, bikeLocation.Y + 1) != System.Drawing.Color.Black) {
                                        degree = getDegreeFromNextNode(
                                            new Vector2(bikeLocation.X, bikeLocation.Y),
                                            new Vector2(bikeLocation.X, bikeLocation.Y + 1)
                                        );
                                        await Console.Out.WriteLineAsync($"{degree}");
                                    }
                                    else if (image.GetPixel(bikeLocation.X + 1, bikeLocation.Y + 1) != System.Drawing.Color.Black) {
                                        degree = getDegreeFromNextNode(
                                            new Vector2(bikeLocation.X, bikeLocation.Y),
                                            new Vector2(bikeLocation.X + 1, bikeLocation.Y + 1)
                                        );
                                        await Console.Out.WriteLineAsync($"{degree}");
                                    }
                                    await SteerBike(bike.id, true, degree, async () => { });


                                    //var result = FindNearestPacket(bike.id);
                                    //await Console.Out.WriteLineAsync($">>>{result.node.Center.X}, {result.node.Center.Y}");

                                    //var nextNode = result.node;
                                    //ItemLocationModel bikeLocation = Items.FirstOrDefault(t => t.Id == bike.id);

                                    //degree = getDegreeFromNextNode(
                                    //    new Vector2(bikeLocation.X, bikeLocation.Y),
                                    //    new Vector2(nextNode.Position.X, nextNode.Position.Y)
                                    //);
                                    //await Console.Out.WriteLineAsync($"{degree}");
                                    //await SteerBike(bike.id, true, degree, async () => { });
                                }
                                {
                                }
                                var currentbikeitem = Items.FirstOrDefault(t => t.Id == bike.id);
                                if (currentbikeitem != null)
                                {
                                    var userPackets = Items.Where(i => i.Type == "Packet" && bike.Packets.Where(p => i.Id == p.Id).Any());
                                    //if user arraived on packet destianation
                                    if (userPackets.Where(p => p.DestinationX == currentbikeitem.X && p.DestinationY == currentbikeitem.Y).Any())
                                    {
                                        var packetId = userPackets.Where(p => p.DestinationX == currentbikeitem.X && p.DestinationY == currentbikeitem.Y).Select(p => p.Id).First();
                                        await DropPacket(packetId, async () => { move(); });
                                    }
                                    //pickup packets
                                    else if (Items.Where(i => i.Type == "Packet" && currentbikeitem.X == i.X && currentbikeitem.Y == i.Y).Any())
                                    {
                                        var packetId = Items.Where(i => i.Type == "Packet" && currentbikeitem.X == i.X && currentbikeitem.Y == i.Y).Select(i => i.Id).First();
                                        await PickupPacket(bike.id, packetId, async () => { move(); });
                                    }
                                    else
                                    {
                                        move();
                                    }
                                }

                            }
                        }
                        else if (Bikes.Count() == 0)
                        {
                            var idk = Items.FirstOrDefault(i => i.Type == "DeliveryBike" && i.OwnerId == ownerId);
                            if (idk != null)
                            {
                                Console.Out.WriteLineAsync("Getting our bike back !");
                                var bikes = Items.Where(i => i.Type == "DeliveryBike" && i.OwnerId == ownerId).ToList();
                                foreach (ItemLocationModel bike in bikes)
                                {
                                    Bikes.Add(new MotorBike(bike.Id, 2, true, 0));
                                }
                                Thread.Sleep(3000);
                            }
                            else {
                                await BuyBike(async() => { });
                                Thread.Sleep(3000);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        int getDegreeFromNextNode(Vector2 current, Vector2 next)
        {
            Random rnsd = new Random();
            int asd = rnsd.Next(0, 3);
            if (asd == 0) {
                return rnsd.Next(0, 90);
            }
            if (asd == 1) {
                return rnsd.Next(90, 180);
            }
            else {
                return rnsd.Next(0, 360);
            }

            int degree = 0;
            if (current.X <= next.X)
            {
                //jobbra
                degree = 0;
                if (current.Y <= next.Y)
                {
                    //jobbra fel
                    degree = 315;
                }
                if (current.Y >= next.Y)
                {
                    //jobbra le
                    degree = 45;
                }
            }
            else if (current.X >= next.X)
            {
                //balra
                degree = 180;
                if (current.Y <= next.Y)
                {
                    //balra fel
                    degree = 225;
                }
                if (current.Y >= next.Y)
                {
                    //balra le
                    degree = 135;
                }
            }
            else
            { //x1 és x2 egyenlőek
                if (current.Y < next.Y)
                {
                    //fel
                    degree = 270;
                }
                else
                {
                    //le
                    degree = 90;
                }
            }
            return degree;
        }

        public Clientlibrary(
            PdService.PdServiceClient client,
            AsyncDuplexStreamingCall<CommandMessage, CommandMessage> call,
            int cmdCounter,
            Bitmap image,
            List<List<Node>> grid,
            int ownerId
            )
        {
            this.client = client;
            this.call = call;
            this.cmdCounter = cmdCounter;
            this.image = image;
            stream = call.RequestStream;
            currentMoney = 1100;
            this.grid = grid;
            this.ownerId = ownerId;
        }

        #region client->server
        public async Task Login(string teamName, string password, Func<Task> callback)
        {
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
            currentEvent = callback;
        }

        public async Task BuyBike(Func<Task> callback)
        {
            Console.WriteLine("\n>>> Buying new bike");
            cmdCounter++;
            currentCommandId = "BuyBike";
            var modelAsJSON = JsonConvert.SerializeObject("");

            await stream.WriteAsync(new CommandMessage
            {
                CmdCounter = cmdCounter,
                CommandId = currentCommandId,
                CommandData = modelAsJSON
            });
            currentEvent = callback;
        }

        public async Task BuyMine(int bikeId, Func<Task> callback)
        {
            Console.WriteLine(">>> BuyMine for bike with bikeId:" + bikeId);
            currentBikeID = bikeId;
            cmdCounter++;
            currentCommandId = "BuyMine";

            var modelAsJSON = JsonConvert.SerializeObject(new { bikeId = bikeId });

            await stream.WriteAsync(new CommandMessage
            {
                CmdCounter = cmdCounter,
                CommandId = currentCommandId,
                CommandData = modelAsJSON
            });
            currentEvent = callback;
        }

        public async Task PlaceMine(int bikeId, Func<Task> callback)
        {
            Console.WriteLine("\n>>> Placing mine");
            cmdCounter++;
            currentCommandId = "PlaceMine";
            currentBikeID = bikeId;
            var modelAsJSON = JsonConvert.SerializeObject(new { bikeId = bikeId });

            await stream.WriteAsync(new CommandMessage
            {
                CmdCounter = cmdCounter,
                CommandId = currentCommandId,
                CommandData = modelAsJSON
            });
            currentEvent = callback;
        }

        public async Task PickupPacket(int bikeId, int packetId, Func<Task> callback)
        {
            Console.WriteLine("\n>>> Picking up packet");
            cmdCounter++;
            var payload = new PickupPacketInputModel { bikeId = bikeId, packetId = packetId };
            currentCommandId = "PickupPacket";
            currentPacketID = packetId;
            currentBikeID = bikeId;
            var modelAsJSON = JsonConvert.SerializeObject(payload);

            await stream.WriteAsync(new CommandMessage
            {
                CmdCounter = cmdCounter,
                CommandId = currentCommandId,
                CommandData = modelAsJSON
            });
            currentEvent = callback;
        }

        public async Task DropPacket(int packetId, Func<Task> callback)
        {
            currentEvent = callback;
            Console.WriteLine("\n>>> Dropping packet");
            cmdCounter++;
            currentCommandId = "DropPacket";
            currentPacketID = packetId;
            var modelAsJSON = JsonConvert.SerializeObject(new { packetId = packetId });

            await stream.WriteAsync(new CommandMessage
            {
                CmdCounter = cmdCounter,
                CommandId = currentCommandId,
                CommandData = modelAsJSON
            });
        }

        public async Task SteerBike(int bikeId, bool isActive, int degree, Func<Task> callback)
        {
            currentEvent = callback;
            Console.WriteLine("\n>>> Steering bike");
            cmdCounter++;
            currentCommandId = "SteerBike";
            currentBikeID = bikeId;
            SteerBikeInputModel model = new SteerBikeInputModel
            {
                bikeId = bikeId,
                isActive = Convert.ToInt16(isActive),
                degree = degree
            };
            var modelAsJSON = JsonConvert.SerializeObject(model);

            await stream.WriteAsync(new CommandMessage
            {
                CmdCounter = cmdCounter,
                CommandId = currentCommandId,
                CommandData = modelAsJSON
            });
        }

        public async Task Cheat(Func<Task> callback)
        {
            currentEvent = callback;
            Console.WriteLine("\n>>> cheat activated :D");
            cmdCounter++;
            currentCommandId = "Cheat";

            var modelAsJSON = JsonConvert.SerializeObject("");

            await stream.WriteAsync(new CommandMessage
            {
                CmdCounter = cmdCounter,
                CommandId = currentCommandId,
                CommandData = modelAsJSON
            });
        }
        #endregion

        #region A* stuff
        public PacketStepModel FindNearestPacket(int bikeId)
        {

            //stopItems = true;
            var ItemsCopy = Items.ConvertAll(x => new ItemLocationModel
            {
                DestinationX = x.DestinationX,
                DestinationY = x.DestinationY,
                Id = x.Id,
                OwnerId = x.OwnerId,
                Type = x.Type,
                Value = x.Value,
                X = x.X,
                Y = x.Y
            });
            //stopItems = false;
            Console.WriteLine("Finding nearest packet");
            List<ItemLocationModel> Mines = ItemsCopy.Where(i => i.Type == "Mine").ToList();
            List<List<Node>> MineFreeNodes = grid.ToList();

            Console.WriteLine("creating new GRID");

            foreach (var mine in Mines) {
                MineFreeNodes[mine.X][mine.Y].Walkable = false;

                //    //felette->x - 1
                //    //jobbra->y + 1
                //    //alatta->x + 1
                //    //balra->y - 1

                //    //for (int i = mine.X - 40; i < mine.X + 40; i++) {
                //    //    for (int j = mine.Y - 40; j < mine.Y + 40; j++) {
                //    //        MineFreeNodes[i][j].Walkable = false;
                //    //    }
                //    //}
            }

            Console.WriteLine("new grid created");

            List<ItemLocationModel> bikes = ItemsCopy.Where(t => t.Type == "DeliveryBike").ToList();
            ItemLocationModel bike = ItemsCopy.FirstOrDefault(t => t.Id == bikeId);

            //Graphics g = Graphics.FromImage(image);
            //Pen myPen = new Pen(System.Drawing.Color.Red, 5);
            //g.DrawRectangle(myPen, bike.X, bike.Y, 2, 2);
            //image.Save("jozsi.png");
            //myPen.Dispose();

            Dictionary<double, PacketStepModel> packetScores = new Dictionary<double, PacketStepModel>();
            List<ItemLocationModel> packets = ItemsCopy.Where(i => i.Type == "Packet").ToList();

            int counter = 0;
            foreach (ItemLocationModel packet in packets)
            {
                //myPen = new Pen(System.Drawing.Color.Green, 5);
                //g.DrawRectangle(myPen, packet.X, packet.Y, 10, 10);
                //image.Save("jozsi.png");
                //myPen.Dispose();

                var scoreAndPacketStep = getPathAndScore(bike, packet, MineFreeNodes);
                if (scoreAndPacketStep.packetStep != null)
                {
                    packetScores.Add(scoreAndPacketStep.score, scoreAndPacketStep.packetStep);
                    counter++;
                }
                if (counter == 5)
                {
                    break;
                }
            }
            ;
            if (packetScores.Count() != 0)
            {
                var minIndex = packetScores.OrderBy(i => i.Key).Select(i => i.Key).First();
                return packetScores[minIndex];
            }
            else
            {
                return null;
            }
        }

        public dynamic getPathAndScore(ItemLocationModel bike, ItemLocationModel packet, List<List<Node>> MineFreeNodes)
        {
            A_star pathFinder = new A_star(MineFreeNodes);
            Stack<Node> ToPacket = pathFinder.FindPath(new Vector2(bike.X, bike.Y), new Vector2(packet.X, packet.Y));
            Stack<Node> ToDestination = pathFinder.FindPath(new Vector2(packet.X, packet.Y), new Vector2(packet.DestinationX, packet.DestinationY));

            if (ToDestination.Count() > 0)
            {
                if (ToPacket.Count() < ToDestination.Count() && Bikes.FirstOrDefault(b => b.id == bike.Id).Packets.Where(p => p.Id == packet.Id).Any())
                {
                    int s = ToDestination.Count();
                    double _score = s;
                    return new { score = _score, packetStep = new PacketStepModel() { node = ToDestination.Pop(), packet = packet } };

                }
                else
                {
                    int s = ToPacket.Count() + ToDestination.Count();
                    double _score = s;
                    return new { score = _score, packetStep = new PacketStepModel() { node = ToPacket.Pop(), packet = packet } };
                }
            }
            else { return null; }
        }


        #endregion
    }
}


