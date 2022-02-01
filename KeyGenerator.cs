using System.Text.RegularExpressions;
using System.Diagnostics;

public class KeyGenerator {

    // We use this regex to only allow the alfabet
    const string InputRegexString = "^[A-Za-z0-9]+$";
    private static readonly Regex InputRegex = new(InputRegexString, RegexOptions.IgnoreCase);

    const string EmailRegexString = "[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";
    private static readonly Regex EmailRegex = new(EmailRegexString, RegexOptions.IgnoreCase);
    private bool InputValid(string input)
    {
        // This checks if the email is valid by matching it to the e-mail regex
        return InputRegex.IsMatch(input);
    }

    private bool EmailValid(string email)
    {
        // This checks if the email is valid by matching it to the e-mail regex
        return EmailRegex.IsMatch(email);
    }


    // These will be used to generate the arguments for ssh-keygen
    public string KeyType { get; }
    public string Comment { get; }
    private string FileName { get; }
    private string PassPhrase { get; }

    // These will be used for returning public and private key
    private string PublicKey { get; set; }
    private string PrivateKey { get; set; }
    private List<string> Logs {get;}

    public string GetPublicKey() {
        return PublicKey;
    }

    public string GetPrivateKey() {
        return PrivateKey;
    }

    public Dictionary<string, string> GetKeys() {
        // Here we generate our response for the user
        Dictionary<string, string> keys = new();

        // This will be our public key
        keys.Add("Public", GetPublicKey());

        // This will be our private key
        keys.Add("Private", GetPrivateKey());

        // This will be our ssh-keygen output and ascii art
        keys.Add("Log", string.Join("\n", Logs.ToArray()));
        
        return keys;
    }

    public KeyGenerator(string keyType, string comment, string passPhrase) {
        // Here we construct the KeyGenerator object with the passed arguments
        KeyType = keyType;
        Comment = comment;
        PassPhrase = passPhrase;

        // We use this later, but initialize them in the constructor
        PublicKey = "";
        PrivateKey = "";
        Logs = new List<string>();

        // We want to have a name that is (pretty) unique for each session
        // So we use Guid as a file name, we delete the file afterwards
        Guid sessionUuid = Guid.NewGuid();
        string tempFile = sessionUuid.ToString();
        FileName = Path.Combine(Path.GetTempPath(), tempFile);
    }

    public void GenerateKey() {

        // We sanitize input first, we don't want anyone to mess with our ssh-keygen command

        // bool fail = false;
        //
        // // Check for valid comment or valid e-mail
        // if (!InputValid(Comment) || !EmailValid(Comment)) {
        //     Logs.Add("Invalid comment/e-mail for ssh-key!\n");
        //     fail = true;
        // }
        //
        // if (!InputValid(PassPhrase)) {
        //     Logs.Add("Invalid passphrase for ssh-key!\n");
        //     fail = true;
        // }
        //
        // if (fail) 
        //     return;

        // We call the ssh-keygen command with the following argument format:
        // ssh-keygen -t type [-N new_passphrase] [-C comment] [-f output_keyfile] 

        // We use this format so that ssh-keygen does not ask us for a passphrase

        // When a different format was used, the process would wait forever waiting
        // for input, using TCL was tried (you can still see and use the file) but
        // was not used anymore after figuring out a format without the need to 
        // interact with the ssh-keygen command

        // see https://linux.die.net/man/1/ssh-keygen for more formats and usage

        Process keygenProcess = new Process{
            // In here we define the behaviour of our process
            StartInfo = new ProcessStartInfo{
                // We run ssh-keygen directly
                FileName = "./ssh-keygen", 

                // We use the following commands, see comment block above why
                Arguments = $"-t {KeyType} -N \"{PassPhrase}\" -C \"{Comment}\" -f \"{FileName}\"",

                // We redirect the standard output so that we can return the output
                // of the command to the user
                RedirectStandardOutput = true
            }
        };

        // Start our process
        keygenProcess.Start();

        // We want to log the output of the ssh-keygen command for the user to see, so
        // we use the following while-loop to add the output lines to return them later
        while (!keygenProcess.StandardOutput.EndOfStream)
        {
            // We use the ! because we know that we will not read after
            // the stream has ended due to our while conditions
            string line = keygenProcess.StandardOutput.ReadLine()!;

            // Write line to CreationLog
            Logs.Add(line);
        }

        // We don't want to do the following steps if the process might still be
        // interacting with the files, so we wait a little bit
        keygenProcess.WaitForExit();
    
        // The public key has .pub appended to the end of the file name
        string publicKeyName = $"{FileName}.pub";

        // Read contents of public key
        PublicKey = File.ReadAllText(publicKeyName);

        // Delete public key file
        File.Delete(publicKeyName);

        // The private key has the same file name as passed to ssh-keygen
        string privateKeyName = FileName;
    
        // Read contents of private key
        PrivateKey = File.ReadAllText(privateKeyName);

        // Delete private key
        File.Delete(privateKeyName);
    }
}