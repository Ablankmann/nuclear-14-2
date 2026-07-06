namespace Content.Server._Nuclear14.VaultDoorConsole;

[RegisterComponent, Access(typeof(VaultDoorConsoleSystem))]
public sealed partial class VaultDoorPendingBoltComponent : Component
{
    public EntityUid Console;
}
