using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


/// <summary>
/// 这个是挂在Prefab上数据参数,里面实例化RotationSpeed_IJobChunk组件
/// </summary>
//需要有EntityConversion组件
[RequiresEntityConversion]
//在Component菜单栏下添加组件快捷键，会将这个组件以Rotation Speed的命名挂在GameObject上
[AddComponentMenu("DOTS Samples/IJobChunk/Rotation Speed")]
[ConverterVersion("joe", 1)]
public class RotationSpeedAuthoring_IJobChunk : MonoBehaviour, IConvertGameObjectToEntity
{
    public float DegreesPerSecond = 360.0F;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        //将Inspector上的配置的数据实例化一个数据组件然后添加在Entity实体上
        var data = new RotationSpeed_IJobChunk { RadiansPerSecond = math.radians(DegreesPerSecond) };
        dstManager.AddComponentData(entity, data);
    }
}
