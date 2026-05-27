using Content.Server.DeviceNetwork.Components;
using Content.Shared.DeviceLinking.Events;

namespace Content.Server.DeviceNetwork.Systems;

public sealed class BlockNetworkConfiguratorSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlockNetworkConfiguratorComponent, LinkAttemptEvent>(OnLinkAttempt);
    }

    private void OnLinkAttempt(EntityUid uid, BlockNetworkConfiguratorComponent component, LinkAttemptEvent args)
    {
        if (args.User != null)
            args.Cancel();
    }
}
