using BugBusters_GRPC_Client.Models;
using Google.Protobuf;
using Grpc.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugBusters_GRPC_Client {
    public class Clientlibrary {

        #region props
        //theese will stay the same all the way trough
        PdService.PdServiceClient client;
        AsyncDuplexStreamingCall<CommandMessage, CommandMessage> call;

        //values will be changed depending on the command and the commands output
        public Action<string> currentReturnEvent;
        string currentCommandId;

        //theese guys will change all the time
        ByteString mapImagePNG;
        int cmdCounter;
        #endregion

        public async Task ReadTask()
        {
            await foreach (var res in this.call.ResponseStream.ReadAllAsync())
            {
                string data = res.CommandData;
                if (res.CommandId != "ItemLocations")
                {
                    Console.WriteLine("\n>>>CommandID:" + res.CommandId + ", " + data.Substring(0, 10));

                    currentReturnEvent.Invoke(data);
                }
                else if (res.CommandId == "ItemLocations")
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
        }


        public async Task Login(string teamName, string password, Action<string> callback)
        {
            Console.WriteLine(">>> Login");
            //AsyncDuplexStreamingCall<CommandMessage, CommandMessage> call = client.CommunicateWithStreams();
            var req = call.RequestStream;

            LoginInputModel model = new LoginInputModel { teamName = teamName, password = password };
            var modelAsJSON = JsonConvert.SerializeObject(model);
            cmdCounter++;

            await call.RequestStream.WriteAsync(new CommandMessage
            {
                CmdCounter = cmdCounter,
                CommandId = "Login",
                CommandData = modelAsJSON
            });
            await call.RequestStream.CompleteAsync();
            currentReturnEvent = callback;
            await Console.Out.WriteLineAsync(">>> Login closed");
        }





    }
}
