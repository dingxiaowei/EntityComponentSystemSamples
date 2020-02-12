using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;


public class RotationSpeedSystem_IJobChunk : JobComponentSystem
{
    //实体查询类
    EntityQuery m_Group;

    protected override void OnCreate()
    {
        //查询具有Rotation和RotationSpeed_IJobChunk的组件的集合
        m_Group = GetEntityQuery(typeof(Rotation), ComponentType.ReadOnly<RotationSpeed_IJobChunk>());
    }

    //用于遍历包含多个匹配实体的连续内存块，比IJobForEach需要更多的代码设置，但其访问过程中可以对数据进行最直接的访问，可以直接修改其实际存储数据。
    [BurstCompile]
    struct RotationSpeedJob : IJobChunk
    {
        public float DeltaTime;
        public ArchetypeChunkComponentType<Rotation> RotationType;
        [ReadOnly]
        public ArchetypeChunkComponentType<RotationSpeed_IJobChunk> RotationSpeedType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            //获取组件块，类型是NativeArray
            var chunkRotations = chunk.GetNativeArray(RotationType);
            var chunkRotationSpeeds = chunk.GetNativeArray(RotationSpeedType);

            //遍历所有的组件快然后通过计算修改Rotation的Value值
            for (var i = 0; i < chunk.Count; i++)
            {
                var rotation = chunkRotations[i];
                var rotationSpeed = chunkRotationSpeeds[i];
                chunkRotations[i] = new Rotation
                {
                    Value = math.mul(math.normalize(rotation.Value),
                        quaternion.AxisAngle(math.up(), rotationSpeed.RadiansPerSecond * DeltaTime))
                };
            }
        }
    }

    //同样这个OnUpdate方法还是在主线程执行的
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var rotationType = GetArchetypeChunkComponentType<Rotation>(); //可读写组件块
        var rotationSpeedType = GetArchetypeChunkComponentType<RotationSpeed_IJobChunk>(true); //参数表示是否只读，默认是false，这个这个RotationSpeed_IJobChunk组件只是数据配置组件，不需要修改，所以参数为只读 true

        //实例化出JobChunk对象
        var job = new RotationSpeedJob()
        {
            RotationType = rotationType,
            RotationSpeedType = rotationSpeedType,
            DeltaTime = Time.DeltaTime
        };
        //将job提上日程
        return job.Schedule(m_Group, inputDependencies);
    }
}
