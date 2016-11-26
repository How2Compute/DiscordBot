using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;

internal class TokenModule
{
    Modules Modules;

    public TokenModule(Modules BotModules)
    {
        Modules = BotModules;
        
    }

    // Base64 encodes the user's id sothat it looks a bit more like a token.
    public string GetUserTrackHash(ulong UserID)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(UserID.ToString());
        return System.Convert.ToBase64String(plainTextBytes);
    }

    // BEING DEPRICATED!!! Get's the inputted users tracking has (base64 encoded discord user ID)
    public string GetUserTrackHash(Discord.User User)
    {
        return GetUserTrackHash(User.Id);
    }

    // Decode the tracking hash (base64 encoded discord user ID).
    public ulong TrackHashToID(string base64EncodedData)
    {
        var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
        return (ulong) Convert.ToInt64(System.Text.Encoding.UTF8.GetString(base64EncodedBytes));
    }
}