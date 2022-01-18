using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

public class KeyGenerator {

    const string regexString = "^[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$";
    private readonly static Regex regex = new Regex(regexString, RegexOptions.IgnoreCase);

    public bool mailAddressValid(String email)
    {
        return regex.IsMatch(email);
    }

    [DllImport("ssh-keygen.o", EntryPoint="main")]

    unsafe static extern int main(int argc, char **argv);

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

    public string GenerateKey() {

        string tempDirectory = GenerateTempDirectory();

        string argv = $"-o -a 100 -t {KeyType} -f ${tempDirectory} -C {Comment}";
        int argc = argv.Split(" ")
                        .Count();

        unsafe {
            fixed (char* args = argv.ToCharArray())

            main(argc, &args);
        }

        return "yes";
    }

}