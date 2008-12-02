﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Diagnostics;

namespace QuickGraph.Collections
{
    /// <summary>
    /// An equatable pair of vertices
    /// </summary>
    /// <typeparam name="TVertex"></typeparam>
    [Serializable]
    [DebuggerDisplay("{Source} -> {Target}")]
    public struct VertexPair<TVertex>
        : IEquatable<VertexPair<TVertex>>
    {
        public readonly TVertex Source;
        public readonly TVertex Target;

        public static VertexPair<TVertex> FromEdge<TEdge>(TEdge edge)
            where TEdge : IEdge<TVertex>
        {
            Contract.Requires(edge != null);

            return new VertexPair<TVertex>(edge.Source, edge.Target);
        }

        public VertexPair(TVertex source, TVertex target)
        {
            Contract.Requires(source != null);
            Contract.Requires(target != null);

            this.Source = source;
            this.Target = target;
        }

        public override int GetHashCode()
        {
            return HashCodeHelper.Combine(
                this.Source.GetHashCode(),
                this.Target.GetHashCode()
                );
        }

        public bool Equals(VertexPair<TVertex> other)
        {
            return
                other.Source.Equals(this.Source) &&
                other.Target.Equals(this.Target);
        }

        public override bool Equals(object obj)
        {
            return
                obj is VertexPair<TVertex> &&
                base.Equals((VertexPair<TVertex>)obj);
        }

        public override string ToString()
        {
            return this.Source.ToString() + " -> " + this.Target.ToString();
        }
    }
}