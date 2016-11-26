// Structctor1.cs
using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;



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

    /* Modules */
    public Modules Modules;

    private List<UnspecificUserInfo> UsersInfo;

    // Classes constructor
    public BotUser(Discord.DiscordClient Bot)
    {
        // Create a SaveModule named BotSaveModule to handle the saving / loading of user info data.
        Modules = new Modules();

        UsersInfo = Modules.SaveModule.LoadUserData();
    }

    // TODO extract this back to the send event?
    public void MessageSent(ulong UserID)
    {
        UnspecificUserInfo InitialUserInfo = Modules.DataModule.GetUserInfo(UserID);
        int NewNumMessagesSent = InitialUserInfo.NumMessagesSent + 1;
        UnspecificUserInfo NewUserInfo = InitialUserInfo;
        NewUserInfo.NumMessagesSent = NewNumMessagesSent;

        Modules.DataModule.UpdateUserInfo(NewUserInfo);

        // Add XP (if last message sent was X ago, but that is set by params and in AddXP etc)
        Random RandomNumberGen = new Random();
        int XPToAdd = RandomNumberGen.Next(10, 25); // TODO make these params!
        Modules.XPModule.AddXP(XPToAdd, UserID);
    }

    

    
    
}

// TODO make 1 central update function that accept like the user and the new struct or only the new struct (for server specific user info?)
// TODO Add username/nick name to userinfo
// TODO log error messages instead of doing nothing for some commands.