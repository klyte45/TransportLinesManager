

using Klyte.Commons.Utils;
using System;
using UnityEngine;

public struct CardinalPoint
{
    private static readonly string[] m_cardinal16 = new string[]
    {
        "N",
        "NNE",
        "NE",
        "ENE",
        "E",
        "ESE",
        "SE",
        "SSE",
        "S",
        "SSW",
        "SW",
        "WSW",
        "W",
        "WNW",
        "NW",
        "NNW",
    };

    public static string GetCardinalPoint16(float angle)
    {
        float diagSize = 22.5f;
        angle %= 360;
        angle += 360;
        angle %= 360;

        for (int i = 1; i < m_cardinal16.Length; i++)
        {
            if (Math.Abs(angle - diagSize * i) < diagSize / 2)
            {
                return m_cardinal16[i];
            }
        }
        return m_cardinal16[0];
    }



    public static CardinalPoint GetCardinalPoint(float angle, float diagSize = 45)
    {
        angle %= 360;
        angle += 360;
        angle %= 360;

        if (Math.Abs(angle - 45) < diagSize / 2)
        {
            return NE;
        }
        else if (Math.Abs(angle - 90) < diagSize / 2)
        {
            return E;
        }
        else if (Math.Abs(angle - 135) < diagSize / 2)
        {
            return SE;
        }
        else if (Math.Abs(angle - 180) < diagSize / 2)
        {
            return S;
        }
        else if (Math.Abs(angle - 225) < diagSize / 2)
        {
            return SW;
        }
        else if (Math.Abs(angle - 270) < diagSize / 2)
        {
            return W;
        }
        else if (Math.Abs(angle - 315) < diagSize / 2)
        {
            return NW;
        }
        else
        {
            return N;
        }
    }

    public static CardinalPoint GetCardinalPoint4(float angle, bool azimutal = false)
    {
        angle %= 360;
        if (azimutal)
        {
            angle += 630;
        }
        else
        {
            angle += 360;
        }
        angle %= 360;

        if (angle < 135f && angle >= 45f)
        {
            return E;
        }
        else if (angle < 45f || angle >= 315f)
        {
            return N;
        }
        else if (angle < 315f && angle >= 225f)
        {
            return W;
        }
        else
        {
            return S;
        }
    }

    private CardinalInternal InternalValue { get; set; }

    public CardinalInternal Value { get { return InternalValue; } }

    public static readonly CardinalPoint N = CardinalInternal.N;
    public static readonly CardinalPoint E = CardinalInternal.E;
    public static readonly CardinalPoint S = CardinalInternal.S;
    public static readonly CardinalPoint W = CardinalInternal.W;
    public static readonly CardinalPoint NE = CardinalInternal.NE;
    public static readonly CardinalPoint SE = CardinalInternal.SE;
    public static readonly CardinalPoint SW = CardinalInternal.SW;
    public static readonly CardinalPoint NW = CardinalInternal.NW;
    public static readonly CardinalPoint ZERO = CardinalInternal.ZERO;

    public static implicit operator CardinalPoint(CardinalInternal otherType)
    {
        return new CardinalPoint
        {
            InternalValue = otherType
        };
    }

    public static implicit operator CardinalInternal(CardinalPoint otherType)
    {
        return otherType.InternalValue;
    }

    public int StepsTo(CardinalPoint other)
    {
        if (other.InternalValue == InternalValue) return 0;
        if ((((int)other.InternalValue) & ((int)other.InternalValue - 1)) != 0 || (((int)InternalValue) & ((int)InternalValue - 1)) != 0) return int.MaxValue;
        CardinalPoint temp = other;
        int count = 0;
        while (temp.InternalValue != this.InternalValue)
        {
            temp++;
            count++;
        }
        if (count > 4) count -= 8;
        return count;
    }

    public static int operator -(CardinalPoint c, CardinalPoint other)
    {
        return c.StepsTo(other);
    }

    public Vector2 GetCardinalOffset()
    {
        switch (InternalValue)
        {
            case CardinalInternal.E:
                return new Vector2(1, 0);
            case CardinalInternal.W:
                return new Vector2(-1, 0);
            case CardinalInternal.N:
                return new Vector2(0, 1);
            case CardinalInternal.S:
                return new Vector2(0, -1);
            case CardinalInternal.NE:
                return new Vector2(1, 1);
            case CardinalInternal.NW:
                return new Vector2(-1, 1);
            case CardinalInternal.SE:
                return new Vector2(1, -1);
            case CardinalInternal.SW:
                return new Vector2(-1, -1);
        }
        return Vector2.zero;
    }


    public Vector2 GetCardinalOffset2D()
    {
        switch (InternalValue)
        {
            case CardinalInternal.E:
                return new Vector2(1, 0);
            case CardinalInternal.W:
                return new Vector2(-1, 0);
            case CardinalInternal.S:
                return new Vector2(0, 1);
            case CardinalInternal.N:
                return new Vector2(0, -1);
            case CardinalInternal.SE:
                return new Vector2(1, 1);
            case CardinalInternal.SW:
                return new Vector2(-1, 1);
            case CardinalInternal.NE:
                return new Vector2(1, -1);
            case CardinalInternal.NW:
                return new Vector2(-1, -1);
        }
        return Vector2.zero;
    }

