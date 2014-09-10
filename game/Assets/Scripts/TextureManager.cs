using Assets.Scripts.LogicBase;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TextureManager
{
    #region fields

    private Dictionary<string, Texture2D> m_knownEquipmentTextures;
    private Texture2D m_uiBackground;

    #endregion fields

    #region constructor

    public TextureManager()
    {
        var textures = Resources.LoadAll<Texture2D>("Sprites/equipment");
        m_knownEquipmentTextures = textures.ToDictionary(texture => texture.name,
                                                        texture => texture);
        m_uiBackground = Resources.Load<Texture2D>(@"Sprites/PlayerStateDisplay");
    }

    #endregion constructor

    #region public methods

    public Texture2D GetTexture(PlayerEquipment equipment)
    {
        return m_knownEquipmentTextures[equipment.Name];
    }

    public Texture2D GetUIBackground()
    {
        return m_uiBackground;
    }

    #endregion public methods
}