namespace TinyTUI;

public class TUI
{
    public static readonly ConsoleColor DefaultForegroundColor = Console.ForegroundColor;

    public static int ShowList(string title, ListOption[] options)
        => ShowList(title, () => options);

    public static int ShowList(string title, Func<ListOption[]> optionsLoader)
    {
        int selectedOptionId = 0;
        while (true)
        {
            var options = optionsLoader();
            if (selectedOptionId >= options.Length)
                selectedOptionId = options.Length - 1;
            Clear();
            WriteLine(title);
            WriteLine();
            for (int optionId = 0; optionId < options.Length; optionId++)
            {
                var option = options[optionId];
                Write("[", ConsoleColor.DarkGray);
                Write(optionId == selectedOptionId ? '+' : ' ', ConsoleColor.Green);
                Write("]", ConsoleColor.DarkGray);
                Write(" ");
                WriteLine(option.Name);
            }

            switch (ReadKey())
            {
                case ConsoleKey.Escape:
                    return 0;
                case ConsoleKey.Enter:
                    Clear();
                    try
                    {
                        var stepsToGoBack = options[selectedOptionId].Action();
                        if (stepsToGoBack > 0)
                            return stepsToGoBack - 1;
                    }
                    catch (Exception? ex)
                    {
                        return ShowException(ex);
                    }
                    break;
                case ConsoleKey.UpArrow:
                    if (selectedOptionId >= 1)
                        selectedOptionId--;
                    break;
                case ConsoleKey.DownArrow:
                    if (selectedOptionId <= options.Length - 2)
                        selectedOptionId++;
                    break;
            }
        }
    }

