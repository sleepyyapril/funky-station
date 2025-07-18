// SPDX-FileCopyrightText: 2022 Jessica M <jessica@jessicamaybe.com>
// SPDX-FileCopyrightText: 2023 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Psychpsyo <60073468+Psychpsyo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Nutrition.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Server.Nutrition.EntitySystems
{
    public sealed class TrashOnSolutionEmptySystem : EntitySystem
    {
        [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
        [Dependency] private readonly TagSystem _tagSystem = default!;

        private static readonly ProtoId<TagPrototype> TrashTag = "Trash";

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<TrashOnSolutionEmptyComponent, MapInitEvent>(OnMapInit);
            SubscribeLocalEvent<TrashOnSolutionEmptyComponent, SolutionContainerChangedEvent>(OnSolutionChange);
        }

        public void OnMapInit(Entity<TrashOnSolutionEmptyComponent> entity, ref MapInitEvent args)
        {
            CheckSolutions(entity);
        }

        public void OnSolutionChange(Entity<TrashOnSolutionEmptyComponent> entity, ref SolutionContainerChangedEvent args)
        {
            CheckSolutions(entity);
        }

        public void CheckSolutions(Entity<TrashOnSolutionEmptyComponent> entity)
        {
            if (!EntityManager.HasComponent<SolutionContainerManagerComponent>(entity))
                return;

            if (_solutionContainerSystem.TryGetSolution(entity.Owner, entity.Comp.Solution, out _, out var solution))
                UpdateTags(entity, solution);
        }

        public void UpdateTags(Entity<TrashOnSolutionEmptyComponent> entity, Solution solution)
        {
            if (solution.Volume <= 0)
            {
                _tagSystem.AddTag(entity.Owner, TrashTag);
                return;
            }

            _tagSystem.RemoveTag(entity.Owner, TrashTag);
        }
    }
}
