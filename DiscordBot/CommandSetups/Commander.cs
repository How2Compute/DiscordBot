using Discord;
using Discord.Commands;

internal class Commander
{
    private XPCommands XPCommands;
    private TokenCommands TokenCommands;
    private PunishmentCommands PunishmentCommands;

    public Commander(ref Discord.DiscordClient Bot, ref Modules BotModules)
    {
        XPCommands = new XPCommands(ref Bot, ref BotModules);
        TokenCommands = new TokenCommands(Bot, BotModules);
        PunishmentCommands = new PunishmentCommands(Bot, BotModules);
    }
}