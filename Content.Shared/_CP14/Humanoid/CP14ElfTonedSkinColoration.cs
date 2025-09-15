using System.Numerics;
using Content.Shared.Humanoid;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Humanoid;

/// <summary>
/// Unary coloration strategy that returns human skin tones, with 0 being lightest and 100 being darkest
/// </summary>
[DataDefinition]
[Serializable, NetSerializable]
public sealed partial class CP14ElfTonedSkinColoration : ISkinColorationStrategy
{
    [DataField]
    public Color ValidElfSkinTone = Color.FromHsv(new Vector4(0.07f, 0.05f, 1f, 1f));

    public SkinColorationStrategyInput InputType => SkinColorationStrategyInput.Unary;

    public bool VerifySkinColor(Color color)
    {
        var hsv = Color.ToHsv(color);
        var hue = Math.Round(hsv.X * 360f);
        var sat = Math.Round(hsv.Y * 100f);
        var val = Math.Round(hsv.Z * 100f);

        if (hue < 20f || hue > 270f)
            return false;

        if (sat < 5f || sat > 35f)
            return false;

        if (val < 20f || val > 100f)
            return false;

        return true;
    }

    public Color ClosestSkinColor(Color color)
    {
        return ValidElfSkinTone;
    }

    public Color FromUnary(float t)
    {
        var tone = Math.Clamp(t, 0f, 100f);

        var startSat = 5f;
        var startVal = 100f;

        var endSat = 30f;
        var endVal = 25f;

        var hue = 260f;
        var sat = MathHelper.Lerp(startSat, endSat, tone / 100f);
        var val = MathHelper.Lerp(startVal, endVal, tone / 100f);

        return Color.FromHsv(new Vector4(hue / 360f, sat / 100f, val / 100f, 1.0f));
    }

    public float ToUnary(Color color)
    {
        var hsv = Color.ToHsv(color);

        // Вычисляем относительное положение между светлой и тёмной точкой
        var hue = hsv.X * 360f;
        var sat = hsv.Y * 100f;
        var val = hsv.Z * 100f;

        // Нормируем по value, потому что основной градиент идёт по яркости
        var progressVal = (100f - val) / (100f - 25f);

        // Можно слегка учитывать hue/sat, но value остаётся главным драйвером
        return Math.Clamp(progressVal * 100f, 0f, 100f);
    }
}
