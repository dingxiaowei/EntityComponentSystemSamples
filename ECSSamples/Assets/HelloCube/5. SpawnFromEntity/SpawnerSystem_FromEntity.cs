using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// 任务组件系统（JobComponentSystems）可以在工作线程上运行，但是创建和移除实体只能在主线程上做，从而防止线程之间的竞争
/// Jobs系统使用一个实体命令缓存（EntityCommandBuffer）来延迟那些不能在任务系统内完成的任务。
/// </summary>
[UpdateInGroup(typeof(SimulationSystemGroup))] //标记更新组为模拟系统组
public class SpawnerSystem_FromEntity : JobComponentSystem
{
    /// <summary>
    /// 开始初始化实体命令缓存系统（BeginInitializationEntityCommandBufferSystem）被用来创建一个命令缓存，
    /// 这个命令缓存将在阻塞系统执行时被回放。虽然初始化命令在生成任务（SpawnJob）中被记录下来，
    /// 它并非真正地被执行（或“回放”）直到相应的实体命令缓存系统（EntityCommandBufferSystem）被更新。
    /// 为了确保transform系统有机会在新生的实体初次被渲染之前运行，SpawnerSystem_FromEntity将使用
    /// 开始模拟实体命令缓存系统（BeginSimulationEntityCommandBufferSystem）来回放其命令。
    /// 这就导致了在记录命令和初始化实体之间一帧的延迟，但是该延迟实际通常被忽略掉。
    /// </summary>
    BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;

    protected override void OnCreate()
    {
        //在开始的时候缓存这个对象，就不需要每帧去创建调用
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        //取代直接执行结构的改变，一个任务可以添加一个命令到EntityCommandBuffer（实体命令缓存），从而在主线程上完成其任务后执行这些改变
        //命令缓存允许在工作线程上执行任何潜在消耗大的计算，同时把实际的增删排到之后
        var commandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();

        //这个job将实例化命令添加到EntityCommandBuffer
        //由于这个job仅在第一帧上运行，因此我们要确保Burst在运行前对其进行编译以获得最佳性能(WithBurst的第三个参数)
        //job在编译后将被缓存（Burst仅编译一次）
        var jobHandle = Entities
            .WithName("SpawnerSystem_FromEntity")
            .WithBurst(FloatMode.Default, FloatPrecision.Standard, true) //通过Burst编译，提高效率
            .ForEach((Entity entity, int entityInQueryIndex, in Spawner_FromEntity spawnerFromEntity, in LocalToWorld location) =>
        {
            for (var x = 0; x < spawnerFromEntity.CountX; x++)
            {
                for (var y = 0; y < spawnerFromEntity.CountY; y++)
                {
                    var instance = commandBuffer.Instantiate(entityInQueryIndex, spawnerFromEntity.Prefab);
                    var position = math.transform(location.Value,
                        new float3(x * 1.3F, noise.cnoise(new float2(x, y) * 0.21F) * 2, y * 1.3F));
                    commandBuffer.SetComponent(entityInQueryIndex, instance, new Translation {Value = position});
                }
            }

            commandBuffer.DestroyEntity(entityInQueryIndex, entity);
        }).Schedule(inputDeps);

        ///生成任务并行且没有同步机会直到阻塞系统执行
        ///当阻塞系统执行时，我们想完成生成任务，然后再执行那些命令（创建实体并放置到指定位置）
        /// 我们需要告诉阻塞系统哪个任务需要在它能回放命令之前完成
        m_EntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }
}
