using Discord;
using Discord.Commands;

internal class XPCommands
{
    private XP XPCommand;

    public XPCommands(ref Discord.DiscordClient Bot, ref Modules BotModules)
    {
        XPCommand = new XP(ref Bot, BotModules);
    }
}