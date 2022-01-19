using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;
using System.Text;

public class KeyGenerator {

    const string regexString = "^[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$";
    private readonly static Regex regex = new Regex(regexString, RegexOptions.IgnoreCase);

    public bool mailAddressValid(String email)
    {
        return regex.IsMatch(email);
    }
    public string KeyType {get; set;}
    public string Comment {get; set;}

    public KeyGenerator(string keyType, string comment) {
       KeyType = keyType;
       Comment = comment;
    }

    public string GenerateTempDirectory() {
        // Create a new UUID for this session
        Guid sessionUUID = Guid.NewGuid();

        string tempDirectory = sessionUUID.ToString();

        // Create a temporary folder to generate an SSH key in
        System.IO.Directory.CreateDirectory(tempDirectory);

        return tempDirectory;
    }

    public string GenerateArguments() {
        string tempDirectory = GenerateTempDirectory();

        // Here is an example of what this function will generate:
        // -o -a 100 -t ed25519 -f 862603c0-2e86-47c4-98cc-fd595184fd6b -C "test"
        return $"-o -a 100 -t {KeyType} -f {tempDirectory}/id_{KeyType} -C {Comment}";
    }
    public string GenerateKey(string? passphrase = null) {

        string arguments = GenerateArguments();
        Process greeterProcess = new Process();

        // Setup command and assign arguments
        greeterProcess.StartInfo.FileName = "ssh-keygen";
        greeterProcess.StartInfo.Arguments = arguments;

        // We want to interact with this ssh-keygen, so we redirect standard I/O
        greeterProcess.StartInfo.RedirectStandardInput = true;
        greeterProcess.StartInfo.RedirectStandardOutput = true;

        // We don't have to execute this in shell
        greeterProcess.StartInfo.UseShellExecute = false;

        // Start process
        Console.WriteLine($"Running ssh-keygen {arguments}");
        greeterProcess.Start();

        /*
            Example / Intermezzo:
            ./ssh-keygen -o -a 100 -t ed25519 -f 862603c0-2e86-47c4-98cc-fd595184fd6b -C "test"

            Will give the following lines of output:

            Generating public/private ed25519 key pair.
            Enter passphrase (empty for no passphrase): 

            After this line, we want to enter the passphrase.
        */

        // Get standard input and output;
        StreamWriter writer = greeterProcess.StandardInput;
        StreamReader reader = greeterProcess.StandardOutput;

        while (!reader.ReadLine().Contains("The key's randomart image is:")) {
            string line = reader.ReadLine();
            Console.WriteLine(line);
            if (line.StartsWith("Enter") && line.Contains("passphrase")) {
                // We want this to run for these two lines:
                // Enter passphrase (empty for no passphrase): 
                // Enter same passphrase again: 

                if (passphrase != null) {
                    writer.Write(passphrase);
                }

                // "Press" enter for new line
                writer.WriteLine();
            }
        }

        return reader.ReadToEnd();
    }

}