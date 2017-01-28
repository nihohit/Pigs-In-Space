using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using Assets.Scripts.MapScene;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.UnityBase
{
    public class TextureManager
    {
        #region fields

        private readonly Dictionary<string, Sprite> r_knownEquipmentTextures;
        private readonly Dictionary<string, Sprite> r_knownShotTextures;

        #endregion fields

        #region constructor

        public TextureManager()
        {
            var textures = Resources.LoadAll<Sprite>("Sprites/equipment");
            this.r_knownEquipmentTextures = textures.ToDictionary(texture => texture.name,
                                                            texture => texture);
            textures = Resources.LoadAll<Sprite>("Sprites/shots");
            this.r_knownShotTextures = textures.ToDictionary(texture => texture.name,
                                                            texture => texture);
        }

        #endregion constructor

        #region public methods

        public Sprite GetTexture(PlayerEquipment equipment)
        {
            return this.r_knownEquipmentTextures.Get(equipment.Name, "Equipment textures");
        }

        public void ReplaceTexture(ShotScript shot, string shotType)
        {
            var sprite = this.r_knownShotTextures.Get(shotType, "Shot textures");
            shot.GetComponent<SpriteRenderer>().sprite = sprite;
            shot.name = shotType;
        }

        #endregion public methods

        #region private methods

        private void ReplaceTexture(SpriteRenderer renderer, Texture2D newTexture, string name)
        {
            renderer.sprite = Sprite.Create(newTexture, renderer.sprite.rect, new Vector2(0.5f, 0.5f));
            renderer.sprite.name = name;
        }

        #endregion private methods
    }
}