namespace TinyTUI;

public abstract class AbstractSelectorInput(string label) : AbstractInput(label)
{
    public abstract string ValueText { get; }

    public abstract void Open();
}
