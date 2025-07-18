// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Popups;
using Content.Shared.Projectiles;
using Content.Shared.StatusEffect;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server.Magic;

public sealed partial class ChainFireballSystem : EntitySystem
{
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChainFireballComponent, ProjectileHitEvent>(OnHit);
    }

    private void OnHit(Entity<ChainFireballComponent> ent, ref ProjectileHitEvent args)
    {
        if (_random.Prob(ent.Comp.DisappearChance))
            return;

        // spawn new fireball on target
        Spawn(args.Target, ent.Comp.IgnoredTargets);

        QueueDel(ent);
    }

    public bool Spawn(EntityUid source, List<EntityUid> ignoredTargets)
    {
        var lookup = _lookup.GetEntitiesInRange(source, 5f);

        List<EntityUid> mobs = new();
        foreach (var look in lookup)
        {
            if (ignoredTargets.Contains(look)
            || !HasComp<StatusEffectsComponent>(look)) // ignore non mobs
                continue;

            mobs.Add(look);
        }
        if (mobs.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("heretic-ability-fail-notarget"), source, source);
            return false;
        }

        return Spawn(source, mobs[_random.Next(0, mobs.Count - 1)], ignoredTargets);
    }
    public bool Spawn(EntityUid source, EntityUid target, List<EntityUid> ignoredTargets)
    {
        return SpawnFireball(source, target, ignoredTargets);
    }
    public bool SpawnFireball(EntityUid uid, EntityUid target, List<EntityUid> ignoredTargets)
    {
        var ball = Spawn("FireballChain", Transform(uid).Coordinates);

        // set ignore list if it wasn't set already
        if (TryComp<ChainFireballComponent>(ball, out var sfc))
            sfc.IgnoredTargets = sfc.IgnoredTargets.Count > 0 ? sfc.IgnoredTargets : ignoredTargets;

        // launch it towards the target
        var fromCoords = Transform(uid).Coordinates;
        var toCoords = Transform(target).Coordinates;
        var userVelocity = _physics.GetMapLinearVelocity(uid);

        // If applicable, this ensures the projectile is parented to grid on spawn, instead of the map.
        var fromMap = fromCoords.ToMap(EntityManager, _transform);
        var spawnCoords = _mapMan.TryFindGridAt(fromMap, out var gridUid, out _)
            ? fromCoords.WithEntityId(gridUid, EntityManager)
            : new(_mapMan.GetMapEntityId(fromMap.MapId), fromMap.Position);


        var direction = toCoords.ToMapPos(EntityManager, _transform) -
                        spawnCoords.ToMapPos(EntityManager, _transform);

        _gun.ShootProjectile(ball, direction, userVelocity, uid, uid);

        return true;
    }
}
