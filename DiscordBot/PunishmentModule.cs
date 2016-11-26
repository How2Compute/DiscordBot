internal class PunishmentModule
{
    private Modules Modules;

    public PunishmentModule(Modules BotModules)
    {
        Modules = BotModules;
    }

    public UnspecificUserInfo ChangeStrikes(Discord.Server Server, ulong UserID, int Change)
    {
        const int NUMSTRIKESFORKICK = 2;
        const int NUMSTRIKESFORBAN = 3;

        // Setup variables
        UnspecificUserInfo InitialUserInfo = Modules.DataModule.GetUserInfo(UserID);
        ServerSpecificUserInfo InitialUserServerInfo = Modules.DataModule.GetUserServerInfo(UserID, Server.Id);
        Discord.User User = Modules.UserModule.GetUserByID(UserID, Server).User;

        // Check if the user was NOT found...
        if (!Modules.UserModule.GetUserByID(UserID, Server).WasFound)
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
            if (Modules.DataModule.GetUserServerInfo(UserID, Server.Id).NumStrikes == 0)
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

        Modules.DataModule.UpdateUserInfo(NewUserInfo);

        // Check if the user has enough strikes to get kicked
        if (Modules.DataModule.GetUserServerInfo(UserID, Server.Id).NumStrikes == NUMSTRIKESFORKICK)
        {
            // TODO make a rejoinmsg.
            User.SendMessage($"You got a total of {NewNumberOfStrikes} strikes, this is more than {NUMSTRIKESFORKICK} so you got kicked!");
            // Now kick the user TODO: be sure that User.Kick(); is actually the right command :D
            User.Kick();
        }
        // Check if the user has enough strikes for a ban.
        else if (Modules.DataModule.GetUserServerInfo(UserID, Server.Id).NumStrikes >= NUMSTRIKESFORBAN)
        {
            // Send the user a message telling them they got banned as they had more than the ban threshhold strikes.
            User.SendMessage($"You got a total of {NewNumberOfStrikes} strikes, this is more than {NUMSTRIKESFORBAN} so you got banned!");
            // And then actually ban them from the server
            Server.Ban(Modules.UserModule.GetUserByID(UserID, Server).User);
        }
    // an end goto for stuff that doesnt need to process the ammount of strikes etc. When strikes are not processed it will automagically fall through to this.
    end:
        // Get and return the updated user info
        return Modules.DataModule.GetUserInfo(UserID);
    }
}
// TODO rename this module?