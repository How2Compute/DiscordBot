using Discord;
using Discord.Commands;

internal class XP
{
    public XP(ref Discord.DiscordClient Bot, Modules Modules)
    {
        Bot.GetService<CommandService>().CreateCommand("XP")
            .Alias(new string[] { "Rank" })
            .Description("Gives you your current ammount of xp!")
            .Do(async e =>
            {
                await e.Channel.SendMessage($"{e.User.NicknameMention} has {Modules.DataModule.GetUserInfo(e.User.Id).XP} XP!!!"); // TODO make levels and ranking and shit
            });
    }
}