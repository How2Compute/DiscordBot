internal class TokenCommands
{
    private GetToken GetTokenCommand;
    private DeToken DeTokenCommand;

    public TokenCommands(Discord.DiscordClient Bot, Modules BotModules)
    {
        GetTokenCommand = new GetToken(Bot, BotModules);
        DeTokenCommand = new DeToken(Bot, BotModules);
    }
}