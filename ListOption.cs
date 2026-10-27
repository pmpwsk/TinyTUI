namespace TinyTUI;

public class ListOption(string name, Func<int> action)
{
    public readonly string Name = name;

    public readonly Func<int> Action = action;
}
