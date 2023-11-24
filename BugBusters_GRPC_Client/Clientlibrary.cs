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

namespace BugBusters_GRPC_Client {
    public class Clientlibrary {

        #region props
        //theese will stay the same all the way trough
        PdService.PdServiceClient client;
        AsyncDuplexStreamingCall<CommandMessage, CommandMessage> call;
        IClientStreamWriter<CommandMessage> stream;

        //values will be changed depending on the command and the commands output
        public Action<string> currentReturnEvent;
        string currentCommandId;

        //theese guys will change all the time
        ByteString mapImagePNG;
        int cmdCounter;
        public List<MotorBike> Bikes = new List<MotorBike>();
        private int currentBikeID;
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
                    currentReturnEvent.Invoke(data);
                    
                    if (cmdID == "BuyBikeResponse") {
                        if (data == "0") {
                            await Console.Out.WriteLineAsync("Could not buy new bike");
                        }
                        else {
                            MotorBike bike = new MotorBike(Convert.ToInt32(data), 0);
                            Bikes.Add(bike);

                            await Console.Out.WriteLineAsync("\t> new bike added with id:" + data);
                            currentBikeID = bike.id;
                            
                            await BuyMine((string resp) => { }, bike.id);
                        }                        
                    }
                    if( cmdID == "BuyMineResponse") {
                        if (data == "NOP") {
                            await Console.Out.WriteLineAsync("\t> Something wen horribly wrong i'm a teapot and i can make coffe" + data);
                        }
                        else {
                            int index = Bikes.FindIndex(b => b.id == currentBikeID);
                            Bikes[index].mineCount++;
                        }                        
                    }
                }
                else
                {
                    // TODO - save map info
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
                this.mapImagePNG = mapImagePng;
                this.stream = call.RequestStream;
        }


        public async Task Login(string teamName, string password, Action<string> callback)
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

            currentReturnEvent = callback;
        }

        public async Task BuyBike(Action<string> callback) {
            Console.WriteLine("\n>>> Buying new bike");
            cmdCounter++;
            currentCommandId = "BuyBike";
            var modelAsJSON = JsonConvert.SerializeObject("");

            await stream.WriteAsync(new CommandMessage {
                CmdCounter = cmdCounter,
                CommandId = currentCommandId,
                CommandData = modelAsJSON
            });

            currentReturnEvent = callback;
        }


        public async Task BuyMine(Action<string> callback, int bikeId)
        {
            Console.WriteLine(">>> BuyMine");
            currentBikeID = bikeId;
            cmdCounter++;
            currentCommandId = "BuyMine";

            await stream.WriteAsync(new CommandMessage
            {
                CmdCounter = cmdCounter,
                CommandId = currentCommandId,
                CommandData = bikeId.ToString()
            });

            currentReturnEvent = callback;
        }
    }
}
