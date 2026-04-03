using System.Text.Json;

var handshake = new
{
    status = "placeholder",
    message = "Native-Messaging-Host ist noch nicht implementiert. Dies ist nur das Startprojekt."
};
Console.WriteLine(JsonSerializer.Serialize(handshake));
