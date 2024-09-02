
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

[Serializable]
public struct Fixed64 : IEquatable<Fixed64>, IComparable, IComparable<Fixed64>
{
    public long value;
    /// <summary>
    /// С�����ֵ�λ��
    /// </summary>
    private const int FRACTIONALBITS = 12;
    private const long ONE = 1L << FRACTIONALBITS;
    public static Fixed64 Zero = new Fixed64(0);
    /// <summary>
    /// ֱ�Ӷ�value���и�ֵ
    /// </summary>
    /// <param name="value"></param>
    Fixed64(long value)
    {
        this.value = value;
    }
    /// <summary>
    /// ����������ֵĹ��캯��
    /// </summary>
    /// <param name="value"></param>
    public Fixed64(int value)
    {
        this.value = value * ONE;
    }

    public static Fixed64 operator +(Fixed64 a, Fixed64 b)
    {
        return new Fixed64(a.value + b.value);
    }
    public static Fixed64 operator -(Fixed64 a, Fixed64 b)
    {
        return new Fixed64(a.value - b.value);
    }
    public static Fixed64 operator *(Fixed64 a, Fixed64 b)
    {
        return new Fixed64((a.value * b.value) >> FRACTIONALBITS);
    }
    public static Fixed64 operator /(Fixed64 a, Fixed64 b)
    {
        return new Fixed64((a.value << FRACTIONALBITS) / b.value);
    }
    public static bool operator ==(Fixed64 a, Fixed64 b)
    {
        return a.value == b.value;
    }
    public static bool operator !=(Fixed64 a, Fixed64 b)
    {
        return !(a == b);
    }
    public static bool operator >(Fixed64 a, Fixed64 b)
    {
        return a.value > b.value;
    }
    public static bool operator <(Fixed64 a, Fixed64 b)
    {
        return a.value < b.value;
    }
    public static bool operator >=(Fixed64 a, Fixed64 b)
    {
        return a.value >= b.value;
    }
    public static bool operator <=(Fixed64 a, Fixed64 b)
    {
        return a.value <= b.value;
    }

    public static explicit operator long(Fixed64 value)
    {
        return value.value >> FRACTIONALBITS;
    }
    public static explicit operator Fixed64(long value)
    {
        return new Fixed64(value);
    }
    public static explicit operator float(Fixed64 value)
    {
        return (float)value.value / ONE;
    }
    public static explicit operator Fixed64(float value)
    {
        return new Fixed64((long)(value * ONE));
    }
    public int CompareTo(object obj)
    {
        return value.CompareTo(obj);
    }

    public int CompareTo(Fixed64 other)
    {
        return value.CompareTo(other.value);
    }

    public bool Equals(Fixed64 other)
    {
        return value == other.value;
    }
    public override int GetHashCode()
    {
        return value.GetHashCode();
    }
    public override bool Equals(object obj)
    {
        return obj is Fixed64 && ((Fixed64)obj).value == value;
    }
    public override string ToString()
    {
        return ((float)this).ToString();
    }
}
