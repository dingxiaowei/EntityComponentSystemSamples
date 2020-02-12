using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

/// <summary>
/// 生命周期，这里属于Component
/// </summary>
public struct LifeTime : IComponentData
{
    public float Value;
}

/// <summary>
/// 这个系统负责场景中所有实体的生命周期
/// 也可以将其改装来负责特定实体的生命周期，添加刷选条件Filter即可
/// </summary>
public class LifeTimeSystem : JobComponentSystem
{
    /// <summary>
    /// 实体命令缓存系统--阻塞
    /// </summary>
    EntityCommandBufferSystem m_Barrier;

    /// <summary>
    /// 将阻塞缓存起来
    /// </summary>
    protected override void OnCreate()
    {
        m_Barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    [BurstCompile]
    struct LifeTimeJob : IJobForEachWithEntity<LifeTime>
    {
        public float DeltaTime;

        [WriteOnly]
        public EntityCommandBuffer.Concurrent CommandBuffer;

        /// <summary>
        /// 每帧执行，如果寿命 < 0 则摧毁实体
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="jobIndex">任务索引</param>
        /// <param name="lifeTime">寿命</param>
        public void Execute(Entity entity, int jobIndex, ref LifeTime lifeTime)
        {
            lifeTime.Value -= DeltaTime;

            if (lifeTime.Value < 0.0f)
            {
                CommandBuffer.DestroyEntity(jobIndex, entity);
            }
        }
    }

    /// <summary>
    /// 在主线程上每帧运行OnUpdate
    /// </summary>
    /// <param name="inputDependencies">输入依赖</param>
    /// <returns>任务</returns>
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var commandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent();

        var job = new LifeTimeJob
        {
            DeltaTime = Time.DeltaTime,
            CommandBuffer = commandBuffer,

        }.Schedule(this, inputDependencies);

        m_Barrier.AddJobHandleForProducer(job);

        return job;
    }
}
