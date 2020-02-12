using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
[AddComponentMenu("DOTS Samples/SpawnFromEntity/Spawner")]
[ConverterVersion("joe", 1)]
public class SpawnerAuthoring_FromEntity : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    public GameObject Prefab;
    public int CountX;
    public int CountY;

    //IDeclareReferencedPrefabs接口的实现，声明引用的预设，好让系统提前知道他们的存在
    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(Prefab); //将我们要实例化出来的prefab添加进referencedPrefabs
    }

    /// <summary>
    /// 我们将编辑器的数据表述转化成实体最佳的运行时表述
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="dstManager">目标实体管理器</param>
    /// <param name="conversionSystem">转化系统</param>
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var spawnerData = new Spawner_FromEntity
        {
            Prefab = conversionSystem.GetPrimaryEntity(Prefab), //将Prefab转成实体Entity
            CountX = CountX,
            CountY = CountY
        };
        dstManager.AddComponentData(entity, spawnerData);
    }
}
