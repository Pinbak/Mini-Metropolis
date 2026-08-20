using Agents;
using Buildings;
using UnityEngine;

/// <summary>
///     A helper class for getting the colours from positional data from the atlas.
/// </summary>
public class ColourSampler : MonoBehaviour
{
    [SerializeField] private Texture2D colourAtlas;
    [SerializeField] private Vector2Int residentialPixelAtlasPosition;
    [SerializeField] private Vector2Int commercialPixelAtlasPosition;
    [SerializeField] private Vector2Int industrialPixelAtlasPosition;
    [SerializeField] private Vector2Int specialBuildingsPixelAtlasPosition;
    [SerializeField] private Vector2Int invalidPixelAtlasPosition;
    [SerializeField] private Vector2Int roadPixelAtlasPosition;

    /// <summary>
    ///     Returns the colour from a given <see cref="BuildingType"/>. Uses predefined colour positions and the games atlas.
    /// </summary>
    public Color GetColourByBuildingType(BuildingType type)
    {
        var colour = type switch
        {
            BuildingType.Residential => colourAtlas.GetPixel(residentialPixelAtlasPosition.x,
                residentialPixelAtlasPosition.y),
            BuildingType.Commercial => colourAtlas.GetPixel(commercialPixelAtlasPosition.x,
                commercialPixelAtlasPosition.y),
            BuildingType.Industrial => colourAtlas.GetPixel(industrialPixelAtlasPosition.x,
                industrialPixelAtlasPosition.y),
            _ => colourAtlas.GetPixel(specialBuildingsPixelAtlasPosition.x,
                specialBuildingsPixelAtlasPosition.y),
        };
        return colour;
    }

    /// <summary>
    ///     Gets colour from a given <see cref="Need"/>.
    /// </summary>
    public Color GetColourByNeed(Need need)
    {
        var colour = need.Type switch
        {
            AgentType.Commuter => colourAtlas.GetPixel(industrialPixelAtlasPosition.x,
                industrialPixelAtlasPosition.y),
            AgentType.Fire => colourAtlas.GetPixel(specialBuildingsPixelAtlasPosition.x,
                specialBuildingsPixelAtlasPosition.y),
            AgentType.Police => colourAtlas.GetPixel(invalidPixelAtlasPosition.x,
                invalidPixelAtlasPosition.y),
            AgentType.Shopper => colourAtlas.GetPixel(commercialPixelAtlasPosition.x,
                commercialPixelAtlasPosition.y),
            AgentType.Student => colourAtlas.GetPixel(residentialPixelAtlasPosition.x,
                residentialPixelAtlasPosition.y),
            _ => colourAtlas.GetPixel(specialBuildingsPixelAtlasPosition.x,
                specialBuildingsPixelAtlasPosition.y),
        };
        return colour;
    }

    /// <summary>
    ///     Returns the colour that is considered invalid. In this case, black.
    /// </summary>
    public Color GetInvalidColour()
    {
        return colourAtlas.GetPixel(invalidPixelAtlasPosition.x, invalidPixelAtlasPosition.y);
    }

    /// <summary>
    ///     Returns the colour that is considered the road colour. In this case white.
    /// </summary>
    public Color GetRoadColour()
    {
        return colourAtlas.GetPixel(roadPixelAtlasPosition.x, roadPixelAtlasPosition.y);
    }
}