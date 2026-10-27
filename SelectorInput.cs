namespace TinyTUI;

public class SelectorInput<T>(string label, T[] values, T value, Func<T, string> textFunction) : AbstractSelectorInput(label)
{
    public readonly T[] Values = values;

    public T Value = value;

    public readonly Func<T, string> TextFunction = textFunction;

    public override string ValueText => TextFunction(Value);

    public override void Open()
        => TUI.ShowSelector<T>(Label, Values, ref Value, TextFunction);
}
