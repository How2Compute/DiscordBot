// Structctor1.cs
using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

// Struct for server specific info like strikes, if the user is being monitored etc.
[Serializable]
public struct ServerSpecificUserInfo
{
    public ulong ServerID;
    public int NumStrikes;
    public bool IsBeingMonitored;

    public ServerSpecificUserInfo(Discord.Server Server)
    {
        this.ServerID = Server.Id;
        this.NumStrikes = 0;
        this.IsBeingMonitored = false;
    }

    public ServerSpecificUserInfo(ulong ServerID)
    {
        this.ServerID = ServerID;
        this.NumStrikes = 0;
        this.IsBeingMonitored = false;
    }
}

// Struct for the general server info, so accross all servers with this bot.
[Serializable]
public struct UnspecificUserInfo
{
    public ulong UserID;
    public List<ServerSpecificUserInfo> ServerInfo;
    public int NumMessagesSent;
    public int XP;
    public DateTime LastXPGain;

    public UnspecificUserInfo(Discord.User User)
    {
        this.UserID = User.Id;
        this.NumMessagesSent = 0;
        this.XP = 0;
        this.LastXPGain = new DateTime();
        List<ServerSpecificUserInfo> NewServerInfo = new List<ServerSpecificUserInfo>();

        ServerInfo = NewServerInfo;
    }

    public UnspecificUserInfo(ulong UserID)
    {
        this.UserID = UserID;
        this.NumMessagesSent = 0;
        this.XP = 0;
        this.LastXPGain = new DateTime();
        List<ServerSpecificUserInfo> NewServerInfo = new List<ServerSpecificUserInfo>();

        ServerInfo = NewServerInfo;
    }

    public string GetStatsInText(ulong ServerID)
    {
        BotUser Bot = new BotUser();
        ServerSpecificUserInfo ServerInfo = Bot.GetUserServerInfo(UserID, ServerID);
        if (ServerInfo.IsBeingMonitored)
            return (String.Format("(User #{0} has {1} strike(s) and is being monitored). This user has sent {2} messages and recieved {3} XP!", UserID, ServerInfo.NumStrikes, NumMessagesSent, XP));
        else
            return (String.Format("(User #{0} has {1} strike(s) and is NOT being monitored). This user has sent {2} messages and recieved {3} XP!", UserID, ServerInfo.NumStrikes, NumMessagesSent, XP));
    }
}

// Struct for user search results
public struct UserSearchResult
{
    public Discord.User User;
    public UnspecificUserInfo Info;
    public bool WasFound;
}

internal class BotUser
{
    // TODO make this settable per server and through the actual bot so with a command like $NUMSTRIKESFORBAN 4 etc.

    /* Config */
    private const int NUMSTRIKESFORBAN = 3;
    private const int NUMSTRIKESFORKICK = 2;
    private TimeSpan TIMEBETWEENXPGAINS = new TimeSpan(0, 1, 0);

    private List<UnspecificUserInfo> UsersInfo;

    // Classes constructor
    public BotUser()
    {
        // When this class is constructed, attempt to load previous save data.
        UsersInfo = LoadUserData();
    }

    // Gets the info about specified user.
    public UnspecificUserInfo GetUserInfo(ulong UserID)
    {
        // Declare a default user info to fall back on.
        UnspecificUserInfo Info = new UnspecificUserInfo(UserID);
        bool WasFound = false;

        // Go through all the users.
        foreach (UnspecificUserInfo UsersInfo in UsersInfo)
        {
            // If it finds a matching user ID...
            if (UsersInfo.UserID == UserID)
            {
                // Set the info, be sure the system knows the users info was found and exit the loop.
                Info = UsersInfo;
                WasFound = true;
                break;
            }
        }

        // If there wasen't a valid user's info found, that means it needs to create a new user's info.
        if (!WasFound || Info.Equals(null))
        {
            UsersInfo.Add(new UnspecificUserInfo(UserID));
            Info = GetUserInfo(UserID);
        }

        return Info;
    }

    // NOT RECCOMENDED & DEPRICATING!!! Combines GetUserInfo and GetUserByName info 1 function.
    public UnspecificUserInfo GetUserInfo(string Name, Discord.Channel ChannelToSearch)
    {
        return GetUserInfo(GetUserByName(Name, ChannelToSearch).User.Id);
    }

    // Attempts to find a user by the inputted name, returns search result. TODO make this not user based but ID based?
    public UserSearchResult GetUserByName(string Name, Channel ChannelToSearch)
    {
        // Create default struct and set it to not found in case nothing was found later on.
        UserSearchResult SearchResult = new UserSearchResult();
        SearchResult.WasFound = false;

        // Go through all the users in a given channel and try to find a user by the specified name.
        foreach (Discord.User User in ChannelToSearch.Users)
        {
            // If the user with a matching name or nickname is found...
            if (User.Name == Name || User.Nickname == Name)
            {
                // Save the user into a variable, set it to found and exit the loop.
                SearchResult.User = User;
                SearchResult.WasFound = true;
                break;
            }
        }

        return SearchResult;
    }

