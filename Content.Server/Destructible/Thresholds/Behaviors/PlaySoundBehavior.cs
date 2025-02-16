using Content.Shared.Audio;
using Content.Shared.Sound;
using Robust.Shared.Audio;
using Robust.Shared.Player;

namespace Content.Server.Destructible.Thresholds.Behaviors
{
    [Serializable]
    [DataDefinition]
    public sealed class PlaySoundBehavior : IThresholdBehavior
    {
        /// <summary>
        ///     Sound played upon destruction.
        /// </summary>
        [DataField("sound", required: true)] public SoundSpecifier Sound { get; set; } = default!;

        public void Execute(EntityUid owner, DestructibleSystem system)
        {
            var pos = system.EntityManager.GetComponent<TransformComponent>(owner).Coordinates;
            SoundSystem.Play(Filter.Pvs(pos), Sound.GetSound(), pos, AudioHelpers.WithVariation(0.125f));
        }
    }
}
