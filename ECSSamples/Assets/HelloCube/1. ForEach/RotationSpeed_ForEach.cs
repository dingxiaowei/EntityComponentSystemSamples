using System;
using Unity.Entities;

// 该组件可以挂在GameObject上
[GenerateAuthoringComponent]
public struct RotationSpeed_ForEach : IComponentData
{
    public float RadiansPerSecond;
}
