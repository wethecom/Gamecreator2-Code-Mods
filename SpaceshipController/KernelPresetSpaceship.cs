using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;

[Title("Spaceship Controller")]
[Image(typeof(IconCharacter), ColorTheme.Type.Green)]
[Category("Spaceship Controller")]
[Description("Configures the character to behave like a spaceship")]

[Serializable]
public class KernelPresetSpaceship : IKernelPreset
{
    public TUnitPlayer MakePlayer => new UnitPlayerDirectional(); // Reuse directional input
    public TUnitMotion MakeMotion => new UnitMotionSpaceship();
    public TUnitDriver MakeDriver => new UnitDriverSpaceship();
    public TUnitFacing MakeFacing => new UnitFacingSpaceship();
    public TUnitAnimim MakeAnimim => new UnitAnimimKinematic(); // Reuse existing animation system
}