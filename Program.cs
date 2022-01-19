var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("", (string comment, string passphrase) => {

    // Generate SSH key
    // public KeyGenerator(string keyType, string comment, string tempDirectory)
    KeyGenerator generator = new("ed25519", comment);

    return generator.GenerateKey(passphrase);
});

app.Run();
