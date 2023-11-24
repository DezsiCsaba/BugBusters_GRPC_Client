using System.Threading.Tasks;
using Grpc.Net.Client;
using BugBusters_GRPC_Client;
using Google.Protobuf;
using Grpc.Core;
using System;
using BugBusters_GRPC_Client.Models;
using System.Text.Json;
using Newtonsoft.Json;

Console.WriteLine(">>> Starting up GRPC client..");

//getting team image
string url = "https://external-content.duckduckgo.com/iu/?u=https://www.sipsa.net/wp-content/uploads/2020/04/bugbusters_ok-1024x576.png&f=1&nofb=1&ipt=ee84cf52e56821195c8aafd620f7e7a75e818ba7dae444da34f40b9962f7bcd9&ipo=images&fbclid=IwAR3_jN5NO5sGdkpj0MIbxcOQA6yxKu64Z5P__cEUpfQ341iB86xkyrvwsxI";
byte[] imageBytes;
ByteString imagebyteStr;
    using (var http_client = new HttpClient()) {
        using (var response = await http_client.GetAsync(url)) {
            imageBytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        }
    }

imagebyteStr = ByteString.CopyFrom(imageBytes);

#region props
//----------PROPS----------
ByteString mapImagePNG;
object streams;
int cmdCounter;

//----------PROPS----------
#endregion

// The port number must match the port of the gRPC server.
using var channel = GrpcChannel.ForAddress("http://10.8.9.121:9080");
var client = new PdService.PdServiceClient(channel);
var call = client.CommunicateWithStreams();

//RegisterTeam (teamName, teamPassword, teamImagePng) 
var registerReply = await client.RegisterTeamAsync(
    new RegistrationRequestMessage { 
        TeamName = "BugBusters",
        TeamPassword = "password",
        TeamImagePng = imagebyteStr
    }
);
mapImagePNG = registerReply.MapImagePng;
Console.WriteLine("Registration complete with the following id : " + registerReply.TeamId);


object Communicate(CommandMessage msg){

    return streams = client.CommunicateWithStreams();
}
 
async Task<T> ReadTask<T>(AsyncDuplexStreamingCall<CommandMessage,CommandMessage> call)
{
    await foreach (var res in call.ResponseStream.ReadAllAsync())
    {
        if (res.CommandId != "ItemLocations") {
            Console.WriteLine("CommandID:" + res.CommandId + ", " + res.CommandData.Substring(0, 10));
        }
        
        var data = res.CommandData;
        //return JsonSerializer.Deserialize<T>(data);
    }
    return default(T);
}


async Task Login(string teamName, string password)
{
    Console.WriteLine(">>> Login");
    //AsyncDuplexStreamingCall<CommandMessage, CommandMessage> call = client.CommunicateWithStreams();
    var req = call.RequestStream;

    LoginInputModel model = new LoginInputModel { teamName = teamName,password = password };
    var modelAsJSON = JsonConvert.SerializeObject(model);

    await call.RequestStream.WriteAsync(new CommandMessage {
        CmdCounter = 1,
        CommandId = "Login",
        CommandData = modelAsJSON
    });
    await call.RequestStream.CompleteAsync();
    await Console.Out.WriteLineAsync(">>> Login closed");
}

await Login("BugBusters", "password");

await ReadTask<loginOutputModel>(call);

Console.WriteLine("Press any key to exit...");
Console.ReadKey();