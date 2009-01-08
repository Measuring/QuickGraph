using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using QuickGraph.Contracts;

namespace QuickGraph
{
    [Serializable]
    [DebuggerDisplay("VertexCount = {VertexCount}, EdgeCount = {EdgeCount}")]
    public class BidirectionalMatrixGraph<TEdge> 
        : IBidirectionalGraph<int, TEdge>
        , IMutableEdgeListGraph<int, TEdge>
        , ICloneable
        where TEdge : IEdge<int>
    {
        private readonly int vertexCount;
        private int edgeCount;
        private readonly TEdge[,] edges;

        public BidirectionalMatrixGraph(int vertexCount)        
        {
            Contract.Requires(vertexCount > 0);

            this.vertexCount = vertexCount;
            this.edgeCount = 0;
            this.edges = new TEdge[vertexCount, vertexCount];
        }

        #region IGraph
        public bool AllowParallelEdges
        {
            get { return false; }
        }

        public bool IsDirected
        {
            get { return true; }
        }
        #endregion

        #region IVertexListGraph
        public int VertexCount
        {
            get { return this.vertexCount; }
        }

        public bool IsVerticesEmpty
        {
            get { return this.VertexCount == 0; }
        }
        #endregion

        #region IEdgeListGraph
        public int EdgeCount
        {
            get { return this.edgeCount; }
        }

        public bool IsEdgesEmpty
        {
            get { return this.EdgeCount == 0; }
        }

        public IEnumerable<TEdge> Edges
        {
            get
            {
                for (int i = 0; i < this.VertexCount; ++i)
                {
                    for (int j = 0; j < this.VertexCount; ++j)
                    {
                        TEdge e = this.edges[i, j];
                        if (e != null)
                            yield return e;
                    }
                }
            }
        }
        #endregion

        #region IBidirectionalGraph<int,Edge> Members
        [Pure]
        public bool IsInEdgesEmpty(int v)
        {
            Contract.Requires(0 <= v && v < this.VertexCount);

            for (int i = 0; i < this.VertexCount; ++i)
                if (this.edges[i, v] != null)
                    return false;
            return true;
        }

        [Pure]
        public int InDegree(int v)
        {
            Contract.Requires(0 <= v && v < this.VertexCount);

            int count = 0;
            for (int i = 0; i < this.VertexCount; ++i)
                if (this.edges[i, v] != null)
                    count++;
            return count;
        }

        [Pure]
        public IEnumerable<TEdge> InEdges(int v)
        {
            Contract.Requires(0 <= v && v < this.VertexCount);
            Contract.Ensures(Contract.Result<IEnumerable<TEdge>>() != null);

            for (int i = 0; i < this.VertexCount; ++i)
            {
                TEdge e = this.edges[i, v];
                if (e != null)
                    yield return e;
            }
        }

        [Pure]
        public bool TryGetInEdges(int v, out IEnumerable<TEdge> edges)
        {
            Contract.Ensures(Contract.Result<bool>() == (0 <= 0 && v > this.VertexCount));
            Contract.Ensures(
                Contract.Result<bool>() == 
                (Contract.ValueAtReturn<IEnumerable<TEdge>>(out edges) != null));

            if (v > -1 && v < this.vertexCount)
            {
                edges = this.InEdges(v);
                return true;
            }
            edges = null;
            return false;
        }

        public TEdge InEdge(int v, int index)
        {
            Contract.Requires(0 <= v && v < this.VertexCount);
            Contract.Requires(0 <= index && index < this.VertexCount);
            Contract.Ensures(Contract.Result<TEdge>() != null);

            int count = 0;
            for (int i = 0; i < this.VertexCount; ++i)
            {
                TEdge e = this.edges[i, v];
                if (e != null)
                {
                    if (count == index)
                        return e;
                    count++;
                }
            }
            throw new ArgumentOutOfRangeException("index");
        }

        public int Degree(int v)
        {
            Contract.Requires(0 <= v && v < this.VertexCount);
            Contract.Ensures(Contract.Result<int>() == this.InDegree(v) + this.OutDegree(v));

            return this.InDegree(v) + this.OutDegree(v);
        }

        #endregion

        #region IIncidenceGraph<int,Edge> Members

        public bool ContainsEdge(int source, int target)
        {
            Contract.Requires(0 <= source && source < this.VertexCount);
            Contract.Requires(0 <= target && target < this.VertexCount);

            return this.edges[source, target] != null;
        }

        public bool TryGetEdge(int source, int target, out TEdge edge)
        {
            Contract.Requires(0 <= source && source < this.VertexCount);
            Contract.Requires(0 <= target && target < this.VertexCount);

            edge = this.edges[source, target];
            return edge != null;
        }

        public bool TryGetEdges(int source, int target, out IEnumerable<TEdge> edges)
        {
            Contract.Requires(0 <= source && source < this.VertexCount);
            Contract.Requires(0 <= target && target < this.VertexCount);

            TEdge edge;
            if (this.TryGetEdge(source, target, out edge))
            {
                edges = new TEdge[] { edge };
                return true;
            }
            else
            {
                edges = null;
                return false;
            }
        }

        #endregion

        #region IImplicitGraph<int,Edge> Members

        [Pure]
        public bool IsOutEdgesEmpty(int v)
        {
            Contract.Requires(0 <= v && v < this.VertexCount);

            for (int j = 0; j < this.vertexCount; ++j)
                if (this.edges[v, j] != null)
                    return false;
            return true;
        }

        [Pure]
        public int OutDegree(int v)
        {
            Contract.Requires(0 <= v && v < this.VertexCount);

            int count = 0;
            for (int j = 0; j < this.vertexCount; ++j)
                if (this.edges[v, j] != null)
                    count++;
            return count;
        }

        [Pure]
        public IEnumerable<TEdge> OutEdges(int v)
        {
            Contract.Requires(0 <= v && v < this.VertexCount);

            for (int j = 0; j < this.vertexCount; ++j)
            {
                TEdge e = this.edges[v, j];
                if (e != null)
                    yield return e;
            }
        }

        [Pure]
        public bool TryGetOutEdges(int v, out IEnumerable<TEdge> edges)
        {
            Contract.Requires(0 <= v && v < this.VertexCount);

            if (v > -1 && v < this.vertexCount)
            {
                edges = this.OutEdges(v);
                return true;
            }
            edges = null;
            return false;
        }

        [Pure]
        public TEdge OutEdge(int v, int index)
        {
            Contract.Requires(0 <= v && v < this.VertexCount);

            int count = 0;
            for (int j = 0; j < this.vertexCount; ++j)
            {
                TEdge e = this.edges[v, j];
                if (e != null)
                {
                    if (count==index)
                        return e;
                    count++;
                }
            }
            throw new ArgumentOutOfRangeException("index");
        }

        #endregion

        #region IVertexSet<int,Edge> Members

        public IEnumerable<int> Vertices
        {
            get 
            {
                for (int i = 0; i < this.VertexCount; ++i)
                    yield return i;
            }
        }

        [Pure]
        public bool ContainsVertex(int vertex)
        {
            Contract.Requires(0 <= vertex && vertex < this.VertexCount);

            return vertex >= 0 && vertex < this.VertexCount;
        }

        #endregion

        #region IEdgeListGraph<int,Edge> Members
        [Pure]
        public bool ContainsEdge(TEdge edge)
        {
            TEdge e = this.edges[edge.Source, edge.Target];
            return e!=null && 
                e.Equals(edge);
        }

        #endregion

        #region IMutableBidirectionalGraph<int,Edge> Members

        public int RemoveInEdgeIf(int v, EdgePredicate<int, TEdge> edgePredicate)
        {
            Contract.Requires(0 <= v && v < this.VertexCount);

            int count = 0;
            for (int i = 0; i < this.VertexCount; ++i)
            {
                TEdge e = this.edges[i, v];
                if (e != null && edgePredicate(e))
                {
                    this.RemoveEdge(e);
                    count++;
                }
            }
            return count;
        }

        public void ClearInEdges(int v)
        {
            Contract.Requires(0 <= v && v < this.VertexCount);

            for (int i = 0; i < this.VertexCount; ++i)
            {
                TEdge e = this.edges[i, v];
                if (e != null)
                    this.RemoveEdge(e);
            }
        }

        public void ClearEdges(int v)
        {
            Contract.Requires(0 <= v && v < this.VertexCount);

            this.ClearInEdges(v);
            this.ClearOutEdges(v);
        }

        #endregion

        #region IMutableIncidenceGraph<int,Edge> Members

        public int RemoveOutEdgeIf(int v, EdgePredicate<int, TEdge> predicate)
        {
            Contract.Requires(0 <= v && v < this.VertexCount);

            int count = 0;
            for (int j = 0; j < this.VertexCount; ++j)
            {
                TEdge e = this.edges[v, j];
                if (e != null && predicate(e))
                {
                    this.RemoveEdge(e);
                    count++;
                }
            }
            return count;
        }

        public void ClearOutEdges(int v)
        {
            Contract.Requires(0 <= v && v < this.VertexCount);

            for (int j = 0; j < this.VertexCount; ++j)
            {
                TEdge e = this.edges[v, j];
                if (e != null)
                    this.RemoveEdge(e);
            }
        }

        #endregion

        #region IMutableGraph<int,Edge> Members
        public void Clear()
        {
            for(int i = 0;i<this.vertexCount;++i)
                for(int j = 0;j<this.vertexCount;++j)
                    this.edges[i,j] = default(TEdge);
            this.edgeCount = 0;
        }
        #endregion

        #region IMutableEdgeListGraph<int,Edge> Members
        public bool AddEdge(TEdge edge)
        {
            Contract.Requires(edge != null);

            if (this.edges[edge.Source, edge.Target]!=null)
                throw new ParallelEdgeNotAllowedException();
            this.edges[edge.Source,edge.Target] = edge;
            this.edgeCount++;
            this.OnEdgeAdded(new EdgeEventArgs<int, TEdge>(edge));
            return true;
        }

        public int AddEdgeRange(IEnumerable<TEdge> edges)
        {
            int count = 0;
            foreach (var edge in edges)
                if (this.AddEdge(edge))
                    count++;
            return count;
        }

        public event EdgeEventHandler<int, TEdge> EdgeAdded;
        protected virtual void OnEdgeAdded(EdgeEventArgs<int, TEdge> args)
        {
            var eh = this.EdgeAdded;
            if (eh != null)
                eh(this, args);
        }

        public bool RemoveEdge(TEdge edge)
        {
            Contract.Requires(edge != null);

            TEdge e = this.edges[edge.Source, edge.Target];
            this.edges[edge.Source, edge.Target] = default(TEdge);
            if (!e.Equals(default(TEdge)))
            {
                this.edgeCount--;
                this.OnEdgeRemoved(new EdgeEventArgs<int, TEdge>(edge));
                return true;
            }
            else
                return false;
        }

        public event EdgeEventHandler<int, TEdge> EdgeRemoved;
        protected virtual void OnEdgeRemoved(EdgeEventArgs<int, TEdge> args)
        {
            var eh = this.EdgeRemoved;
            if (eh != null)
                eh(this, args);
        }

        public int RemoveEdgeIf(EdgePredicate<int, TEdge> predicate)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region ICloneable Members
        private BidirectionalMatrixGraph(
            int vertexCount,
            int edgeCount,
            TEdge[,] edges)
        {
            Contract.Requires(vertexCount > 0);
            Contract.Requires(edgeCount >= 0);
            Contract.Requires(edges != null);
            Contract.Requires(vertexCount == edges.GetLength(0));
            Contract.Requires(vertexCount == edges.GetLength(1));

            this.vertexCount = vertexCount;
            this.edgeCount = edgeCount;
            this.edges = edges;
        }

        public BidirectionalMatrixGraph<TEdge> Clone()
        {
            return new BidirectionalMatrixGraph<TEdge>(
                this.vertexCount,
                this.edgeCount,
                (TEdge[,])this.edges.Clone()
                );
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
        #endregion
    }
}
