internal class Modules
{
    public SaveModule SaveModule;
    public TokenModule TokenModule;

    public Modules()
    {
        SaveModule = new SaveModule("UserData.json");
        TokenModule = new TokenModule();
    }
}