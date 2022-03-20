namespace BadVM.Drives;

public class DrivePluginSettings
{

    public bool Enable { get; set; } = true;

    public DriveSettings[] Drives { get; set; } = new[] { new DriveSettings() };

}
