using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

//该系统更新所有的带有RotationSpeed_ForEach和Rotation组件的实体
public class RotationSpeedSystem_ForEach : JobComponentSystem
{
    //OnUpdate方法运行在主线程上
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        float deltaTime = Time.DeltaTime;
        
        //每一个jobHandle都是让一个实体围绕着up轴旋转
        var jobHandle = Entities
            .WithName("RotationSpeedSystem_ForEach")
            .ForEach((ref Rotation rotation, in RotationSpeed_ForEach rotationSpeed) =>
             {
                 rotation.Value = math.mul(
                     math.normalize(rotation.Value), 
                     quaternion.AxisAngle(math.up(), rotationSpeed.RadiansPerSecond * deltaTime));
             })
            .Schedule(inputDependencies);
    
        //返回作业句柄作为此系统的依赖
        return jobHandle;
    }
}
