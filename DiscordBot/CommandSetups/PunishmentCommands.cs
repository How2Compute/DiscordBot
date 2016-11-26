internal class PunishmentCommands
{
    private Strike StrikeCommand;

    public PunishmentCommands(Discord.DiscordClient Bot, Modules BotModules)
    {
        StrikeCommand = new Strike(Bot, BotModules);
    }
}