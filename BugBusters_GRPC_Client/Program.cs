using System.Threading.Tasks;
using Grpc.Net.Client;
using BugBusters_GRPC_Client;
using Google.Protobuf;
using Grpc.Core;
using System;
using BugBusters_GRPC_Client.Models;
using System.Text.Json;
using Newtonsoft.Json;
using ImageMagick;
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

Console.WriteLine(">>> Connected to server.");
string name = "BugBusters";
string password = "password";

//RegisterTeam (teamName, teamPassword, teamImagePng) 
var registerReply = await client.RegisterTeamAsync(
    new RegistrationRequestMessage { 
        TeamName = name,
        TeamPassword = password,
        TeamImagePng = imagebyteStr
    }
);

#region map conversion
byte[] map=registerReply.MapImagePng.ToByteArray();
MagickImage image = new MagickImage(map);
var pixels = image.GetPixels();
List<PixelModel> Paths = new List<PixelModel>();
Console.WriteLine($"height: {image.Height}, width: ${image.Width}");
for (int x = 0; x < image.Width; x++) {
    for (int y = 0; y < image.Height; y++) {
        var color = pixels[x, y].ToColor();
        if (color.R == 65535 && color.G == 65535 && color.B == 65535) { 
            Paths.Add(new PixelModel { X = x, Y = y, walkable = true });
        }
        else {
            Paths.Add(new PixelModel { X = x, Y = y, walkable = false });
        }
    }
}

//int leptek = 10;
//int counter = 0;
//for (int i = 0; i < image.Height; i+=leptek) {
//    for (int j = 0; j < image.Width; j+=leptek) {
//        if (Paths[counter].walkable) {
//            Console.Write("#");
//        }
//        else {
//            Console.Write(" ");
//        }
//        counter += leptek;
//    }
//    Console.WriteLine();
//}
#endregion

Console.WriteLine("Registration complete with the following id : " + registerReply.TeamId);

Clientlibrary cli = new Clientlibrary(
    client = client,
    call = call,
    cmdCounter = 0,
    mapImagePNG = registerReply.MapImagePng,
    Paths = Paths
);


//setup
await cli.Login(name, password, async() => { });
cli.ReadTask();


//other call tests
await cli.Cheat(async() => { 
    //await cli.BuyBike(async () => { }); 
});


//await cli.DESTROY_THIS_SHIT();

Console.WriteLine("Press any key to exit...");
Console.ReadKey();