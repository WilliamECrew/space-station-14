using Content.Server.DoAfter;
using Content.Server.Engineering.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Verbs;
using JetBrains.Annotations;
namespace Content.Server.Engineering.EntitySystems
{
    [UsedImplicitly]
    public sealed class DisassembleOnAltVerbSystem : EntitySystem
    {
        [Dependency] private readonly SharedHandsSystem _handsSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<DisassembleOnAltVerbComponent, GetVerbsEvent<AlternativeVerb>>(AddDisassembleVerb);
        }
        private void AddDisassembleVerb(EntityUid uid, DisassembleOnAltVerbComponent component, GetVerbsEvent<AlternativeVerb> args)
        {
            if (!args.CanInteract || !args.CanAccess)
                return;

            AlternativeVerb verb = new()
            {
                Act = () =>
                {
                    AttemptDisassemble(uid, args.User, args.Target, component);
                },
                Text = Loc.GetString("disassemble-system-verb-disassemble"),
                Priority = 2
            };
            args.Verbs.Add(verb);
        }

        public async void AttemptDisassemble(EntityUid uid, EntityUid user, EntityUid target, DisassembleOnAltVerbComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return;
            if (string.IsNullOrEmpty(component.Prototype))
                return;

            if (component.DoAfterTime > 0 && TryGet<DoAfterSystem>(out var doAfterSystem))
            {
                var doAfterArgs = new DoAfterEventArgs(user, component.DoAfterTime, component.TokenSource.Token)
                {
                    BreakOnUserMove = true,
                    BreakOnStun = true,
                };
                var result = await doAfterSystem.WaitDoAfter(doAfterArgs);

                if (result != DoAfterStatus.Finished)
                    return;
                component.TokenSource.Cancel();
            }

            if (component.Deleted || Deleted(component.Owner))
                return;

            if (!TryComp<TransformComponent>(component.Owner, out var transformComp))
                return;

            var entity = EntityManager.SpawnEntity(component.Prototype, transformComp.Coordinates);

            _handsSystem.TryPickup(user, entity);

            EntityManager.DeleteEntity(component.Owner);

            return;
        }
    }
}
