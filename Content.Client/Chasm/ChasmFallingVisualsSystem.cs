// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Chasm;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;

namespace Content.Client.Chasm;

/// <summary>
///     Handles the falling animation for entities that fall into a chasm.
/// </summary>
public sealed class ChasmFallingVisualsSystem : EntitySystem
{
    [Dependency] private readonly AnimationPlayerSystem _anim = default!;

    private readonly string _chasmFallAnimationKey = "chasm_fall";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChasmFallingComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<ChasmFallingComponent, ComponentRemove>(OnComponentRemove);
    }

    private void OnComponentInit(EntityUid uid, ChasmFallingComponent component, ComponentInit args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite) ||
            TerminatingOrDeleted(uid))
        {
            return;
        }

        component.OriginalScale = sprite.Scale;

        if (!TryComp<AnimationPlayerComponent>(uid, out var player))
            return;

        if (_anim.HasRunningAnimation(player, _chasmFallAnimationKey))
            return;

        _anim.Play((uid, player), GetFallingAnimation(component), _chasmFallAnimationKey);
    }

    private void OnComponentRemove(EntityUid uid, ChasmFallingComponent component, ComponentRemove args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        sprite.Scale = component.OriginalScale;

        if (!TryComp<AnimationPlayerComponent>(uid, out var player))
            return;

        if (_anim.HasRunningAnimation(player, _chasmFallAnimationKey))
            _anim.Stop((uid, player), _chasmFallAnimationKey);
    }

    private Animation GetFallingAnimation(ChasmFallingComponent component)
    {
        var length = component.AnimationTime;

        return new Animation()
        {
            Length = length,
            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Scale),
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(component.OriginalScale, 0.0f),
                        new AnimationTrackProperty.KeyFrame(component.AnimationScale, length.Seconds),
                    },
                    InterpolationMode = AnimationInterpolationMode.Cubic
                }
            }
        };
    }
}
