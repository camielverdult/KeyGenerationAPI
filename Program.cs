using System.Text.RegularExpressions;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("", (string comment) => {

    // Generate SSH key
    // public KeyGenerator(string keyType, string comment, string tempDirectory)
    KeyGenerator generator = new("ed25519", comment);

    return generator.GenerateKey();
});

app.Run();