    public UserSearchResult GetUserByID(ulong UserID, Discord.Server ServerToSearch)
    {
        // Create default struct and set it to not found in case nothing was found later on.
        UserSearchResult SearchResult = new UserSearchResult();
        SearchResult.WasFound = false;

        // Go through all the users in a given server.
        foreach (Discord.User User in ServerToSearch.Users)
        {
            // If a user with a matching ID was found...
            if (User.Id == UserID)
            {
                // Set the searchresult to this user, signal that it was found and exit the loop.
                SearchResult.User = User;
                SearchResult.WasFound = true;
                break;
            }
        }

        return SearchResult;
    }

    public ServerSpecificUserInfo GetUserServerInfo(ulong UserID, ulong ServerID)
    {
        UnspecificUserInfo UserInfo = GetUserInfo(UserID);
        ServerSpecificUserInfo SearchResult = new ServerSpecificUserInfo(ServerID);
        bool WasServerFound = false;

        foreach (ServerSpecificUserInfo ServerSpecifics in UserInfo.ServerInfo)
        {
            // TODO be sure that a server gets autoadded to a user on join (AND NOT ONLY THE FIRST SERVER THEY JOIN LIKE NOW!!!!!!)
            if (ServerSpecifics.ServerID == ServerID)
            {
                // The server with a matching ID was found!
                SearchResult = ServerSpecifics;
                WasServerFound = true;
                break;
            }
        }

        // Search was completed, now if there wheren't any servers found that means it needs to add the server to the users servers!
        if (!WasServerFound)
        {
            UserInfo.ServerInfo.Add(SearchResult);
        }

        return SearchResult;
    }

    public UnspecificUserInfo ChangeStrikes(Discord.Server Server, ulong UserID, int Change)
    {
        // Setup variables
        UnspecificUserInfo InitialUserInfo = GetUserInfo(UserID);
        ServerSpecificUserInfo InitialUserServerInfo = GetUserServerInfo(UserID, Server.Id);
        Discord.User User = GetUserByID(UserID, Server).User;

        // Check if the user was NOT found...
        if (!GetUserByID(UserID, Server).WasFound)
        {
            // and return an empty user info.
            return InitialUserInfo;
        }

        int NewNumberOfStrikes = InitialUserServerInfo.NumStrikes + Change; // will calculate the new number of strikes CUZ ( +- = -)

        if (Change == 0)
            goto end; // Nothing to change!
        else if (Change > 0)
            User.SendMessage($"Be carefull! You just recieved {Change} strikes (total number of strikes: {NewNumberOfStrikes}). {NUMSTRIKESFORKICK} strikes is a kick and {NUMSTRIKESFORBAN} is a ban!");
        else if (Change < 0)
        {
            if (GetUserServerInfo(UserID, Server.Id).NumStrikes == 0)
                goto end; // Do nothing as the user cannot have sub-zero strikes!
            else
                User.SendMessage($"{Change * -1} strikes just got revoked to give you a total of {NewNumberOfStrikes} strikes. {NUMSTRIKESFORKICK} strikes is a kick and {NUMSTRIKESFORBAN} is a ban!");
        }

        // Create a new ServerSpecificUserInfo struct with the update ammount of strikes.
        ServerSpecificUserInfo NewServerUserInfo = InitialUserServerInfo;
        NewServerUserInfo.NumStrikes = NewNumberOfStrikes;
        // Create a new UserInfo based on the origional user info to house the new ServerUserInfo
        UnspecificUserInfo NewUserInfo = InitialUserInfo;

        // Find the server's list index in the new userinfo.
        int index = NewUserInfo.ServerInfo.FindIndex(d => d.ServerID == NewServerUserInfo.ServerID);
        // And set the above index of the list to the new serveruserinfo.
        NewUserInfo.ServerInfo[index] = NewServerUserInfo;
        // TODO extract above (update UserServer info) into function or something.

        UpdateUserInfo(NewUserInfo);

        // Check if the user has enough strikes to get kicked
        if (GetUserServerInfo(UserID, Server.Id).NumStrikes == NUMSTRIKESFORKICK)
        {
            // TODO make a rejoinmsg.
            User.SendMessage($"You got a total of {NewNumberOfStrikes} strikes, this is more than {NUMSTRIKESFORKICK} so you got kicked!");
            // Now kick the user TODO: be sure that User.Kick(); is actually the right command :D
            User.Kick();
        }
        // Check if the user has enough strikes for a ban.
        else if (GetUserServerInfo(UserID, Server.Id).NumStrikes >= NUMSTRIKESFORBAN)
        {
            // Send the user a message telling them they got banned as they had more than the ban threshhold strikes.
            User.SendMessage($"You got a total of {NewNumberOfStrikes} strikes, this is more than {NUMSTRIKESFORBAN} so you got banned!");
            // And then actually ban them from the server
            Server.Ban(GetUserByID(UserID, Server).User);
        }
    // an end goto for stuff that doesnt need to process the ammount of strikes etc. When strikes are not processed it will automagically fall through to this.
    end:
        // Get and return the updated user info
        return GetUserInfo(UserID);
    }

