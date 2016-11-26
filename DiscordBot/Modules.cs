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
        SaveModule = new SaveModule(this, "UserData.json");
        DataModule = new DataModule(this);
        PunishmentModule = new PunishmentModule(this);
        TokenModule = new TokenModule(this);
        UserModule = new UserModule(this);
        XPModule = new XPModule(this);
    }
}