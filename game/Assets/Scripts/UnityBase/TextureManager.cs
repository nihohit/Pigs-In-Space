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

        private Dictionary<string, Texture2D> m_knownEquipmentTextures;
        private Dictionary<string, Texture2D> m_knownShotTextures;
        private Texture2D m_uiBackground;

        #endregion fields

        #region constructor

        public TextureManager()
        {
            var textures = Resources.LoadAll<Texture2D>("Sprites/equipment");
            m_knownEquipmentTextures = textures.ToDictionary(texture => texture.name,
                                                            texture => texture);
            textures = Resources.LoadAll<Texture2D>("Sprites/shots");
            m_knownShotTextures = textures.ToDictionary(texture => texture.name,
                                                            texture => texture);
            m_uiBackground = Resources.Load<Texture2D>(@"Sprites/PlayerStateDisplay");
        }

        #endregion constructor

        #region public methods

        public Texture2D GetTexture(PlayerEquipment equipment)
        {
            return m_knownEquipmentTextures.Get(equipment.Name, "Equipment textures");
        }

        public void ReplaceTexture(ShotScript shot, string shotType)
        {
            var texture = m_knownShotTextures.Get(shotType, "Shot textures");
            ReplaceTexture(shot.GetComponent<SpriteRenderer>(), texture, shotType);
        }

        public Texture2D GetUIBackground()
        {
            return m_uiBackground;
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