using System.Threading.Tasks;
using Grpc.Net.Client;
using BugBusters_GRPC_Client;
using Google.Protobuf;

Console.WriteLine(">>> Starting up GRPC client..");
//getting team image
string url="https://external-content.duckduckgo.com/iu/?u=https://www.sipsa.net/wp-content/uploads/2020/04/bugbusters_ok-1024x576.png&f=1&nofb=1&ipt=ee84cf52e56821195c8aafd620f7e7a75e818ba7dae444da34f40b9962f7bcd9&ipo=images&fbclid=IwAR3_jN5NO5sGdkpj0MIbxcOQA6yxKu64Z5P__cEUpfQ341iB86xkyrvwsxI";
byte[] imageBytes;
ByteString imagebyteStr;

using (var http_client = new HttpClient()) {
    using (var response = await http_client.GetAsync(url))
    {
        imageBytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
    }
}

imagebyteStr = ByteString.CopyFrom(imageBytes);


// The port number must match the port of the gRPC server.
using var channel = GrpcChannel.ForAddress("http://10.8.9.121:9080");
var client = new PdService.PdServiceClient(channel);


//RegisterTeam (teamName, teamPassword, teamImagePng) 
var reply = await client.RegisterTeamAsync(
    new RegistrationRequestMessage { 
        TeamName = "BugBusters",
        TeamPassword = "password",
        TeamImagePng = imagebyteStr
    }
);


//Console.WriteLine("Greeting: " + reply.Message);
Console.WriteLine("Press any key to exit...");
Console.ReadKey();