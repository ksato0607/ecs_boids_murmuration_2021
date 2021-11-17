using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
namespace ColdShowerGames {

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public class BoidSystem : SystemBase {
        private EntityQuery _boidQuery;
        private EntityQuery _targetsQuery;
        private readonly List<Boid> _boidTypes = new();


        protected override void OnStartRunning() {
            base.OnStartRunning();
            _boidQuery = GetEntityQuery(ComponentType.ReadOnly<Boid>(),
                ComponentType.ReadWrite<LocalToWorld>());
            _targetsQuery = GetEntityQuery(ComponentType.ReadOnly<BoidTarget>(),
                ComponentType.ReadOnly<LocalToWorld>());

            RequireForUpdate(_boidQuery);
            RequireForUpdate(_targetsQuery);
        }

        protected override void OnUpdate() {
            var dt = Time.DeltaTime;

            EntityManager.GetAllUniqueSharedComponentData(_boidTypes);
            var cellRadius = Controller.Instance.CellSizeVaried;
            var positionOffsetVary = Controller.Instance.CellPositionOffsetVaried;


            foreach (var boidType in _boidTypes) {
                _boidQuery.AddSharedComponentFilter(boidType);
                var nrOfBoids = _boidQuery.CalculateEntityCount();
                var nrOfTargets = _targetsQuery.CalculateEntityCount();

                if (nrOfBoids == 0) {
                    _boidQuery.ResetFilter();
                    continue;
                }

#region SYSTEM DATA DECLARATIONS

                // hashmap where key is the cell index (calculated based on the boid's floored position)
                // and the value is an array with the indices of the boids that are contained within that cell.
                var boidIndicesPerCell = new NativeMultiHashMap<int, int>(nrOfBoids, Allocator.TempJob);

                // positions of all boids
                var positions = new NativeArray<float3>(nrOfBoids,
                    Allocator.TempJob,
                    NativeArrayOptions.UninitializedMemory);

                // headings of all boids
                var headings = new NativeArray<float3>(nrOfBoids,
                    Allocator.TempJob,
                    NativeArrayOptions.UninitializedMemory);

                // perBoidCellIndex[boidEntityIndex] = index of the cell. Used to retrieve data about the cell 
                var perBoidCellIndex = new NativeArray<int>(nrOfBoids,
                    Allocator.TempJob,
                    NativeArrayOptions.UninitializedMemory);

                // nr of boids inside a cell
                var perCellCount = new NativeArray<int>(nrOfBoids,
                    Allocator.TempJob,
                    NativeArrayOptions.UninitializedMemory);

                // summed positions of boids per cell
                var perCellPositions = new NativeArray<float3>(nrOfBoids,
                    Allocator.TempJob,
                    NativeArrayOptions.UninitializedMemory);

                // summed headings of boids per cell
                var perCellHeadings = new NativeArray<float3>(nrOfBoids,
                    Allocator.TempJob,
                    NativeArrayOptions.UninitializedMemory);

                // positions of all targets
                var targetPositions = new NativeArray<float3>(nrOfTargets,
                    Allocator.TempJob,
                    NativeArrayOptions.UninitializedMemory);

                // radius to avoid this target
                var targetAvoidanceRanges = new NativeArray<float>(nrOfTargets,
                    Allocator.TempJob,
                    NativeArrayOptions.UninitializedMemory);

                // per each cell, this is the position of the closes target
                var perCellClosestTargetIndex = new NativeArray<int>(nrOfBoids,
                    Allocator.TempJob,
                    NativeArrayOptions.UninitializedMemory);

                // per each cell, this is the position of the closes target
                var perCellClosestTargetDistanceSq = new NativeArray<float>(nrOfBoids,
                    Allocator.TempJob,
                    NativeArrayOptions.UninitializedMemory);

  #endregion

                // gather data from existing boids
                var hashMapWriter = boidIndicesPerCell.AsParallelWriter();
                var retrieveBoidDataJobHandle = Entities.WithName("RetrieveBoidDataJob")
                    .WithAll<Boid>()
                    .ForEach((int entityInQueryIndex, in LocalToWorld localToWorld) => {

                        positions[entityInQueryIndex] = localToWorld.Position;
                        headings[entityInQueryIndex] = localToWorld.Forward;

                        // include this boid's index into the containing cell (based on its hashed position) 
                        var cellHash = (int)math.hash(new int3(math.floor(
                            (localToWorld.Position + positionOffsetVary) /
                            (cellRadius + boidType.AddedCellRadius))));
                        hashMapWriter.Add(cellHash, entityInQueryIndex);
                    })
                    .ScheduleParallel(Dependency);

                var retrieveTargetsDataJobHandle = Entities.WithName("RetrieveBoidTargetDataJob")
                    .WithAll<BoidTarget>()
                    .ForEach((
                        int entityInQueryIndex,
                        in LocalToWorld localToWorld,
                        in BoidTarget boidTarget) => {

                        targetPositions[entityInQueryIndex] = localToWorld.Position;
                        targetAvoidanceRanges[entityInQueryIndex] = boidTarget.AvoidanceRadius;
                    })
                    .ScheduleParallel(Dependency);


                var retrieveDataJobHandle =
                    JobHandle.CombineDependencies(retrieveBoidDataJobHandle, retrieveTargetsDataJobHandle);

                var createCellDataJobHandle = new MergeCells {
                    perBoidCellIndex = perBoidCellIndex,
                    perCellCount = perCellCount,
                    headings = headings,
                    positions = positions,
                    perCellHeadings = perCellHeadings,
                    perCellPositions = perCellPositions,
                    targetPositions = targetPositions,
                    perCellClosestTargetIndex = perCellClosestTargetIndex,
                    perCellClosestTargetDistanceSq = perCellClosestTargetDistanceSq
                }.Schedule(boidIndicesPerCell, 64, retrieveDataJobHandle);


                // apply movement to the units
                var applyMovementHandle = Entities.WithName("MoveBoidsJob")
                    .WithAll<Boid>()
                    .WithReadOnly(positions)
                    .WithReadOnly(headings)
                    .WithReadOnly(perCellPositions)
                    .WithReadOnly(perCellHeadings)
                    .WithReadOnly(perBoidCellIndex)
                    .WithReadOnly(perCellCount)
                    .WithReadOnly(targetPositions)
                    .WithReadOnly(targetAvoidanceRanges)
                    .WithReadOnly(perCellClosestTargetIndex)
                    .WithReadOnly(perCellClosestTargetDistanceSq)
                    .ForEach((int entityInQueryIndex, ref LocalToWorld localToWorld) => {

                        var forward = headings[entityInQueryIndex];
                        var currentPosition = positions[entityInQueryIndex];
                        var cellIndex = perBoidCellIndex[entityInQueryIndex];

                        var boidsInCellCount = perCellCount[cellIndex];
                        var neighboursCount = boidsInCellCount - 1;


                        var neighboursHeading =
                            math.normalizesafe((perCellHeadings[cellIndex] - forward) / neighboursCount);
                        var neighboursCenter =
                            (perCellPositions[cellIndex] - currentPosition) / neighboursCount;

                        // -------------------- alignment
                        var alignmentResult = boidType.AlignmentWeight * neighboursHeading;

                        // -------------------- separation and cohesion
                        var separationResult = float3.zero;
                        var cohesionResult = float3.zero;

                        if (neighboursCount > 0) {
                            var distanceToMiddle = math.distancesq(neighboursCenter, currentPosition);
                            var maxDistanceToMiddle = (cellRadius + boidType.AddedCellRadius) *
                                                      (cellRadius + boidType.AddedCellRadius);

                            var distanceToMiddleClamped = distanceToMiddle / maxDistanceToMiddle;
                            var needToLeave = 1 - distanceToMiddleClamped;

                            separationResult = boidType.SeparationWeight *
                                               needToLeave *
                                               math.normalizesafe(currentPosition - neighboursCenter);
                            cohesionResult = boidType.CohesionWeight *
                                             math.normalizesafe(neighboursCenter - currentPosition);
                        }

                        // -------------------- walk to targets
                        var walkToTargetsResult = float3.zero;
                        var closestTargetIndex = perCellClosestTargetIndex[cellIndex];
                        if (closestTargetIndex != -1) {
                            var closestTargetDistanceSq = perCellClosestTargetDistanceSq[cellIndex];

                            var closestTargetPosition = targetPositions[closestTargetIndex];
                            var targetAvoidDistance = targetAvoidanceRanges[closestTargetIndex];
                            targetAvoidDistance *= targetAvoidDistance;


                            // logic to avoid being close to target
                            var distanceToTargetClamped = closestTargetDistanceSq / targetAvoidDistance;
                            var needToAvoidTarget = math.max(1 - distanceToTargetClamped, 0);
                            needToAvoidTarget *= needToAvoidTarget;

                            if (needToAvoidTarget > 0) {
                                walkToTargetsResult = boidType.TargetAvoidanceWeight *
                                                      needToAvoidTarget *
                                                      math.normalizesafe(currentPosition -
                                                          closestTargetPosition);
                            }
                            else {
                                walkToTargetsResult = boidType.TargetWeight *
                                                      math.normalizesafe(closestTargetPosition -
                                                          currentPosition);
                            }
                        }

                        var resultHeading = cohesionResult +
                                            separationResult +
                                            alignmentResult +
                                            walkToTargetsResult;
                        var nextHeading = math.lerp(forward,
                            math.normalizesafe(resultHeading),
                            dt * boidType.TurnSpeed);

                        localToWorld = new LocalToWorld {
                            Value = float4x4.TRS(
                                new float3(currentPosition + nextHeading * boidType.MoveSpeed * dt),
                                quaternion.LookRotationSafe(nextHeading, math.up()),
                                boidType.Scale)
                        };

                    })
                    .ScheduleParallel(createCellDataJobHandle);



                Dependency = applyMovementHandle;
                _boidQuery.AddDependency(Dependency);
                _boidQuery.ResetFilter();

#region SYSTEM DATA DISPOSE

                boidIndicesPerCell.Dispose(Dependency);
                positions.Dispose(Dependency);
                headings.Dispose(Dependency);
                perBoidCellIndex.Dispose(Dependency);
                perCellCount.Dispose(Dependency);
                perCellHeadings.Dispose(Dependency);
                perCellPositions.Dispose(Dependency);

                targetAvoidanceRanges.Dispose(Dependency);
                targetPositions.Dispose(Dependency);
                perCellClosestTargetIndex.Dispose(Dependency);
                perCellClosestTargetDistanceSq.Dispose(Dependency);

#endregion
            }

            _boidTypes.Clear();
        }
    }
}
