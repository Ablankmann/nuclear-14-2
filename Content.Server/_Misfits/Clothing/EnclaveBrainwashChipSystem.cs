using Content.Server.Chat.Systems;
using Content.Shared._Misfits.Clothing;
using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.Chat;
using Content.Shared.Construction;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Inventory.Events;
using Content.Shared.Lock;
using Content.Shared.Mobs.Components;
using Content.Shared.Tools.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Misfits.Clothing;

/// <summary>
/// Handles Enclave chip lock enforcement, rescue cutting with emote broadcast,
/// and unique key generation when a bracelet is crafted. Mirrors the Legion Collars & NCR Bracelets.
/// </summary>
public sealed class EnclaveBrainwashChipSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedToolSystem _tools = default!;
    [Dependency] private readonly LockSystem _lock = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EnclaveBrainwashChipComponent, IsUnequippingAttemptEvent>(OnUnequippingAttempt);
        SubscribeLocalEvent<EnclaveBrainwashChipComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<EnclaveBrainwashChipComponent, EnclaveBrainwashChipCutDoAfterEvent>(OnBChipCut);
        SubscribeLocalEvent<EnclaveBrainwashChipComponent, ConstructionCompletedEvent>(OnConstructionCompleted);
    }

    private void OnUnequippingAttempt(Entity<EnclaveBrainwashChipComponent> ent, ref IsUnequippingAttemptEvent args)
    {
        if (args.Equipment != ent.Owner)
            return;

        if (!TryComp<LockComponent>(ent, out var lockComp) || !lockComp.Locked)
            return;

        // Enclave officers (Senior, Junior, Commander) and holders of the paired chip key may remove it.
        if (_lock.TryUnlock(ent, args.Unequipee, lockComp, skipDoAfter: true))
            return;

        args.Cancel();
    }

    private void OnInteractUsing(Entity<EnclaveBrainwashChipComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<LockComponent>(ent, out var lockComp) || !lockComp.Locked)
            return;

        // Rescue path: cut out the implanter with wire cutters or another cutting tool.
        var started = _tools.UseTool(args.Used, args.User, ent, ent.Comp.CutUnlockTime, ent.Comp.CutToolQuality,
            new EnclaveBrainwashChipCutDoAfterEvent());

        if (started)
        {
            // Broadcast a visible emote so bystanders know the cut is in progress.
            var wearer = Transform(ent.Owner).ParentUid;
            if (EntityManager.EntityExists(wearer) && wearer != args.User && HasComp<MobStateComponent>(wearer))
            {
                var wearerName = Identity.Entity(wearer, EntityManager);
                _chat.TrySendInGameICMessage(args.User,
                    Loc.GetString("misfits-chat-enclave-bchip-removing", ("target", wearerName)),
                    InGameICChatType.Emote, ChatTransmitRange.Normal, ignoreActionBlocker: true);
            }
        }

        args.Handled = started;
    }

    private void OnBChipCut(Entity<EnclaveBrainwashChipComponent> ent, ref EnclaveBrainwashChipCutDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        if (!TryComp<LockComponent>(ent, out var lockComp) || !lockComp.Locked)
            return;

        _lock.Unlock(ent, args.User, lockComp);
    }

    private void OnConstructionCompleted(Entity<EnclaveBrainwashChipComponent> ent, ref ConstructionCompletedEvent args)
    {
        if (ent.Comp.GeneratedKey || args.UserUid == null)
            return;

        if (ent.Comp.RandomKeyMin > ent.Comp.RandomKeyMax)
            return;

        // Stamp a unique runtime access tag onto the crafted bracelet and produce a matching key in the crafter's hand.
        var randomKey = _random.Next(ent.Comp.RandomKeyMin, ent.Comp.RandomKeyMax + 1);
        var accessTag = $"{ent.Comp.RandomAccessPrefix}{randomKey}";

        var reader = EnsureComp<AccessReaderComponent>(ent);
        reader.AccessLists.Add(new HashSet<ProtoId<AccessLevelPrototype>> { accessTag });
        Dirty(ent, reader);

        var key = Spawn(ent.Comp.KeyPrototype, _transform.GetMapCoordinates(args.UserUid.Value));
        var keyAccess = EnsureComp<AccessComponent>(key);
        keyAccess.Tags.Clear();
        keyAccess.Tags.Add(accessTag);
        Dirty(key, keyAccess);

        _meta.SetEntityName(key, $"implanting device key #{randomKey}");
        _hands.PickupOrDrop(args.UserUid.Value, key);

        ent.Comp.GeneratedKey = true;
        Dirty(ent);
    }
}