    public int GetCardinalAngle()
    {
        switch (InternalValue)
        {
            case CardinalInternal.N:
                return 0;
            case CardinalInternal.S:
                return 180;
            case CardinalInternal.E:
                return 90;
            case CardinalInternal.W:
                return 270;
            case CardinalInternal.NE:
                return 45;
            case CardinalInternal.NW:
                return 315;
            case CardinalInternal.SE:
                return 135;
            case CardinalInternal.SW:
                return 225;
        }
        return 0;
    }
    public byte GetCardinalIndex8()
    {
        switch (InternalValue)
        {
            case CardinalInternal.N:
                return 0;
            case CardinalInternal.S:
                return 4;
            case CardinalInternal.E:
                return 2;
            case CardinalInternal.W:
                return 6;
            case CardinalInternal.NE:
                return 1;
            case CardinalInternal.NW:
                return 7;
            case CardinalInternal.SE:
                return 3;
            case CardinalInternal.SW:
                return 5;
        }
        return 8;
    }

    public static CardinalPoint operator ++(CardinalPoint c)
    {
        switch (c.InternalValue)
        {
            case CardinalInternal.N:
                return NE;
            case CardinalInternal.NE:
                return E;
            case CardinalInternal.E:
                return SE;
            case CardinalInternal.SE:
                return S;
            case CardinalInternal.S:
                return SW;
            case CardinalInternal.SW:
                return W;
            case CardinalInternal.W:
                return NW;
            case CardinalInternal.NW:
                return N;
            default:
                return ZERO;
        }
    }

    public static CardinalPoint operator --(CardinalPoint c)
    {
        switch (c.InternalValue)
        {
            case CardinalInternal.N:
                return NW;
            case CardinalInternal.NE:
                return N;
            case CardinalInternal.E:
                return NE;
            case CardinalInternal.SE:
                return E;
            case CardinalInternal.S:
                return SE;
            case CardinalInternal.SW:
                return S;
            case CardinalInternal.W:
                return SW;
            case CardinalInternal.NW:
                return W;
            default:
                return ZERO;
        }
    }

    public static CardinalPoint operator &(CardinalPoint c1, CardinalPoint c2)
    {
        return new CardinalPoint
        {
            InternalValue = c1.InternalValue & c2.InternalValue
        };
    }

    public static CardinalPoint operator |(CardinalPoint c1, CardinalPoint c2)
    {
        return new CardinalPoint
        {
            InternalValue = c1.InternalValue | c2.InternalValue
        };
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object o)
    {

        return o.GetType() == GetType() && this == ((CardinalPoint)o);
    }

    public static bool operator ==(CardinalPoint c1, CardinalPoint c2)
    {
        return c1.InternalValue == c2.InternalValue;
    }

    public static bool operator <(CardinalPoint left, CardinalPoint right)
    {
        return (Compare(left, right) < 0);
    }

    public static bool operator >(CardinalPoint left, CardinalPoint right)
    {
        return (Compare(left, right) > 0);
    }

    public int CompareTo(CardinalPoint other)
    {
        if (this == other) return 0;
        var a = GetCardinalAngle();
        var b = other.GetCardinalAngle() + 360;
        if (b - a > 180)
        {
            return -1;
        }
        else
        {
            return 1;
        }
    }

    public static int Compare(CardinalPoint left, CardinalPoint right)
    {
        if (object.ReferenceEquals(left, right))
        {
            return 0;
        }
        if (object.ReferenceEquals(left, default(CardinalPoint)))
        {
            return -1;
        }
        return left.CompareTo(right);
    }


    public static bool operator !=(CardinalPoint c1, CardinalPoint c2)
    {
        return c1.InternalValue != c2.InternalValue;
    }

    public static CardinalPoint operator ~(CardinalPoint c)
    {
        switch (c.InternalValue)
        {
            case CardinalInternal.N:
                return S;
            case CardinalInternal.NE:
                return SW;
            case CardinalInternal.E:
                return W;
            case CardinalInternal.SE:
                return NW;
            case CardinalInternal.S:
                return N;
            case CardinalInternal.SW:
                return NE;
            case CardinalInternal.W:
                return E;
            case CardinalInternal.NW:
                return SE;
            default:
                return ZERO;
        };
    }

    public enum CardinalInternal
    {
        N = 1,
        NE = 2,
        E = 4,
        SE = 8,
        S = 0x10,
        SW = 0x20,
        W = 0x40,
        NW = 0x80,
        ZERO = 0
    }

    public Vector2 GetPointForAngle(Vector2 p1, float distance)
    {
        return p1 + this.GetCardinalOffset() * distance;
    }


    public override string ToString()
    {
        return InternalValue.ToString();
    }

    public static CardinalPoint GetCardinal2D(Vector2 p1, Vector2 p2)
    {
        Vector2 p1Inv = new Vector2(p1.x, -p1.y);
        Vector2 p2Inv = new Vector2(p2.x, -p2.y);
        return GetCardinalPoint((p1Inv).GetAngleToPoint(p2Inv));
    }

    public static CardinalPoint GetCardinal2D4(Vector2 p1, Vector2 p2)
    {
        Vector2 p1Inv = new Vector2(p1.x, -p1.y);
        Vector2 p2Inv = new Vector2(p2.x, -p2.y);
        return GetCardinalPoint4((p1Inv).GetAngleToPoint(p2Inv));
    }

}