namespace BotPlayer.Classes.AStar.Collections.PriorityQueue;

internal interface IModelAPriorityQueue<T>
{
    int Count { get; }
    int Push(T item);
    T Pop();
    T Peek();

    void Clear();
}