    public static int ShowInputs(string title, AbstractInput[] inputs, Func<int> action)
    {
        int selectedInputId = 0;
        while (true)
        {
            Clear();
            WriteLine(title);
            WriteLine();
            string? lastLabel = null;
            for (int inputId = 0; inputId < inputs.Length; inputId++)
            {
                var input = inputs[inputId];
                if (input.Label != lastLabel)
                {
                    WriteLine(input.Label, ConsoleColor.DarkGray);
                    lastLabel = input.Label;
                }

                Write("> ", inputId == selectedInputId ? ConsoleColor.Green : ConsoleColor.DarkGray);

                switch (input)
                {
                    case TextInput textInput:
                        if (inputId == selectedInputId)
                        {
                            var value = textInput.Value;
                            for (int i = 0; i <= value.Length; i++)
                            {
                                if (i == textInput.Cursor)
                                    Write("|", ConsoleColor.Green);
                                if (i != value.Length)
                                    Write(value[i]);
                            }
                        }
                        else
                        {
                            Write(textInput.Value);
                        }
                        break;
                    case AbstractSelectorInput selectorInput:
                        Write(selectorInput.ValueText);
                        break;
                }

                WriteLine();
            }

            WriteLine();

            Write("[", ConsoleColor.DarkGray);
            Write(selectedInputId == inputs.Length ? '+' : ' ', ConsoleColor.Green);
            Write("]", ConsoleColor.DarkGray);
            Write(" ");
            WriteLine("OK");

            Write("[", ConsoleColor.DarkGray);
            Write(selectedInputId == inputs.Length + 1 ? '+' : ' ', ConsoleColor.Green);
            Write("]", ConsoleColor.DarkGray);
            Write(" ");
            WriteLine("Cancel");

            var key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.Escape:
                    return 0;
                case ConsoleKey.Enter:
                    if (selectedInputId < inputs.Length)
                    {
                        if (inputs[selectedInputId] is AbstractSelectorInput selectorInput)
                            selectorInput.Open();
                    }
                    else if (selectedInputId == inputs.Length)
                    {
                        Clear();
                        try
                        {
                            var stepsToGoBack = action();
                            if (stepsToGoBack > 0)
                                return stepsToGoBack - 1;
                        }
                        catch (Exception? ex)
                        {
                            return ShowException(ex);
                        }
                    }
                    else if (selectedInputId == inputs.Length + 1)
                    {
                        return 0;
                    }
                    break;
                case ConsoleKey.UpArrow:
                    if (selectedInputId >= 1)
                        selectedInputId--;
                    break;
                case ConsoleKey.DownArrow:
                    if (selectedInputId <= inputs.Length)
                        selectedInputId++;
                    break;
                case ConsoleKey.LeftArrow:
                    if (selectedInputId < inputs.Length)
                    {
                        if (inputs[selectedInputId] is TextInput textInput && textInput.Cursor >= 1)
                            textInput.Cursor--;
                    }
                    break;
                case ConsoleKey.RightArrow:
                    if (selectedInputId < inputs.Length)
                    {
                        if (inputs[selectedInputId] is TextInput textInput && textInput.Cursor <= textInput.Value.Length - 1)
                            textInput.Cursor++;
                    }
                    break;
                case ConsoleKey.Backspace:
                    if (selectedInputId < inputs.Length)
                    {
                        if (inputs[selectedInputId] is TextInput textInput && textInput.Cursor >= 1)
                        {
                            var value = textInput.Value;
                            var index = textInput.Cursor;
                            textInput.Value = $"{value[..(index-1)]}{value[index..]}";
                            textInput.Cursor--;
                        }
                    }
                    break;
                case ConsoleKey.Home:
                    if (selectedInputId < inputs.Length)
                    {
                        if (inputs[selectedInputId] is TextInput textInput)
                            textInput.Cursor = 0;
                    }
                    break;
                case ConsoleKey.End:
                    if (selectedInputId < inputs.Length)
                    {
                        if (inputs[selectedInputId] is TextInput textInput)
                            textInput.Cursor = textInput.Value.Length;
                    }
                    break;
                default:
                    if (selectedInputId < inputs.Length && !char.IsControl(key.KeyChar))
                    {
                        if (inputs[selectedInputId] is TextInput textInput)
                        {
                            var value = textInput.Value;
                            var index = textInput.Cursor;
                            textInput.Value = $"{value[..index]}{key.KeyChar}{value[index..]}";
                            textInput.Cursor++;
                        }
                    }
                    break;
            }
        }
    }

    public static int ShowSelector<T>(string title, T[] values, ref T currentValue, Func<T,string> textFunction)
    {
        int selectedValueId = 0;
        for (int i = 0; i < values.Length; i++)
            if (EqualityComparer<T>.Default.Equals(values[i], currentValue))
            {
                selectedValueId = i;
                break;
            }

        while (true)
        {
            Clear();
            WriteLine(title);
            WriteLine();
            for (int valueId = 0; valueId < values.Length; valueId++)
            {
                var value = values[valueId];
                Write("[", ConsoleColor.DarkGray);
                Write(valueId == selectedValueId ? '+' : ' ', ConsoleColor.Green);
                Write("]", ConsoleColor.DarkGray);
                Write(" ");
                if (EqualityComparer<T>.Default.Equals(value, currentValue))
                    WriteLine(textFunction(value), ConsoleColor.Green);
                else
                    WriteLine(textFunction(value));
            }

            WriteLine();

            Write("[", ConsoleColor.DarkGray);
            Write(selectedValueId == values.Length ? '+' : ' ', ConsoleColor.Green);
            Write("]", ConsoleColor.DarkGray);
            Write(" ");
            WriteLine("Cancel");

            switch (ReadKey())
            {
                case ConsoleKey.Escape:
                    return 0;
                case ConsoleKey.Enter:
                    if (selectedValueId < values.Length)
                        currentValue = values[selectedValueId];
                    return 0;
                case ConsoleKey.UpArrow:
                    if (selectedValueId >= 1)
                        selectedValueId--;
                    break;
                case ConsoleKey.DownArrow:
                    if (selectedValueId <= values.Length - 1)
                        selectedValueId++;
                    break;
            }
        }
    }

    public static int ShowMessage(string title, params string[] lines)
    {
        Clear();
        WriteLine(title);
        if (lines.Length > 0)
        {
            WriteLine();
            foreach (var line in lines)
                WriteLine(line, ConsoleColor.DarkGray);
        }
        WriteLine();
        Write("[", ConsoleColor.DarkGray);
        Write("+", ConsoleColor.Green);
        Write("]", ConsoleColor.DarkGray);
        Write(" ");
        WriteLine("OK");

        while (true)
            switch (ReadKey())
            {
                case ConsoleKey.Escape:
                case ConsoleKey.Enter:
                    return 0;
            }
    }

    public static int ShowException(Exception? ex)
    {
        Clear();
        WriteLine("An error has occurred!", ConsoleColor.Red);
        while (ex != null)
        {
            WriteLine();
            WriteLine("Type:", ConsoleColor.DarkGray);
            WriteLine(ex.GetType().Name);
            WriteLine();
            WriteLine("Message:", ConsoleColor.DarkGray);
            WriteLine(ex.Message);
            if (ex.StackTrace != null)
            {
                WriteLine();
                WriteLine("Stack:", ConsoleColor.DarkGray);
                WriteLine(ex.StackTrace);
            }
            ex = ex.InnerException;
        }
        Console.ReadKey(true);
        Environment.Exit(1);
        return int.MaxValue;
    }

    public static void Clear()
    {
        Console.Clear();
    }

    public static void Write(char value)
    {
        Console.Write(value);
    }

    public static void Write(string value)
    {
        Console.Write(value);
    }

    public static void Write(char value, ConsoleColor foregroundColor)
    {
        var oldForegroundColor = Console.ForegroundColor;
        Console.ForegroundColor = foregroundColor;
        Console.Write(value);
        Console.ForegroundColor = oldForegroundColor;
    }

    public static void Write(string value, ConsoleColor foregroundColor)
    {
        var oldForegroundColor = Console.ForegroundColor;
        Console.ForegroundColor = foregroundColor;
        Console.Write(value);
        Console.ForegroundColor = oldForegroundColor;
    }

    public static void WriteLine(char value)
    {
        Console.WriteLine(value);
    }

    public static void WriteLine(string value)
    {
        Console.WriteLine(value);
    }

    public static void WriteLine(char value, ConsoleColor foregroundColor)
    {
        var oldForegroundColor = Console.ForegroundColor;
        Console.ForegroundColor = foregroundColor;
        Console.WriteLine(value);
        Console.ForegroundColor = oldForegroundColor;
    }

    public static void WriteLine(string value, ConsoleColor foregroundColor)
    {
        var oldForegroundColor = Console.ForegroundColor;
        Console.ForegroundColor = foregroundColor;
        Console.WriteLine(value);
        Console.ForegroundColor = oldForegroundColor;
    }

    public static void WriteLine()
    {
        Console.WriteLine();
    }

    public static ConsoleKey ReadKey()
    {
        return Console.ReadKey(true).Key;
    }
}
