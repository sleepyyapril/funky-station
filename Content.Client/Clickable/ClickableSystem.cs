// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Content.Client.Sprite;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Utility;
using Robust.Shared.Graphics;

namespace Content.Client.Clickable;

/// <summary>
/// Handles click detection for sprites.
/// </summary>
public sealed class ClickableSystem : EntitySystem
{
    [Dependency] private readonly IClickMapManager _clickMapManager = default!;
    [Dependency] private readonly SharedTransformSystem _transforms = default!;
    [Dependency] private readonly SpriteSystem _sprites = default!;

    private EntityQuery<ClickableComponent> _clickableQuery;
    private EntityQuery<TransformComponent> _xformQuery;
    private EntityQuery<FadingSpriteComponent> _fadingSpriteQuery;

    public override void Initialize()
    {
        base.Initialize();
        _clickableQuery = GetEntityQuery<ClickableComponent>();
        _xformQuery = GetEntityQuery<TransformComponent>();
        _fadingSpriteQuery = GetEntityQuery<FadingSpriteComponent>();
    }

    /// <summary>
    /// Used to check whether a click worked. Will first check if the click falls inside of some explicit bounding
    /// boxes (see <see cref="Bounds"/>). If that fails, attempts to use automatically generated click maps.
    /// </summary>
    /// <param name="worldPos">The world position that was clicked.</param>
    /// <param name="drawDepth">
    /// The draw depth for the sprite that captured the click.
    /// </param>
    /// <returns>True if the click worked, false otherwise.</returns>
    public bool CheckClick(Entity<ClickableComponent?, SpriteComponent, TransformComponent?, FadingSpriteComponent?> entity, Vector2 worldPos, IEye eye, bool excludeFaded, out int drawDepth, out uint renderOrder, out float bottom)
    {
        if (!_clickableQuery.Resolve(entity.Owner, ref entity.Comp1, false))
        {
            drawDepth = default;
            renderOrder = default;
            bottom = default;
            return false;
        }

        if (!_xformQuery.Resolve(entity.Owner, ref entity.Comp3))
        {
            drawDepth = default;
            renderOrder = default;
            bottom = default;
            return false;
        }

        if (excludeFaded && _fadingSpriteQuery.Resolve(entity.Owner, ref entity.Comp4, false))
        {
            drawDepth = default;
            renderOrder = default;
            bottom = default;
            return false;
        }

        var sprite = entity.Comp2;
        var transform = entity.Comp3;

        if (!sprite.Visible)
        {
            drawDepth = default;
            renderOrder = default;
            bottom = default;
            return false;
        }

        drawDepth = sprite.DrawDepth;
        renderOrder = sprite.RenderOrder;
        var (spritePos, spriteRot) = _transforms.GetWorldPositionRotation(transform);
        var spriteBB = sprite.CalculateRotatedBoundingBox(spritePos, spriteRot, eye.Rotation);
        bottom = Matrix3Helpers.CreateRotation(eye.Rotation).TransformBox(spriteBB).Bottom;

        Matrix3x2.Invert(sprite.GetLocalMatrix(), out var invSpriteMatrix);

        // This should have been the rotation of the sprite relative to the screen, but this is not the case with no-rot or directional sprites.
        var relativeRotation = (spriteRot + eye.Rotation).Reduced().FlipPositive();

        var cardinalSnapping = sprite.SnapCardinals ? relativeRotation.GetCardinalDir().ToAngle() : Angle.Zero;

        // First we get `localPos`, the clicked location in the sprite-coordinate frame.
        var entityXform = Matrix3Helpers.CreateInverseTransform(spritePos, sprite.NoRotation ? -eye.Rotation : spriteRot - cardinalSnapping);
        var localPos = Vector2.Transform(Vector2.Transform(worldPos, entityXform), invSpriteMatrix);

        // Check explicitly defined click-able bounds
        if (CheckDirBound((entity.Owner, entity.Comp1, entity.Comp2), relativeRotation, localPos))
            return true;

        // Next check each individual sprite layer using automatically computed click maps.
        foreach (var spriteLayer in sprite.AllLayers)
        {
            if (spriteLayer is not SpriteComponent.Layer layer || !_sprites.IsVisible(layer))
            {
                continue;
            }

            // Check the layer's texture, if it has one
            if (layer.Texture != null)
            {
                // Convert to image coordinates
                var imagePos = (Vector2i) (localPos * EyeManager.PixelsPerMeter * new Vector2(1, -1) + layer.Texture.Size / 2f);

                if (_clickMapManager.IsOccluding(layer.Texture, imagePos))
                    return true;
            }

            // Either we weren't clicking on the texture, or there wasn't one. In which case: check the RSI next
            if (layer.ActualRsi is not { } rsi || !rsi.TryGetState(layer.State, out var rsiState))
                continue;

            var dir = SpriteComponent.Layer.GetDirection(rsiState.RsiDirections, relativeRotation);

            // convert to layer-local coordinates
            layer.GetLayerDrawMatrix(dir, out var matrix);
            Matrix3x2.Invert(matrix, out var inverseMatrix);
            var layerLocal = Vector2.Transform(localPos, inverseMatrix);

            // Convert to image coordinates
            var layerImagePos = (Vector2i) (layerLocal * EyeManager.PixelsPerMeter * new Vector2(1, -1) + rsiState.Size / 2f);

            // Next, to get the right click map we need the "direction" of this layer that is actually being used to draw the sprite on the screen.
            // This **can** differ from the dir defined before, but can also just be the same.
            if (sprite.EnableDirectionOverride)
                dir = sprite.DirectionOverride.Convert(rsiState.RsiDirections);
            dir = dir.OffsetRsiDir(layer.DirOffset);

            if (_clickMapManager.IsOccluding(layer.ActualRsi!, layer.State, dir, layer.AnimationFrame, layerImagePos))
                return true;
        }

        drawDepth = default;
        renderOrder = default;
        bottom = default;
        return false;
    }

    public bool CheckDirBound(Entity<ClickableComponent, SpriteComponent> entity, Angle relativeRotation, Vector2 localPos)
    {
        var clickable = entity.Comp1;
        var sprite = entity.Comp2;

        if (clickable.Bounds == null)
            return false;

        // These explicit bounds only work for either 1 or 4 directional sprites.

        // This would be the orientation of a 4-directional sprite.
        var direction = relativeRotation.GetCardinalDir();

        var modLocalPos = sprite.NoRotation
            ? localPos
            : direction.ToAngle().RotateVec(localPos);

        // First, check the bounding box that is valid for all orientations
        if (clickable.Bounds.All.Contains(modLocalPos))
            return true;

        // Next, get and check the appropriate bounding box for the current sprite orientation
        var boundsForDir = (sprite.EnableDirectionOverride ? sprite.DirectionOverride : direction) switch
        {
            Direction.East => clickable.Bounds.East,
            Direction.North => clickable.Bounds.North,
            Direction.South => clickable.Bounds.South,
            Direction.West => clickable.Bounds.West,
            _ => throw new InvalidOperationException()
        };

        return boundsForDir.Contains(modLocalPos);
    }
}
