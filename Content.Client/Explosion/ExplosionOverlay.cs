// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aexxie <codyfox.077@gmail.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Content.Shared.Explosion.Components;
using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Client.Explosion;

[UsedImplicitly]
public sealed class ExplosionOverlay : Overlay
{
    [Dependency] private readonly IRobustRandom _robustRandom = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    private readonly SharedTransformSystem _transformSystem;
    private SharedAppearanceSystem _appearance;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    private ShaderInstance _shader;

    public ExplosionOverlay(SharedAppearanceSystem appearanceSystem)
    {
        IoCManager.InjectDependencies(this);
        _shader = _proto.Index<ShaderPrototype>("unshaded").Instance();
        _transformSystem = _entMan.System<SharedTransformSystem>();
        _appearance = appearanceSystem;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var drawHandle = args.WorldHandle;
        drawHandle.UseShader(_shader);

        var xforms = _entMan.GetEntityQuery<TransformComponent>();
        var query = _entMan.EntityQueryEnumerator<ExplosionVisualsComponent, ExplosionVisualsTexturesComponent>();

        while (query.MoveNext(out var uid, out var visuals, out var textures))
        {
            if (visuals.Epicenter.MapId != args.MapId)
                continue;

            if (!_appearance.TryGetData(uid, ExplosionAppearanceData.Progress, out int index))
                continue;

            index = Math.Min(index, visuals.Intensity.Count - 1);
            DrawExplosion(drawHandle, args.WorldBounds, visuals, index, xforms, textures);
        }

        drawHandle.SetTransform(Matrix3x2.Identity);
        drawHandle.UseShader(null);
    }

    private void DrawExplosion(
        DrawingHandleWorld drawHandle,
        Box2Rotated worldBounds,
        ExplosionVisualsComponent visuals,
        int index,
        EntityQuery<TransformComponent> xforms,
        ExplosionVisualsTexturesComponent textures)
    {
        Box2 gridBounds;
        foreach (var (gridId, tiles) in visuals.Tiles)
        {
            if (!_entMan.TryGetComponent(gridId, out MapGridComponent? grid))
                continue;

            var xform = xforms.GetComponent(gridId);
            var (_, _, worldMatrix, invWorldMatrix) = _transformSystem.GetWorldPositionRotationMatrixWithInv(xform, xforms);

            gridBounds = invWorldMatrix.TransformBox(worldBounds).Enlarged(grid.TileSize * 2);
            drawHandle.SetTransform(worldMatrix);

            DrawTiles(drawHandle, gridBounds, index, tiles, visuals, grid.TileSize, textures);
        }

        if (visuals.SpaceTiles == null)
            return;

        Matrix3x2.Invert(visuals.SpaceMatrix, out var invSpace);
        gridBounds = invSpace.TransformBox(worldBounds).Enlarged(2);
        drawHandle.SetTransform(visuals.SpaceMatrix);

        DrawTiles(drawHandle, gridBounds, index, visuals.SpaceTiles, visuals, visuals.SpaceTileSize, textures);
    }

    private void DrawTiles(
        DrawingHandleWorld drawHandle,
        Box2 gridBounds,
        int index,
        Dictionary<int, List<Vector2i>> tileSets,
        ExplosionVisualsComponent visuals,
        ushort tileSize,
        ExplosionVisualsTexturesComponent textures)
    {
        for (var j = 0; j <= index; j++)
        {
            if (!tileSets.TryGetValue(j, out var tiles))
                continue;

            var frameIndex = (int) Math.Min(visuals.Intensity[j] / textures.IntensityPerState, textures.FireFrames.Count - 1);
            var frames = textures.FireFrames[frameIndex];

            foreach (var tile in tiles)
            {
                var centre = (tile + Vector2Helpers.Half) * tileSize;

                if (!gridBounds.Contains(centre))
                    continue;

                var texture = _robustRandom.Pick(frames);
                drawHandle.DrawTextureRect(texture, Box2.CenteredAround(centre, new Vector2(tileSize, tileSize)), textures.FireColor);
            }
        }
    }
}
