using System;
using Unity.Entities;

//旋转组件
[Serializable]
public struct RotationSpeed_IJobChunk : IComponentData
{
    public float RadiansPerSecond;
}
