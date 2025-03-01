using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Path<T> : IEnumerable<Vertex<T>>,
    IEnumerable<GraphEdge<T>>, ITraceable<T> where T : IEquatable<T>
{
    private Dictionary<Vertex<T>, Vertex<T>> path;

    public Vertex<T> Start { get; private set; }
    public Vertex<T> End { get; private set; }
    public Graph<T> Graph { get; private set; }

    /// <summary>
    /// Creates a new path.
    /// </summary>
    public Path(Vertex<T> start, Vertex<T> end,
        Dictionary<Vertex<T>, Vertex<T>> invertedPath,
        Dictionary<T, float> totalCosts, Graph<T> graph)
    {
        if (start == end)
            throw new PathfindingException($"start and end are the same: {start}");

        path = new();
        this.Graph = graph;
        this.Start = start;
        this.End = end;

        Vertex<T> next = end;

        while (next != start)
        {
            var prev = next;
            try
            {
                next = invertedPath[next];
            }
            catch (KeyNotFoundException e)
            {
                throw new PathfindingException(
                    $"Cannot find vertex ({next}) of inverted path.",
                    e
                );
            }

            if (path.TryGetValue(next, out var other))
            {
                if (totalCosts[prev.id] < totalCosts[other.id])
                {
                    path[next] = prev;
                }
            }
            else
            {
                path[next] = prev;
            }
        }
    }

    public void UpdatePath(Vertex<T> from, Vertex<T> to)
    {
        path[from] = to;
    }

    /// <summary>
    /// Traverse to the next item in the path.
    /// </summary>
    /// <param name="begin">The Vertex or Vertex ID at which to begin
    /// iteration.</param>
    /// <returns></returns>
    public T Next(T begin)
    {
        return path[Graph.GetVertex(begin)].id;
    }

    /// <inheritdoc cref="Next(T)"/>
    public Vertex<T> Next(Vertex<T> begin)
    {
        try
        {
            return path[begin];
        }
        catch (KeyNotFoundException e)
        {
            var trailingEdge = path
                .Where(e => e.Value == begin);
            throw new PathfindingException($"Vertex {begin} is not in path.", e);
        }
    }

    /// <summary>
    /// Traverse to the next N items on the path.
    /// </summary>
    /// <param name="steps">How many items to traverse?</param>
    /// <param name="stepsTaken">How many items were actually traversed before
    /// the iteration ended?</param>
    /// <inheritdoc cref="Next(T)"/>
    public T NextN(T begin, int steps, out int stepsTaken)
    {
        return NextN(Graph.GetVertex(begin), steps, out stepsTaken).id;
    }

    /// <inheritdoc cref="NextN(T, int, out int)"/>
    public Vertex<T> NextN(Vertex<T> begin, int steps, out int stepsTaken)
    {
        var next = begin;
        stepsTaken = 0;

        for (int i = 0; i < steps; i++)
        {
            next = path[next];

            if (next == End)
                break;

            stepsTaken++;
        }

        return next;
    }

    /// <summary>
    /// Traverse the path until either we run out of cost or we reach the end.
    /// </summary>
    /// <param name="maxCost">The maximum cost to concur.</param>
    /// <param name="costUsed">The amount of cost used for the
    /// traversal.</param>
    /// <inheritdoc cref="NextN(T, int, out int)"/>
    public T NextCost(T begin, float maxCost,
        out float costUsed, out float stepsTaken)
    {
        return NextCost(Graph.GetVertex(begin), maxCost,
            out costUsed, out stepsTaken).id;
    }

    /// <summary>
    /// Traverse the path until either we run out of cost or we reach the end.
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="maxCost"></param>
    /// <returns></returns>
    public Vertex<T> NextCost(Vertex<T> begin, float maxCost,
        out float costUsed, out float stepsTaken)
    {
        var next = begin;
        var peekNext = Next(next);
        costUsed = 0;
        stepsTaken = 0;

        while (next != End && costUsed + peekNext.heuristic <= maxCost)
        {
            next = peekNext;
            peekNext = Next(peekNext);
            costUsed += next.heuristic;
            stepsTaken++;
        }

        return next;
    }

    /// <summary>
    /// Returns all the vertices along this path from <paramref name="from"/> to
    /// <paramref name="to"/>.
    /// </summary>
    /// <param name="from">The vertex to start the iteration from. This will be
    /// included in the enumerable.</param>
    /// <param name="to">The vertex to end the iteration at. This will be
    /// included in the enumerable.</param>
    /// <returns></returns>
    public IEnumerable<Vertex<T>> GetVertices(Vertex<T> from, Vertex<T> to)
    {
        var next = from;

        do
        {
            yield return next;
            next = Next(next);
        } while (next != to);

        yield return to;
    }

    /// <summary>
    /// Returns all the vertices along this path from <paramref name="from"/> to
    /// <see cref="End"/>.
    /// </summary>
    /// <inheritdoc cref="GetVertices(Vertex{T}, Vertex{T})"/>
    public IEnumerable<Vertex<T>> GetVertices(Vertex<T> from)
    {
        return GetVertices(from, End);
    }

    /// <summary>
    /// Returns all the vertices along this path from <see cref="Start"/> to
    /// <see cref="End"/>.
    /// </summary>
    /// <inheritdoc cref="GetVertices(Vertex{T})"/>
    public IEnumerable<Vertex<T>> GetVertices()
    {
        return GetVertices(Start);
    }

    /// <summary>
    /// Iterates through the path, beginning at <paramref name="begin"/>, until
    /// we find a vertex with an id of <paramref name="stopAt"/>,
    /// </summary>
    /// <param name="begin">Where to start the iteration?</param>
    /// <param name="stopAt">What ID to stop at?</param>
    /// <returns></returns>
    public Vertex<T> Seek(Vertex<T> begin, T stopAt)
    {
        foreach (var vertex in GetVertices(begin))
        {
            if (vertex.id.Equals(stopAt))
                return vertex;
        }

        return null;
    }

    /// <summary>
    /// Gets the length of the entire path, from <see cref="Start"/> to <see
    /// cref="End"/>.
    /// </summary>
    /// <returns></returns>
    public int GetLength()
    {
        int len = 0;

        Vertex<T> next = Start;

        while (next != End)
        {
            next = Next(next);
            len++;
        }

        return len;
    }

    /// <summary>
    /// Gets the max cost of any vertex of the path.
    /// </summary>
    /// <returns></returns>
    public float MaxSingleCost()
    {
        float max = 0;

        Vertex<T> next = Start;

        while (next != End)
        {
            next = Next(next);
            max = Math.Max(max, next.heuristic);
        }

        return max;
    }

    public IEnumerator<GraphEdge<T>> GetTraces()
    {
        var next = Start;

        while (next != End && Next(next) != End)
        {
            var prev = next;
            next = Next(next);

            yield return Graph.GetEdge(prev, next);
        }
    }

    public IEnumerator<Vertex<T>> GetEnumerator()
    {
        return GetVertices(Start, End).GetEnumerator();
    }


    IEnumerator<GraphEdge<T>> IEnumerable<GraphEdge<T>>.GetEnumerator()
    {
        return GetTraces();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}