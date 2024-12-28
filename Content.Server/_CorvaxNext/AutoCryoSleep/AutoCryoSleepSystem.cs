/*
 * Author: TornadoTech
 * License: AGPL
 */

using System.Diagnostics.CodeAnalysis;
using Content.Server.Afk.Events;
using Content.Server.Bed.Cryostorage;
using Content.Server.Mind;
using Content.Shared.Bed.Cryostorage;
using Content.Shared.Station.Components;
using Robust.Server.Containers;
using Robust.Shared.Timing;

namespace Content.Server._CorvaxNext.AutoCryoSleep;

public sealed class AutoCryoSleepSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly CryostorageSystem _cryostorage = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly MindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AFKEvent>(OnAFK);
    }

    private void OnAFK(ref AFKEvent ev)
    {
        if (ev.Session.AttachedEntity is not { } entityUid)
            return;

        TryCryoSleep(entityUid);
    }

    private void TryCryoSleep(EntityUid targetUid)
    {
        if (HasComp<CryostorageContainedComponent>(targetUid))
            return;

        if (!_mind.TryGetMind(targetUid, out _, out var mindComponent))
            return;

        if (!TryGetStation(targetUid, out var stationUid))
            return;

        foreach (var cryostorageEntity in EnumerateCryostorageInSameStation(stationUid.Value))
        {
            if (!_container.TryGetContainer(cryostorageEntity, cryostorageEntity.Comp.ContainerId, out var container))
                continue;

            // We need only empty cryo sleeps
            if (container.ContainedEntities.Count != 0)
                continue;

            var cryostorageContained = AddComp<CryostorageContainedComponent>(targetUid);
            cryostorageContained.Cryostorage = cryostorageEntity;
            cryostorageContained.GracePeriodEndTime = _timing.CurTime;

            _cryostorage.HandleEnterCryostorage((targetUid, cryostorageContained), mindComponent.UserId);
        }
    }

    private IEnumerable<Entity<CryostorageComponent>> EnumerateCryostorageInSameStation(EntityUid stationUid)
    {
        var query = AllEntityQuery<CryostorageComponent>();
        while (query.MoveNext(out var entityUid, out var cryostorageComponent))
        {
            if (!TryGetStation(entityUid, out var cryoStationUid))
                continue;

            if (stationUid != cryoStationUid)
                continue;

            yield return (entityUid, cryostorageComponent);
        }
    }

    private bool TryGetStation(EntityUid entityUid, [NotNullWhen(true)] out EntityUid? stationUid)
    {
        stationUid = null;
        var gridUid = Transform(entityUid).GridUid;

        if (!TryComp<StationMemberComponent>(gridUid, out var stationMemberComponent))
            return false;

        stationUid = stationMemberComponent.Station;
        return true;
    }
}
