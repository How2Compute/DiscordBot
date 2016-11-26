using Discord.Commands;

internal class Strike
{
    public Strike(Discord.DiscordClient Bot, Modules BotModules)
    {
        Bot.GetService<CommandService>().CreateCommand("Strike")
            .Alias(new string[] { "Warn" })
            .Description("Wans/Strikes a user (3 strikes = kick)")
            .Parameter("UserToStrike", ParameterType.Optional)
            .Parameter("ID", ParameterType.Optional)
            .Do(async e =>
            {
                if (e.GetArg("UserToStrike") == "")
                {
                    await e.Channel.SendMessage("Please use $strike <usertostrike> <id>");
                }
                else
                {
                    if (e.User.GetPermissions(e.Channel).ManageChannel)
                    {
                        if (BotModules.UserModule.GetUserByName(e.GetArg("UserToStrike"), e.Channel).WasFound)
                        {
                            await e.Channel.SendMessage($"You just got striked {BotModules.UserModule.GetUserByName(e.GetArg("UserToStrike"), e.Channel).User.Mention}!");
                            BotModules.PunishmentModule.ChangeStrikes(e.Server, BotModules.UserModule.GetUserByName(e.GetArg("UserToStrike"), e.Channel).User.Id, 1);
                        }
                    }
                    else
                    {
                        await e.Channel.SendMessage("You ain't an admin bro...");
                    }
                }
            });
    }
}