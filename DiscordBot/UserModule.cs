internal class UserModule
{
    private Modules Modules;

    public UserModule(Modules BotModules)
    {
        Modules = BotModules;
    }

    // Attempts to find a user by the inputted name, returns search result. TODO make this not user based but ID based?
    public UserSearchResult GetUserByName(string Name, Discord.Channel ChannelToSearch)
    {
        // Create default struct and set it to not found in case nothing was found later on.
        UserSearchResult SearchResult = new UserSearchResult();
        SearchResult.WasFound = false;

        // Go through all the users in a given channel and try to find a user by the specified name.
        foreach (Discord.User User in ChannelToSearch.Users)
        {
            // If the user with a matching name or nickname is found...
            if (User.Name == Name || User.Nickname == Name)
            {
                // Save the user into a variable, set it to found and exit the loop.
                SearchResult.User = User;
                SearchResult.WasFound = true;
                break;
            }
        }

        return SearchResult;
    }

    public UserSearchResult GetUserByID(ulong UserID, Discord.Server ServerToSearch)
    {
        // Create default struct and set it to not found in case nothing was found later on.
        UserSearchResult SearchResult = new UserSearchResult();
        SearchResult.WasFound = false;

        // Go through all the users in a given server.
        foreach (Discord.User User in ServerToSearch.Users)
        {
            // If a user with a matching ID was found...
            if (User.Id == UserID)
            {
                // Set the searchresult to this user, signal that it was found and exit the loop.
                SearchResult.User = User;
                SearchResult.WasFound = true;
                break;
            }
        }

        return SearchResult;
    }
}