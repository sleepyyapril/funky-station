// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Botany.Components;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects.PlantMetabolism;

[UsedImplicitly]
[DataDefinition]
public sealed partial class PlantPhalanximine : EntityEffect
{
    public override void Effect(EntityEffectBaseArgs args)
    {
        if (!args.EntityManager.TryGetComponent(args.TargetEntity, out PlantHolderComponent? plantHolderComp)
                                || plantHolderComp.Seed == null || plantHolderComp.Dead ||
                                plantHolderComp.Seed.Immutable)
            return;

        plantHolderComp.Seed.Viable = true;
    }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) => Loc.GetString("reagent-effect-guidebook-plant-phalanximine", ("chance", Probability));
}
