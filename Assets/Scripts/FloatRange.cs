using System;

public struct FloatRange
{
    public float Start { get; set; }

    public float End { get; set; }


    public override bool Equals(Object obj)
    {
        FloatRange r = (FloatRange)obj;

        return Start.Equals(r.Start) && End.Equals(r.End);
    }

    public override int GetHashCode()
    {
        return Start.GetHashCode();
    }
}