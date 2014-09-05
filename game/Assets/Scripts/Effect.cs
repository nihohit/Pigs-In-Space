using System;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Currently effects only cause damage
    /// </summary>
    public class GroundEffect
    {
        public int Duration { get; set; }

        public double Power { get; private set; }

        public GroundEffectType Type { get; set; }

        public Sprite Sprite { get { return Type.EffectSprite; } }

        public static GroundEffect NoEffect = new GroundEffect { Duration = 0, Power = 0, Type = GroundEffectType.None };

        public static GroundEffect StandardAcid { get { return new GroundEffect { Duration = 10, Power = 2.0, Type = GroundEffectType.Acid }; } }
    }

    public class GroundEffectType
    {
        public Sprite EffectSprite { get; private set; }

        public static GroundEffectType Acid = new GroundEffectType { EffectSprite = SpriteManager.Acid };
        public static GroundEffectType None = new GroundEffectType { EffectSprite = SpriteManager.EmptyMarker };
    }
}