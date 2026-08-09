using System;
using Microsoft.Xna.Framework;

namespace MiasmaColors;

public struct HSV
{
    public float H;
    public float S; 
    public float V;

    public HSV(float h, float s, float v)
    {
        H = h;
        S = s;
        V = v;
    }
}

public static class ColorUtils
{
    public static HSV ColorToHSV(Color color)
    {
        // Normalize RGB to [0,1]
        float r = color.R / 255f;
        float g = color.G / 255f;
        float b = color.B / 255f;

        float max = Math.Max(r, Math.Max(g, b));
        float min = Math.Min(r, Math.Min(g, b));
        float delta = max - min;

        float h = 0f;

        if (delta > 0f)
        {
            if (max == r)
            {
                h = 60f * (((g - b) / delta) % 6f);
            }
            else if (max == g)
            {
                h = 60f * (((b - r) / delta) + 2f);
            }
            else // max == b
            {
                h = 60f * (((r - g) / delta) + 4f);
            }
        }

        if (h < 0f) h += 360f;

        float s = (max == 0f) ? 0f : (delta / max);
        float v = max;

        return new HSV(h, s, v);
    }

    public static Color HSVToColor(HSV hsv, byte alpha = 255)
    {
        float h = hsv.H;
        float s = hsv.S;
        float v = hsv.V;

        float c = v * s; // Chroma
        float x = c * (1 - Math.Abs((h / 60f) % 2 - 1));
        float m = v - c;

        float r1 = 0f, g1 = 0f, b1 = 0f;

        if (h < 60)
        {
            r1 = c;
            g1 = x;
            b1 = 0;
        }
        else if (h < 120)
        {
            r1 = x;
            g1 = c;
            b1 = 0;
        }
        else if (h < 180)
        {
            r1 = 0;
            g1 = c;
            b1 = x;
        }
        else if (h < 240)
        {
            r1 = 0;
            g1 = x;
            b1 = c;
        }
        else if (h < 300)
        {
            r1 = x;
            g1 = 0;
            b1 = c;
        }
        else
        {
            r1 = c;
            g1 = 0;
            b1 = x;
        }

        byte r = (byte)((r1 + m) * 255f);
        byte g = (byte)((g1 + m) * 255f);
        byte b = (byte)((b1 + m) * 255f);

        return new Color(r, g, b, alpha);
    }

    public static void HueShift(ref HSV hsv, float amount)
    {
        hsv.H += amount;
        
        // Wrap around 0-360
        if (hsv.H < 0f)
            hsv.H += 360f;
        else if (hsv.H >= 360f)
            hsv.H -= 360f;
    }

    public static Color HueShift(Color color, float amount)
    {
        HSV hsv = ColorToHSV(color);
        HueShift(ref hsv, amount);
        return HSVToColor(hsv, color.A);
    }
}