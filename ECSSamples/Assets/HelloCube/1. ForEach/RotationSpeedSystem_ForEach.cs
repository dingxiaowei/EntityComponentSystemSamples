using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

//��ϵͳ�������еĴ���RotationSpeed_ForEach��Rotation�����ʵ��
public class RotationSpeedSystem_ForEach : JobComponentSystem
{
    //OnUpdate�������������߳���
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        float deltaTime = Time.DeltaTime;
        
        //ÿһ��jobHandle������һ��ʵ��Χ����up����ת
        var jobHandle = Entities
            .WithName("RotationSpeedSystem_ForEach")
            .ForEach((ref Rotation rotation, in RotationSpeed_ForEach rotationSpeed) =>
             {
                 rotation.Value = math.mul(
                     math.normalize(rotation.Value), 
                     quaternion.AxisAngle(math.up(), rotationSpeed.RadiansPerSecond * deltaTime));
             })
            .Schedule(inputDependencies);
    
        //������ҵ�����Ϊ��ϵͳ������
        return jobHandle;
    }
}
