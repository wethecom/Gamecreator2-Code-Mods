using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;

[Title("Spaceship Controller")]
[Image(typeof(IconCharacter), ColorTheme.Type.Green)]
[Category("Car Controller")]
[Description("Configures the character to behave like a Car")]

[Serializable]
public class KernelPresetCar : IKernelPreset
{
    public TUnitPlayer MakePlayer => new UnitPlayerDirectional(); // Reuse directional input
    public TUnitMotion MakeMotion => new UnitMotionCar();
    public TUnitDriver MakeDriver => new UnitDriverCar();
    public TUnitFacing MakeFacing => new UnitFacingCar();
    public TUnitAnimim MakeAnimim => new UnitAnimimKinematic(); // Reuse existing animation system
}