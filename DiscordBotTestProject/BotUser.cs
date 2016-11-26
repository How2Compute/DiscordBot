// Structctor1.cs
using System;
using Discord;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

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

public struct UserSearchResult
{
    public Discord.User User;
    public UnspecificUserInfo Info;
    public bool WasFound;
}

enum AddWhat
{
    Strike,
    MessageCount,
    Bans
};

class BotUser
{
    /* Config */
    const int NUMSTRIKESFORBAN = 3;
    const int NUMSTRIKESFORKICK = 2;
    TimeSpan TIMEBETWEENXPGAINS = new TimeSpan(0, 1, 0);

    List<UnspecificUserInfo> UserInfos;

    public BotUser()
    {
        //DoSaveThing();
        UserInfos = LoadUserData();
        // Do something here
    }

    void DoSaveThing()
    {
        Console.WriteLine("I'm out of here");
        SaveUserData(UserInfos);
    }

    // Gets the info about specified user.
    public UnspecificUserInfo GetUserInfo(ulong UserID)
    {
        UnspecificUserInfo Info = new UnspecificUserInfo();

        bool WasFound = false;

        foreach(UnspecificUserInfo UsersInfo in UserInfos)
        {
            if(UsersInfo.UserID == UserID)
            {
                Info = UsersInfo;
                WasFound = true;
                break;
            }
        }

        // If there wasen't a valid user's info found, that means it needs to create a new user's info.
        if(!WasFound || Info.Equals(null))
        {
            UserInfos.Add(new UnspecificUserInfo(UserID));
            Info = GetUserInfo(UserID);
        }

        return Info;
    }

    // NOT RECCOMENDED!!! Combines GetUserInfo and GetUserByName info 1 function.
    public UnspecificUserInfo GetUserInfo(string Name, Discord.Channel ChannelToSearch)
    {
        return GetUserInfo(GetUserByName(Name, ChannelToSearch).User.Id);
    }

