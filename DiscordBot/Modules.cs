using System.Collections.Generic;

internal class Modules
{
    public DataModule DataModule;
    public PunishmentModule PunishmentModule;
    public SaveModule SaveModule;
    public TokenModule TokenModule;
    public UserModule UserModule;
    public XPModule XPModule;

    public Modules()
    {
        DataModule = new DataModule(this);
        PunishmentModule = new PunishmentModule(this);
        SaveModule = new SaveModule(this, "UserData.json");
        TokenModule = new TokenModule(this);
        UserModule = new UserModule(this);
        XPModule = new XPModule(this);
    }
}