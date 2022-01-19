var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("", (string comment, string passphrase) => {

    // Initialize KeyGenerator for ed25519 with comment specified
    KeyGenerator generator = new("ed25519", comment, passphrase);
    
    // Generate keys
    generator.GenerateKey();

    // Return keys
    return generator.GetKeys();
});

app.Run();
