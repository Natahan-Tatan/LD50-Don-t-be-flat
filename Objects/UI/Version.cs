using Godot;

public class Version : Label
{
    public override void _Ready()
    {
        GD.Print("Version: ", ProjectSettings.GetSetting("application/config/version"));
        Text = "v" + ProjectSettings.GetSetting("application/config/version") as string;
    }
}
