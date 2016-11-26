using System;
using System.Collections.Generic;

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

    /*
    public string GetStatsInText(ulong ServerID)
    {
        BotUser Bot = new BotUser();
        ServerSpecificUserInfo ServerInfo = Bot.GetUserServerInfo(UserID, ServerID);
        if (ServerInfo.IsBeingMonitored)
            return (String.Format("(User #{0} has {1} strike(s) and is being monitored). This user has sent {2} messages and recieved {3} XP!", UserID, ServerInfo.NumStrikes, NumMessagesSent, XP));
        else
            return (String.Format("(User #{0} has {1} strike(s) and is NOT being monitored). This user has sent {2} messages and recieved {3} XP!", UserID, ServerInfo.NumStrikes, NumMessagesSent, XP));
    }
    */
}

internal class DataModule
{
    private List<UnspecificUserInfo> UsersInfos;

    private Modules Modules;

    public DataModule(Modules BotModules)
    {
        Modules = BotModules;

        UsersInfos = Modules.SaveModule.LoadUserData();
    }

    // Gets the info about specified user.
    public UnspecificUserInfo GetUserInfo(ulong UserID)
    {
        // Declare a default user info to fall back on.
        UnspecificUserInfo Info = new UnspecificUserInfo(UserID);
        bool WasFound = false;

        // Go through all the users.
        foreach (UnspecificUserInfo UserInfo in UsersInfos)
        {
            // If it finds a matching user ID...
            if (UserInfo.UserID == UserID)
            {
                // Set the info, be sure the system knows the users info was found and exit the loop.
                Info = UserInfo;
                WasFound = true;
                break;
            }
        }

        // If there wasen't a valid user's info found, that means it needs to create a new user's info.
        if (!WasFound || Info.Equals(null))
        {
            UsersInfos.Add(new UnspecificUserInfo(UserID));
            Info = GetUserInfo(UserID);
        }

        return Info;
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

    // Updates the inputter users info. Find the one to update by the user info's id.
    public void UpdateUserInfo(UnspecificUserInfo NewUserInfo)
    {
        // Get the index of the current user's id
        int index = UsersInfos.FindIndex(d => d.UserID == NewUserInfo.UserID);
        // Set's the list element at the above found index to the new (user) info.
        UsersInfos[index] = NewUserInfo;

        // As the data has been updated, save it to a file to be sure data has been saved in case of unexpected crashes. (and onexit plain not working :P)
        Modules.SaveModule.SaveUserData(UsersInfos);

        // TODO double check if this creates a new user if the user doesnt exist!!!
    }
}