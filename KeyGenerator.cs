using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Security.Cryptography;

public class KeyGenerator {

    const string regexString = "^[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$";
    private readonly static Regex regex = new Regex(regexString, RegexOptions.IgnoreCase);

    public bool mailAddressValid(String email)
    {
        return regex.IsMatch(email);
    }
    public string KeyType { get; set; }
    public string Comment { get; set; }
    private string TempDirectory { get; set; }
    private string PublicKey { get; set; }
    private string PrivateKey { get; set; }

    private string PassPhrase { get; set; }

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

    public KeyGenerator(string keyType, string comment, string passPhrase) {
        // Here we construct the KeyGenerator object with the passed arguments
       KeyType = keyType;
       Comment = comment;
       PassPhrase = passPhrase;
    }

    public void GenerateTempDirectory() {
        // Create a new UUID for this session
        Guid sessionUUID = Guid.NewGuid();

        TempDirectory = sessionUUID.ToString();

        // Create a temporary folder to generate an SSH key in
        System.IO.Directory.CreateDirectory(TempDirectory);
    }

    public void GenerateKey() {

        // Generate a temp directory to write the keys to
        GenerateTempDirectory();

        Process tclProcess = new();
        // Setup command and assign arguments
        tclProcess.StartInfo.FileName = "./use-keygen.tcl";

        string arguments = $"{KeyType} ${TempDirectory} ${Comment} ${PassPhrase}";
        tclProcess.StartInfo.Arguments = arguments;
        Console.WriteLine(arguments);

        // We don't have to execute this in shell
        tclProcess.StartInfo.UseShellExecute = true;
        tclProcess.StartInfo.CreateNoWindow = true;

        // Start process
        tclProcess.Start();

        Thread.Sleep(1000);
    
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
    }
}