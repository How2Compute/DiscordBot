using Discord.Commands;

internal class DeToken
{
    public DeToken(Discord.DiscordClient Bot, Modules BotModules)
    {
        // TODO remove this as it is only a test
        Bot.GetService<CommandService>().CreateCommand("DeToken")
            .Alias(new string[] { "ResolveTrackHash" })
            .Description("Test Track Hash Resolver")
            .Parameter("Hash", ParameterType.Optional)
            .Do(async e =>
            {
                await e.Channel.SendMessage("Atempting to resolve hash..."); // TODO remove cuz this is only for debugging
                await e.Channel.SendMessage($"{e.GetArg("Hash")} is actually #{BotModules.TokenModule.TrackHashToID(e.GetArg("Hash"))} which in its turn is {BotModules.UserModule.GetUserByID(BotModules.TokenModule.TrackHashToID(e.GetArg("Hash")), e.Server).User.NicknameMention}!"); // TODO make this be more checking as it may get the wrong user or something
            });
    }
}