    public void MessageSent(ulong UserID)
    {
        UnspecificUserInfo InitialUserInfo = GetUserInfo(UserID);
        int NewNumMessagesSent = InitialUserInfo.NumMessagesSent + 1;
        UnspecificUserInfo NewUserInfo = InitialUserInfo;
        NewUserInfo.NumMessagesSent = NewNumMessagesSent;

        UpdateUserInfo(NewUserInfo);

        // Add XP (if last message sent was X ago, but that is set by params and in AddXP etc)
        Random RandomNumberGen = new Random();
        int XPToAdd = RandomNumberGen.Next(10, 25); // TODO make these params!
        AddXP(XPToAdd, UserID);

        // TODO, remove/replace this debug line
        Console.WriteLine(GetUserInfo(UserID).ToString());
    }

    public void AddXP(int Ammount, ulong UserID)
    {
        if (DateTime.Now >= GetUserInfo(UserID).LastXPGain.Add(TIMEBETWEENXPGAINS))
        {
            UnspecificUserInfo InitialUserInfo = GetUserInfo(UserID);
            UnspecificUserInfo NewUserInfo = InitialUserInfo;
            NewUserInfo.XP = InitialUserInfo.XP + Ammount;
            NewUserInfo.LastXPGain = DateTime.Now;
            UpdateUserInfo(NewUserInfo);
        }
    }

    // Updates the inputter users info. Find the one to update by the user info's id.
    private void UpdateUserInfo(UnspecificUserInfo NewUserInfo)
    {
        // Get the index of the current user's id
        int index = UsersInfo.FindIndex(d => d.UserID == NewUserInfo.UserID);
        // Set's the list element at the above found index to the new (user) info.
        UsersInfo[index] = NewUserInfo;

        // As the data has been updated, save it to a file to be sure data has been saved in case of unexpected crashes. (and onexit plain not working :P)
        SaveUserData(UsersInfo);

        // TODO double check if this creates a new user if the user doesnt exist!!!
    }

    // Saving / Loading Section TODO Make this its own class?

    // Saves the inputted user's info to a (json) file for multiple program runs to use. TODO Make this use a database sothat there can be multiple hosts?
    public static void SaveUserData(List<UnspecificUserInfo> input)
    {
        // Serialize JSON to a string and then write string to a file
        File.WriteAllText(@"UserData.json", JsonConvert.SerializeObject(input));
    }

    // Returns the User's their info, read form a save file.
    public static List<UnspecificUserInfo> LoadUserData()
    {
        // Open and Close the file sothat if it does not exist it gets created.
        TextWriter EnsureFileExistThingy = new StreamWriter("UserData.json", true);
        EnsureFileExistThingy.Close();

        // Read file into a string and deserialize JSON to a user struct list.
        List<UnspecificUserInfo> UserInfoData = JsonConvert.DeserializeObject<List<UnspecificUserInfo>>(File.ReadAllText("UserData.json"));

        // If the read info is null (aka there probably wasent a recent save)...
        if (UserInfoData == null)
        {
            // Create a new (blank) list of user info.
            UserInfoData = new List<UnspecificUserInfo>();
        }

        // Return the (newly created?) user data
        return UserInfoData;
    }

    // Token Section TODO Make this its own class?

    // Base64 encodes the user's id sothat it looks a bit more like a token.
    public string GetUserTrackHash(ulong UserID)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(UserID.ToString());
        return System.Convert.ToBase64String(plainTextBytes);
    }

    // BEING DEPRICATED!!! Get's the inputted users tracking has (base64 encoded discord user ID)
    public string GetUserTrackHash(Discord.User User)
    {
        return GetUserTrackHash(User.Id);
    }

    // Decode the tracking hash (base64 encoded discord user ID).
    public ulong TrackHashToID(string base64EncodedData)
    {
        var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
        return (ulong)Convert.ToInt64(System.Text.Encoding.UTF8.GetString(base64EncodedBytes));
    }
}

// TODO make 1 central update function that accept like the user and the new struct or only the new struct (for server specific user info?)
// TODO Add username/nick name to userinfo
// TODO log error messages instead of doing nothing for some commands.