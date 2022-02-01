using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("", (string comment, string passphrase) => {

    // Start stopwatch
    var stopWatch = Stopwatch.StartNew();
    
    // Initialize KeyGenerator for ed25519 with comment specified
    KeyGenerator generator = new("ed25519", comment, passphrase);
    
    // Generate keys
    generator.GenerateKey();

    var keys = generator.GetKeys();
    stopWatch.Stop();
    
    Console.WriteLine($"Key generation took {stopWatch.ElapsedMilliseconds}ms!");
    
    // Return keys
    return keys;
});

app.Run();
