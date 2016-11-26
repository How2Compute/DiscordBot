using Discord;
using Discord.Commands;
using System;

internal class Program
{
    private static void Main(string[] args) => new Program().Start();

    private DiscordClient Bot;
    private BotUser User = new BotUser();

    private void OnProcessExit(object sender, EventArgs e)
    {
        Console.WriteLine("I'm out of here");
    }

    public void Start()
    {
        AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
        Bot = new DiscordClient();

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
                        if (User.GetUserByName(e.GetArg("UserToStrike"), e.Channel).WasFound)
                        {
                            await e.Channel.SendMessage($"You just got striked {User.GetUserByName(e.GetArg("UserToStrike"), e.Channel).User.Mention}!");
                            User.ChangeStrikes(e.Server, User.GetUserByName(e.GetArg("UserToStrike"), e.Channel).User.Id, 1);
                        }
                    }
                    else
                    {
                        await e.Channel.SendMessage("You ain't an admin bro...");
                    }
                }
            });
        Bot.GetService<CommandService>().CreateCommand("XP")
            .Alias(new string[] { "Rank" })
            .Description("Gives you your current ammount of xp!")
            .Do(async e =>
            {
                await e.Channel.SendMessage($"{e.User.NicknameMention} has {User.GetUserInfo(e.User.Id).XP} XP!!!"); // TODO make levels and ranking and shit
            });

        Bot.GetService<CommandService>().CreateCommand("Hash")
            .Alias(new string[] { "TrackHash" })
            .Description("Gets your tracking hash")
            .Parameter("UserToGetHashFor", ParameterType.Optional)
            .Do(async e =>
            {
                if (e.GetArg("UserToGetHashFor") != "") // If the user it needs to hash is specified get that users hash
                {
                    await e.Channel.SendMessage($"{e.GetArg("UserToGetHashFor")}'s hash is: {User.GetUserTrackHash(User.GetUserByName(e.GetArg("UserToGetHashFor"), e.Channel).User.Id)}!"); // TODO make this be more checking as it may get the wrong user or something
                }
                else // If there isn't another user specified get the sending users hash.
                {
                    await e.Channel.SendMessage($"{e.User.NicknameMention}'s hash is: {User.GetUserTrackHash(e.User)}!");
                }
            });

        // TODO remove this as it is only a test
        Bot.GetService<CommandService>().CreateCommand("DeHash")
            .Alias(new string[] { "ResolveTrackHash" })
            .Description("Test Track Hash Resolver")
            .Parameter("Hash", ParameterType.Optional)
            .Do(async e =>
            {
                await e.Channel.SendMessage("Atempting to resolve hash..."); // TODO remove cuz this is only for debugging
                await e.Channel.SendMessage($"{e.GetArg("Hash")} is actually #{User.TrackHashToID(e.GetArg("Hash"))} which in its turn is {User.GetUserByID(User.TrackHashToID(e.GetArg("Hash")), e.Server).User.NicknameMention}!"); // TODO make this be more checking as it may get the wrong user or something
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