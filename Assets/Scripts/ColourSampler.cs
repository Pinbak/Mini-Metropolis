using Buildings;
using UnityEngine;

public class ColourSampler : MonoBehaviour
{
    [SerializeField] private Texture2D colourAtlas;
    [SerializeField] private Vector2Int residentialPixelAtlasPosition;
    [SerializeField] private Vector2Int commercialPixelAtlasPosition;
    [SerializeField] private Vector2Int industrialPixelAtlasPosition;
    [SerializeField] private Vector2Int specialBuildingsPixelAtlasPosition;
    [SerializeField] private Vector2Int invalidPixelAtlasPosition;
    [SerializeField] private Vector2Int roadPixelAtlasPosition;

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
            _ => Color.red
        };
        return colour;
    }

    public Color GetInvalidColour()
    {
        return colourAtlas.GetPixel(invalidPixelAtlasPosition.x, invalidPixelAtlasPosition.y);
    }
    
    public Color GetRoadColour()
    {
        return colourAtlas.GetPixel(roadPixelAtlasPosition.x, roadPixelAtlasPosition.y);
    }
}