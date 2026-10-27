namespace TinyTUI;

public class TextInput(string label, string value) : AbstractInput(label)
{
    public string Value = value;

    public int Cursor = value.Length;

    public bool IsEmpty(out string trimmedValue)
    {
        trimmedValue = Value.Trim();
        return trimmedValue.Length == 0;
    }
}
