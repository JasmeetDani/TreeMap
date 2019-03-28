using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorIntervals
{
    public class ColorRange
    {
        public Color Color1 { get; set; }

        public Color Color2 { get; set; }
    }


    public class ColorInterval
    {
        public FloatRange range { get; set; }

        public ColorRange colorRange { get; set; }
    }


    private List<ColorInterval> colorLookup = new List<ColorInterval>();

    public List<ColorInterval> Intervals
    {
        get { return colorLookup; }

        private set { }
    }


    public void AddInterval(FloatRange r, int r1, int g1, int b1, int r2, int g2, int b2)
    {
        if (r.Start != r.End)
        {
            colorLookup.Add(
                new ColorInterval()
                {
                    range = r,
                    colorRange = new ColorRange()
                    {
                        Color1 = new Color(r1 / 255.0f, g1 / 255.0f, b1 / 255.0f),
                        Color2 = new Color(r2 / 255.0f, g2 / 255.0f, b2 / 255.0f)
                    }
                });
        }
        else
        {
            colorLookup.Add(
                new ColorInterval()
                {
                    range = r,
                    colorRange = new ColorRange()
                    {
                        Color1 = new Color(r1 / 255.0f, g1 / 255.0f, b1 / 255.0f),
                        Color2 = new Color(r1 / 255.0f, g1 / 255.0f, b1 / 255.0f)
                    }
                });
        }
    }

    public void AddInterval(FloatRange r, float r1, float g1, float b1, float r2, float g2, float b2)
    {
        if (r.Start != r.End)
        {
            colorLookup.Add(
                new ColorInterval()
                {
                    range = r,
                    colorRange = new ColorRange()
                    {
                        Color1 = new Color(r1, g1, b1),
                        Color2 = new Color(r2, g2, b2)
                    }
                });
        }
        else
        {
            colorLookup.Add(
                new ColorInterval()
                {
                    range = r,
                    colorRange = new ColorRange()
                    {
                        Color1 = new Color(r1, g1, b1),
                        Color2 = new Color(r1, g1, b1)
                    }
                });
        }
    }


    public void SetStartColor(int i, Color clr)
    {
        colorLookup[i].colorRange.Color1 = clr;
    }

    public void SetColorRange(int i, ColorRange clrs)
    {
        colorLookup[i].colorRange = clrs;
    }


    public Color Lookup(float i)
    {
        foreach (ColorInterval interval in colorLookup)
        {
            if (i >= interval.range.Start)
            {
                if (i < interval.range.End)
                {
                    return Color.LerpUnclamped(interval.colorRange.Color1,
                        interval.colorRange.Color2,
                        (i - interval.range.Start) / (interval.range.End - interval.range.Start));
                }
                else
                {
                    if ((i == interval.range.Start) && (i == interval.range.End))
                    {
                        return interval.colorRange.Color1;
                    }
                }
            }
        }

        return Color.white;
    }


    public IEnumerator GetEnumerator()
    {
        foreach (ColorInterval r in colorLookup)
        {
            yield return r;
        }
    }
}