// ShotSystemSpaceshipDrawer.cs
using UnityEditor;

namespace GameCreator.Editor.Cameras
{
    [CustomPropertyDrawer(typeof(ShotSystemSpaceship))]
    public class ShotSystemSpaceshipDrawer : TShotSystemDrawer
    {
        protected override string Name(SerializedProperty property) => "Spaceship";
    }
}