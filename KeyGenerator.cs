using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;

public class KeyGenerator {

    const string regexString = "^[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$";
    private readonly static Regex regex = new Regex(regexString, RegexOptions.IgnoreCase);

    public bool mailAddressValid(String email)
    {
        return regex.IsMatch(email);
    }
    public string KeyType {get; set;}
    public string Comment {get; set;}
    private string TempDirectory {get; set;}
    private string PublicKey {get; set;}
    private string PrivateKey {get; set;}

    public string GetPublicKey() {
        return PublicKey;
    }

    public string GetPrivateKey() {
        return PrivateKey;
    }

    public Dictionary<string, string> GetKeys() {
        Dictionary<string, string> keys = new();

        keys.Add("Public", GetPublicKey());
        keys.Add("Private", GetPrivateKey());
        
        return keys;
    }

    public KeyGenerator(string keyType, string comment) {
        // Here we construct the KeyGenerator object with the passed arguments
       KeyType = keyType;
       Comment = comment;
    }

    public void GenerateTempDirectory() {
        // Create a new UUID for this session
        Guid sessionUUID = Guid.NewGuid();

        TempDirectory = sessionUUID.ToString();

        // Create a temporary folder to generate an SSH key in
        System.IO.Directory.CreateDirectory(TempDirectory);
    }

    public string GenerateArguments() {
        // Here is an example of what this function will generate:
        // -o -a 100 -t ed25519 -f 862603c0-2e86-47c4-98cc-fd595184fd6b -C "test"
        return $"-o -a 100 -t {KeyType} -f {TempDirectory}/id_{KeyType} -C {Comment}";
    }
    public string GenerateKey(string? passphrase = null) {

        // Generate a temp directory to write the keys to
        GenerateTempDirectory();

        // Generate arguments based on parameters passed to constructor
        string arguments = GenerateArguments();

        // This is where we store our output
        string lines = "";

        using (Process keygenProcess = new()) {
            // Setup command and assign arguments
            keygenProcess.StartInfo.FileName = "ssh-keygen";
            keygenProcess.StartInfo.Arguments = arguments;

            // We want to interact with this ssh-keygen, so we redirect standard I/O
            keygenProcess.StartInfo.RedirectStandardInput = true;
            keygenProcess.StartInfo.RedirectStandardOutput = true;

            // We don't have to execute this in shell
            keygenProcess.StartInfo.UseShellExecute = false;

            // Start process
            // Console.WriteLine($"Running ssh-keygen {arguments}");
            keygenProcess.Start();
            // Console.WriteLine("Running!");

            /*
                Example / Intermezzo:
                ./ssh-keygen -o -a 100 -t ed25519 -f 862603c0-2e86-47c4-98cc-fd595184fd6b -C "test"

                Will give the following lines of output:

                Generating public/private ed25519 key pair.
                Enter passphrase (empty for no passphrase): 

                After this line, we want to enter the passphrase.
            */

            // Get strams to standard input and output of process
            StreamWriter writer = keygenProcess.StandardInput;
            StreamReader reader = keygenProcess.StandardOutput;

            while (!lines.Contains("SHA256")) {
                string line = new(reader.ReadLine());
                lines += $"{line}\n";

                if (line.StartsWith("Enter passphrase")) {
                    // We want this to run for these two lines:
                    // Enter passphrase (empty for no passphrase): 
                    // Enter same passphrase again: 

                    if (passphrase != null) {
                        writer.Write(passphrase);
                    }

                    // "Press" enter for new line
                    writer.Write("\n");
                }

                keygenProcess.WaitForExit();
            }
        }

        // Read public key
        string pathToPublicKey = $"{TempDirectory}/id_{KeyType}.pub";
        PublicKey = File.ReadAllText(pathToPublicKey);

        // Read private key
        string pathToPrivateKey = $"{TempDirectory}/id_{KeyType}";
        PrivateKey = File.ReadAllText(pathToPrivateKey);

        // Delete key files
        DirectoryInfo TempDirInfo = new DirectoryInfo(TempDirectory);

        foreach (FileInfo file in TempDirInfo.GetFiles())
        {
            file.Delete(); 
        }

        // Delete temp directory
        Directory.Delete(TempDirectory);

        return lines;
    }
}