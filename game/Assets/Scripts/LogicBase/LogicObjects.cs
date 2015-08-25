using Assets.Scripts.MapScene;
using System;
using System.Collections;

namespace Assets.Scripts.LogicBase
{
    public enum MovementType { NonMoving = 0, Walking = 1, Flying = 2 }

    public enum EntityTactics { AlwaysAct = 0, ActInRange = 1 }

    public delegate void SquareEffect(SquareScript square);

    public delegate IEnumerator TimedSquareEffect(SquareScript square, float time);

    [Flags]
    public enum EffectTypes
    {
        DamageDealing = 1,
        RockBreaking = 2,
        Piercing = 4,
        CreateEntity = 8,
        CreateLocalEffect = 16,
        Push = 32,
        Heal = 64
    }
}