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



        public GroundEffectType EffectType { get; set; }


        public Sprite Sprite { get; private set; }

        public static GroundEffect NoEffect = new GroundEffect { Duration = 0, Power = 0, EffectType = GroundEffectType.None, Sprite = SpriteManager.EmptyMarker };

        public static GroundEffect StandardAcid { get { return new GroundEffect { Duration = 10, Power = 1.0, EffectType = GroundEffectType.Acid, Sprite = SpriteManager.Acid }; } }
    }

    public enum GroundEffectType { Acid, None }
}