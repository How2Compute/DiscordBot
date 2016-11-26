using System;

internal class XPModule
{
    private static TimeSpan TIMEBETWEENXPGAINS = new TimeSpan(0, 1, 0);

    private Modules Modules;

    public XPModule(Modules BotModules)
    {
        Modules = BotModules;
    }

    public void AddXP(int Ammount, ulong UserID)
    {
        if (DateTime.Now >= Modules.DataModule.GetUserInfo(UserID).LastXPGain.Add(TIMEBETWEENXPGAINS))
        {
            UnspecificUserInfo InitialUserInfo = Modules.DataModule.GetUserInfo(UserID);
            UnspecificUserInfo NewUserInfo = InitialUserInfo;
            NewUserInfo.XP = InitialUserInfo.XP + Ammount;
            NewUserInfo.LastXPGain = DateTime.Now;
            Modules.DataModule.UpdateUserInfo(NewUserInfo);
        }
    }
}