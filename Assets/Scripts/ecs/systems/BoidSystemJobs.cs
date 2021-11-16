using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
namespace ColdShowerGames {

    public struct CopyAllPositions : IJob {
        [ReadOnly]
        public NativeArray<float3> elementsToAdd;
        public NativeElement<float3> result;

        public void Execute() {
            foreach (var t in elementsToAdd) {
                result.Value += t;
            }
        }
    }

    [BurstCompile]
    struct MergeCells : IJobNativeMultiHashMapMergedSharedKeyIndices {
        [ReadOnly] public NativeArray<float3> positions;
        [ReadOnly] public NativeArray<float3> headings;
        
        public NativeArray<int> perBoidCellIndex;
        public NativeArray<float3> perCellHeadings;
        public NativeArray<float3> perCellPositions;
        // public NativeArray<int> cellObstaclePositionIndex;
        // public NativeArray<float> cellObstacleDistance;
        // public NativeArray<int> cellTargetPositionIndex;
        public NativeArray<int> perCellCount;
        // [ReadOnly]
        // public NativeArray<float3> targetPositions;
        // [ReadOnly]
        // public NativeArray<float3> obstaclePositions;

        void NearestPosition(
            NativeArray<float3> targets,
            float3 position,
            out int nearestPositionIndex,
            out float nearestDistance) {
            
            nearestPositionIndex = 0;
            nearestDistance = math.lengthsq(position - targets[0]);
            for (int i = 1; i < targets.Length; i++) {
                var targetPosition = targets[i];
                var distance = math.lengthsq(position - targetPosition);
                var nearest = distance < nearestDistance;

                nearestDistance = math.select(nearestDistance, distance, nearest);
                nearestPositionIndex = math.select(nearestPositionIndex, i, nearest);
            }
            nearestDistance = math.sqrt(nearestDistance);
        }

        // Resolves the distance of the nearest obstacle and target and stores the cell index.
        public void ExecuteFirst(int index) {
            // var position = cellSeparation[index] / cellCount[index];

            // int obstaclePositionIndex;
            // float obstacleDistance;
            // NearestPosition(obstaclePositions, position, out obstaclePositionIndex, out obstacleDistance);
            // cellObstaclePositionIndex[index] = obstaclePositionIndex;
            // cellObstacleDistance[index] = obstacleDistance;
            //
            // int targetPositionIndex;
            // float targetDistance;
            // NearestPosition(targetPositions, position, out targetPositionIndex, out targetDistance);
            // cellTargetPositionIndex[index] = targetPositionIndex;

            perBoidCellIndex[index] = index;
            perCellCount[index] = 1;
            perCellHeadings[index] = headings[index];
            perCellPositions[index] = positions[index];
        }

        // Sums the alignment and separation of the actual index being considered and stores
        // the index of this first value where we're storing the cells.
        // note: these items are summed so that in `Steer` their average for the cell can be resolved.
        public void ExecuteNext(int cellIndex, int index) {
            perCellCount[cellIndex] += 1;
            perCellHeadings[cellIndex] += headings[index];
            perCellPositions[cellIndex] += positions[index];
            perBoidCellIndex[index] = cellIndex;
        }
    }

}
