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

        if (sat < 5f || sat > 50f)
        return false;

        if (val < 20f || val > 100f)
            return false;

        return true;
    }

    public Color ClosestSkinColor(Color color)
    {
        return ValidElfSkinTone;
    }

    public Color FromUnary(float color)
    {
        var tone = Math.Clamp(color, 0f, 100f);

        if (color < 50f) //Human toned Elf
        {
            var rangeOffset = tone - 20f;

            var hue = 25f;
            var sat = 20f;
            var val = 100f;

            if (rangeOffset <= 0)
            {
                hue += Math.Abs(rangeOffset);
            }
            else
            {
                sat += rangeOffset;
                val -= rangeOffset;
            }

            return Color.FromHsv(new Vector4(hue / 360f, sat / 100f, val / 100f, 1.0f));
        }
        else //Dark elf
        {
            var startSat = 5f;
            var startVal = 100f;

            var endSat = 30f;
            var endVal = 25f;

            var hue = 260f;
            var sat = MathHelper.Lerp(startSat, endSat, tone / 100f);
            var val = MathHelper.Lerp(startVal, endVal, tone / 100f);

            return Color.FromHsv(new Vector4(hue / 360f, sat / 100f, val / 100f, 1.0f));
        }
    }

    public float ToUnary(Color color)
    {
        var hsv = Color.ToHsv(color);
        var hue = hsv.X * 360f;
        var sat = hsv.Y * 100f;
        var val = hsv.Z * 100f;
        if(hue>255)
        {
            var progressVal = (100f - val) / (100f - 25f);
            return Math.Clamp(progressVal * 100f, 0f, 100f);
        }else
        {
            // check for hue/value first, if hue is lower than this percentage
            // and value is 1.0
            // then it'll be hue
            if (Math.Clamp(hsv.X, 25f / 360f, 1) > 25f / 360f
                && hsv.Z == 1.0)
            {
                return Math.Abs(45 - hsv.X * 360);
            }
            // otherwise it'll directly be the saturation
            else
            {
                return hsv.Y * 100;
            }
        }


    }
}
