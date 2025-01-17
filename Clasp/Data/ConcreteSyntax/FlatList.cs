﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Terms;

namespace Clasp.Data.ConcreteSyntax
{
    /// <summary>
    /// Represents a strongly-typed list that emulates the semantics of a linked list built out of <see cref="ConsCell"/> segments.
    /// </summary>
    //internal sealed class FlatList<T> : IEnumerable<T>
    //{
    //    public readonly T[] Values;
    //    public readonly T Final;

    //    public int LeadingCount => Values.Length;
    //    public int TotalCount => Values.Length + 1;
    //    public bool IsDotted => Final is not Nil;
    //    public bool IsEmpty => Final is Nil && Values.Length == 0;

    //    public FlatList(params T[] values)
    //    {
    //        Values = values[..^1];
    //        Final = values[^1];
    //    }

    //    public FlatList(T[] values, T final)
    //    {
    //        Values = values;
    //        Final = final;
    //    }

    //    public FlatList(IEnumerable<T> enumerated)
    //    {
    //        Values = enumerated.SkipLast(1).ToArray();
    //        Final = enumerated.Last();
    //    }

    //    public static FlatList<T>? FromNested(AstNode node)
    //    {
    //        AstNode target = node;
    //        List<T> cars = new List<T>();

    //        while (target is ConsCell cc)
    //        {
    //            if (cc.Car is T typedItem)
    //            {
    //                cars.Add(typedItem);
    //                target = cc.Cdr;
    //            }
    //            else
    //            {
    //                return null;
    //            }
    //        }

    //        if (target is T finalItem)
    //        {
    //            return new FlatList<T>(cars.ToArray(), finalItem);
    //        }
    //        else
    //        {
    //            return null;
    //        }
    //    }

    //    public T this[int i] { get => i == Values.Length ? Final : Values[i]; }

    //    public TArg[]? GetArgValues<TArg>()
    //        where TArg : AstNode
    //    {
    //        TArg?[] output = Array.ConvertAll(Values, x => x as TArg);

    //        return output.Any(x => x is null)
    //            ? null
    //            : output as TArg[];
    //    }

    //    public override string ToString()
    //    {
    //        return string.Format("FLAT({0}; {1})",
    //            string.Join(", ", Values),
    //            Final);
    //    }

    //    #region IEnumerable Implementation

    //    private IEnumerable<T> Enumerate()
    //    {
    //        for (int i = 0; i < Values.Length; ++i)
    //        {
    //            yield return Values[i];
    //        }
    //        yield return Final;
    //    }

    //    public IEnumerator<T> GetEnumerator() => Enumerate().GetEnumerator();

    //    IEnumerator IEnumerable.GetEnumerator() => Enumerate().GetEnumerator();

    //    #endregion
    //}
}
