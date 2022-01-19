var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("", () => {

    string comment = "test";
    string passphrase = "test";

    // Generate SSH key
    // public KeyGenerator(string keyType, string comment, string tempDirectory)
    KeyGenerator generator = new("ed25519", comment);

    generator.GenerateKey(passphrase);

    return generator.GetKeys();
});

app.Run();
