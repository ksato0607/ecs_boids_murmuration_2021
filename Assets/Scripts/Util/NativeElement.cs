using System;
using Unity.Collections;
using Unity.Jobs;
namespace ColdShowerGames {
    public struct NativeElement<T> : IDisposable where T : struct {

        private NativeArray<T> _array;

        public T Value {
            get => _array[0]; set => _array[0] = value;
        }

        public NativeElement(T value, Allocator alloc) {

            _array = new NativeArray<T>(1, alloc, NativeArrayOptions.UninitializedMemory);
            Value = value;
        }
            

        public void Dispose(JobHandle inputDeps) {
            _array.Dispose(inputDeps);
        }
        public void Dispose() {
            _array.Dispose();
        }
    }
    
    public struct IntFloat {

        public IntFloat(int i1, float f1) {
            i = i1;
            f = f1;
        }

        public int i;
        public float f;
    }
}
