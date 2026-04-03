using System.Text;
using System.Text.Json;

var stdin = Console.OpenStandardInput();
var stdout = Console.OpenStandardOutput();
var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = false
};

while (true)
{
    var request = await ReadMessageAsync(stdin);
    if (request is null)
    {
        break;
    }

    object response = request.Command?.ToLowerInvariant() switch
    {
        "ping" => new NativeHostResponse("pong", true, null, new { name = "DocuDeskHost", version = "0.1.0" }),
        "create-draft" => new NativeHostResponse("create-draft", false, "Noch nicht implementiert", null),
        _ => new NativeHostResponse(request.Command ?? "unknown", false, "Unbekannter Befehl", null)
    };

    await WriteMessageAsync(stdout, response);
}

async Task<NativeHostRequest?> ReadMessageAsync(Stream input)
{
    var lengthBuffer = new byte[4];
    var read = await input.ReadAsync(lengthBuffer, 0, 4);
    if (read == 0)
    {
        return null;
    }
    if (read < 4)
    {
        throw new InvalidOperationException("Incomplete native messaging header.");
    }

    var length = BitConverter.ToInt32(lengthBuffer, 0);
    var payloadBuffer = new byte[length];
    var offset = 0;
    while (offset < length)
    {
        var chunk = await input.ReadAsync(payloadBuffer, offset, length - offset);
        if (chunk == 0)
        {
            throw new InvalidOperationException("Unexpected end of native message payload.");
        }
        offset += chunk;
    }

    var json = Encoding.UTF8.GetString(payloadBuffer);
    return JsonSerializer.Deserialize<NativeHostRequest>(json, jsonOptions);
}

async Task WriteMessageAsync(Stream output, object payload)
{
    var json = JsonSerializer.Serialize(payload, jsonOptions);
    var bytes = Encoding.UTF8.GetBytes(json);
    var header = BitConverter.GetBytes(bytes.Length);
    await output.WriteAsync(header, 0, header.Length);
    await output.WriteAsync(bytes, 0, bytes.Length);
    await output.FlushAsync();
}


internal sealed record NativeHostRequest(string? Command, JsonElement? Payload);
internal sealed record NativeHostResponse(string Command, bool Success, string? Error, object? Payload);
