// ShotSystemSpaceship.cs
using System;
using GameCreator.Runtime.Cameras;
using GameCreator.Runtime.Common;
using UnityEngine;

[Serializable]
public class ShotSystemSpaceship : TShotSystem
{
    public static readonly int ID = nameof(ShotSystemSpaceship).GetHashCode();

    // EXPOSED MEMBERS
    [SerializeField] private Transform m_CameraTransform;

    // PROPERTIES
    public override int Id => ID;

    // IMPLEMENTS
    public override void OnUpdate(TShotType shotType)
    {
        base.OnUpdate(shotType);
        if (this.m_CameraTransform == null) return;

        shotType.Position = this.m_CameraTransform.position;
        shotType.Rotation = this.m_CameraTransform.rotation;
    }

    // GIZMOS
    public override void OnDrawGizmosSelected(TShotType shotType, Transform transform)
    {
        base.OnDrawGizmosSelected(shotType, transform);
        Gizmos.color = GIZMOS_COLOR_ACTIVE;
        if (this.m_CameraTransform != null)
        {
            Gizmos.DrawWireSphere(this.m_CameraTransform.position, 0.5f);
        }
    }
}