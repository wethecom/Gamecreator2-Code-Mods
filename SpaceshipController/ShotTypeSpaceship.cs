// ShotTypeSpaceship.cs
using System;
using GameCreator.Runtime.Cameras;
using GameCreator.Runtime.Common;
using UnityEngine;

[Title("Spaceship")]
[Category("Spaceship")]
[Image(typeof(IconCamera), ColorTheme.Type.Blue)]
[Description("Camera that follows and moves like a spaceship")]
[Serializable]
public class ShotTypeSpaceship : TShotType
{
    // EXPOSED MEMBERS
    [SerializeField] private ShotSystemSpaceship m_ShotSystemSpaceship;

    // MEMBERS
    [NonSerialized] private readonly Transform[] m_Ignore = Array.Empty<Transform>();

    // PROPERTIES
    public override Transform[] Ignore => m_Ignore;
    public override bool HasTarget => false;
    public override Vector3 Target => this.Transform.position;

    public override Args Args
    {
        get
        {
            this.m_Args ??= new Args(this.m_ShotCamera, null);
            return this.m_Args;
        }
    }

    // CONSTRUCTOR
    public ShotTypeSpaceship()
    {
        this.m_ShotSystemSpaceship = new ShotSystemSpaceship();
        this.m_ShotSystems.Add(this.m_ShotSystemSpaceship.Id, this.m_ShotSystemSpaceship);
    }

    // OVERRIDE METHODS
    protected override void OnBeforeAwake(ShotCamera shotCamera)
    {
        base.OnBeforeAwake(shotCamera);
        this.m_ShotSystemSpaceship?.OnAwake(this);
    }

    protected override void OnBeforeUpdate()
    {
        base.OnBeforeUpdate();
        this.m_ShotSystemSpaceship?.OnUpdate(this);
    }
}