    // Attempts to find a user by the inputted name, returns search result.
    public UserSearchResult GetUserByName(string Name, Channel ChannelToSearch)
    {
        // Create default struct and set it to not found in case nothing was found later on.
        UserSearchResult SearchResult = new UserSearchResult();
        SearchResult.WasFound = false;

        foreach (Discord.User User in ChannelToSearch.Users)
        {
            if (User.Name == Name || User.Nickname == Name)
            {
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
        
        foreach (Discord.User User in ServerToSearch.Users)
        {
            if (User.Id == UserID)
            {
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
            if(ServerSpecifics.ServerID == ServerID)
            {
                // The server with a matching ID was found!
                SearchResult = ServerSpecifics;
                WasServerFound = true;
                break;
            }
        }

        // Search was completed, now if there wasent any servers found that means it needs to add the searched server to the users servers!
        if(!WasServerFound)
        {
            UserInfo.ServerInfo.Add(SearchResult);
        }

        return SearchResult;
    }

    public UnspecificUserInfo ChangeStrikes(Discord.Server Server, ulong UserID, int Change)
    {
        // TODO make a user ID -> user lookup! (done)
        UnspecificUserInfo InitialUserInfo = GetUserInfo(UserID);
        ServerSpecificUserInfo InitialUserServerInfo = GetUserServerInfo(UserID, Server.Id);
        Discord.User User = GetUserByID(UserID, Server).User;

        if(!GetUserByID(UserID, Server).WasFound)
        {
            // The user was not found in the given server so return an empty userinfo and do nothing
            return InitialUserInfo;
        }

        // TODO double check if user is indeed in given server

        int NewNumberOfStrikes = InitialUserServerInfo.NumStrikes + Change; // will calculate the new num of strikes CUZ ( +- = -) 

        if (Change == 0)
            goto end; // Nothing to change!

        else if (Change > 0)
            User.SendMessage($"Be carefull! You just recieved {Change} strikes (total number of strikes: {NewNumberOfStrikes}). {NUMSTRIKESFORKICK} strikes is a kick and {NUMSTRIKESFORBAN} is a ban!");

        else if (Change < 0)
        {
            if (GetUserServerInfo(UserID, Server.Id).NumStrikes == 0)
            {
                goto end; // Do nothing as the user cannot have sub-zero strikes!
            }
            else
            {
                User.SendMessage($"{Change * -1} strikes just got revoked to give you a total of {NewNumberOfStrikes} strikes. {NUMSTRIKESFORKICK} strikes is a kick and {NUMSTRIKESFORBAN} is a ban!");
            }
        }

        // Remove the old user data and add the new data.
        //UserInfos.Remove(GetUserInfo(UserID));
        //UnspecificUserInfo NewUserInfo = InitialUserInfo;
        //UpdateUserInfo(NewUserInfo);
        ServerSpecificUserInfo NewServerUserInfo = InitialUserServerInfo;
        NewServerUserInfo.NumStrikes = NewNumberOfStrikes;
        UnspecificUserInfo NewUserInfo = InitialUserInfo;

        int index = NewUserInfo.ServerInfo.FindIndex(d => d.ServerID == NewServerUserInfo.ServerID);
        NewUserInfo.ServerInfo[index] = NewServerUserInfo;
        // TODO extract above into function or something.

        UpdateUserInfo(NewUserInfo);

        if (GetUserServerInfo(UserID, Server.Id).NumStrikes == NUMSTRIKESFORKICK)
        {
            // TODO make a rejoinmsg.
            User.SendMessage($"You got a total of {NewNumberOfStrikes} strikes, this is more than {NUMSTRIKESFORKICK} so you got kicked!");
            //System.Threading.Thread.Sleep(2000);
            User.Kick();
        }
        else if(GetUserServerInfo(UserID, Server.Id).NumStrikes >= NUMSTRIKESFORBAN)
        {
            User.SendMessage($"You got a total of {NewNumberOfStrikes} strikes, this is more than {NUMSTRIKESFORBAN} so you got banned!");
            //System.Threading.Thread.Sleep(2000);
            Server.Ban(GetUserByID(UserID, Server).User);
        }


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
            // TODO reenable the following line and add username/nick name to userinfo
            //Console.WriteLine($"{User.Nickname} just got {Ammount} XP!");
            UnspecificUserInfo NewUserInfo = InitialUserInfo;
            NewUserInfo.XP = InitialUserInfo.XP + Ammount;
            NewUserInfo.LastXPGain = DateTime.Now;
            UpdateUserInfo(NewUserInfo);

        }
        else
        {
            //Console.WriteLine($"{User.Nickname} isn't getting xp as he already got some in the last minute!!!");
        }
    }

    private void UpdateUserInfo(UnspecificUserInfo NewUserInfo)
    {
        int index = UserInfos.FindIndex(d => d.UserID == NewUserInfo.UserID);
        UserInfos[index] = NewUserInfo;

        // TODO: Find a better way to do this only on app exit or something
        // Save the data sothat we dont have any onappexist problems, but this is TEMP
        DoSaveThing();
    }

    public static void SaveUserData(List<UnspecificUserInfo> input)
    {
        // TODO update the file paths
        // serialize JSON to a string and then write string to a file
        File.WriteAllText(@"movie.json", JsonConvert.SerializeObject(input));
    }

    public static List<UnspecificUserInfo> LoadUserData()
    {
        TextWriter EnsureFileExistThingy = new StreamWriter("movie.json", true);
        EnsureFileExistThingy.Close();

        // read file into a string and deserialize JSON to a type
        List<UnspecificUserInfo> Infoz = JsonConvert.DeserializeObject<List<UnspecificUserInfo>>(File.ReadAllText("movie.json"));
        if(Infoz == null)
        {
            Console.WriteLine("Infoz = null!!!!!!");
            Infoz = new List<UnspecificUserInfo>();
        }
        return Infoz;
    }

    public string GetUserTrackHash(ulong UserID)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(UserID.ToString());
        return System.Convert.ToBase64String(plainTextBytes);
    }

    public string GetUserTrackHash(Discord.User User)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(User.Id.ToString());
        return System.Convert.ToBase64String(plainTextBytes);
    }

    public ulong TrackHashToID(string base64EncodedData)
    {
        var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
        return (ulong) Convert.ToInt64(System.Text.Encoding.UTF8.GetString(base64EncodedBytes));
    }
}


// TODO make strikes bans etc server based
// TODO make 1 central update function that accept like the user and the new struct or only the new struct