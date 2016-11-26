using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class GetToken
    {
    public GetToken(Discord.DiscordClient Bot, Modules BotModules)
    {
        Bot.GetService<CommandService>().CreateCommand("GetToken")
            .Alias(new string[] { "TrackHash" })
            .Description("Gets your tracking token")
            .Parameter("UserToGetHashFor", ParameterType.Optional)
            .Do(async e =>
            {
                if (e.GetArg("UserToGetHashFor") != "") // If the user it needs to hash is specified get that users hash
                {
                    await e.Channel.SendMessage($"{e.GetArg("UserToGetHashFor")}'s hash is: {BotModules.TokenModule.GetUserTrackHash(BotModules.UserModule.GetUserByName(e.GetArg("UserToGetHashFor"), e.Channel).User.Id)}!"); // TODO make this be more checking as it may get the wrong user or something
                }
                else // If there isn't another user specified get the sending users hash.
                {
                    await e.Channel.SendMessage($"{e.User.NicknameMention}'s hash is: {BotModules.TokenModule.GetUserTrackHash(e.User)}!");
                }
            });
    }
    }
