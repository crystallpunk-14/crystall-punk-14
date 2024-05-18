namespace Content.Shared._CP14.Humanoid;

public static class CP14SkinColor
{
    public const float MinTieflingHuesLightness = 10f / 100f;
    public const float MaxTieflingHuesLightness = 35f / 100f;

    public static Color TieflingHues(Color color)
    {
        return color;
    }

    public static Color MakeTieflingHueValid(Color color)
    {
        color.G = Math.Min(color.G, color.B);

        var hsl = Color.ToHsl(color);
        hsl.Z = Math.Clamp(hsl.Z, MinTieflingHuesLightness, MaxTieflingHuesLightness);

        return Color.FromHsl(hsl);
    }

    public static bool VerifyTieflingHues(Color color)
    {
        var hsl = Color.ToHsl(color);
        var lightness = hsl.Z;

        // Allows you to use shades of green, but not make green tieflings
        if (color.G > color.B)
            return false;

        // No black or white holes instead of tieflings
        if (lightness is < MinTieflingHuesLightness or > MaxTieflingHuesLightness)
            return false;

        return true;
    }
}
