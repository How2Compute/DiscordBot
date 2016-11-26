using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

internal class SaveModule
{
    string SavePath;

    // Construct the SaveModule with a given path
    public SaveModule(string Path)
    {
        SavePath = Path;
    }

    // Saves the inputted user's info to a (json) file for multiple program runs to use. TODO Make this use a database sothat there can be multiple hosts?
    public void SaveUserData(List<UnspecificUserInfo> input)
    {
        // Serialize JSON to a string and then write string to a file
        File.WriteAllText(SavePath, JsonConvert.SerializeObject(input));
    }

    // Returns the User's their info, read form a save file.
    public List<UnspecificUserInfo> LoadUserData()
    {
        // Open and Close the file sothat if it does not exist it gets created.
        TextWriter EnsureFileExistThingy = new StreamWriter(SavePath, true);
        EnsureFileExistThingy.Close();

        // Read file into a string and deserialize JSON to a user struct list.
        List<UnspecificUserInfo> UserInfoData = JsonConvert.DeserializeObject<List<UnspecificUserInfo>>(File.ReadAllText(SavePath));

        // If the read info is null (aka there probably wasent a recent save)...
        if (UserInfoData == null)
        {
            // Create a new (blank) list of user info.
            UserInfoData = new List<UnspecificUserInfo>();
        }

        // Return the (newly created?) user data
        return UserInfoData;
    }
}