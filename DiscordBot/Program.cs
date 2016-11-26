using Discord;
using Discord.Commands;
using System;

internal class Program
{
    private static void Main(string[] args) => new Program().Start();

    private DiscordClient Bot;
    private BotUser User;
    private Commander Commander;

    private void OnProcessExit(object sender, EventArgs e)
    {
        Console.WriteLine("I'm out of here");
    }

    public void Start()
    {
        AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
        Bot = new DiscordClient();
        User = new BotUser(Bot);
        //Commander = new Commander(ref Bot, ref User.Modules);
        new XP(ref Bot, User.Modules);

        
        Bot.MessageReceived += (s, e) =>
        {
            if (!e.Message.IsAuthor)
            {
                User.MessageSent(e.User.Id);
            }
        };

        Bot.UsingCommands(x =>
        {
            x.AllowMentionPrefix = true;
            x.PrefixChar = '$';
            x.HelpMode = HelpMode.Public;
        });

        
        // Connect the bot to the discord API.
        Bot.ExecuteAndWait(async () =>
            {
            // TODO make this connect to api token
            BotConnector MyBotConnector = new BotConnector();
                await Bot.Connect(BotConnector.GetAPIToken(), TokenType.Bot);
                Bot.SetGame("Testing biatch!");
            });
    }
}