namespace LibSqlite3Orm;

public static class ConsoleLogger
{
    private static List<Tuple<string, int>> executedStatements = new();
    
    public static void WriteLine(string message)
    {
        WriteLine(null, message);
    }
    
    public static void WriteLine(ConsoleColor? color, string message)
    {
        var lastStatement = executedStatements.LastOrDefault();
        if (lastStatement is not null)
        {
            if (string.Equals(message, lastStatement.Item1, StringComparison.InvariantCultureIgnoreCase))
            {
                executedStatements[^1] = new Tuple<string, int>(lastStatement.Item1, lastStatement.Item2 + 1);
            }
            else
            {
                executedStatements.Clear();
            
                if (lastStatement.Item2 > 1)
                {
                    ConsoleWriteLineWrapper(ConsoleColor.DarkYellow,
                        $"Previous statement repeated {lastStatement.Item2 - 1} time(s)");
                }

                executedStatements.Add(new Tuple<string, int>(message, 1));
                ConsoleWriteLineWrapper(color, message);
            }
        }
        else
        {
            executedStatements.Add(new Tuple<string, int>(message, 1));
            ConsoleWriteLineWrapper(color, message);
        }
    }

    private static void ConsoleWriteLineWrapper(ConsoleColor? color, string message)
    {
        if (!color.HasValue)
        {
            Console.WriteLine(message);    
            return;
        }
        
        var prevColor = Console.ForegroundColor;
        Console.ForegroundColor = color.Value;
        Console.WriteLine(message);
        Console.ForegroundColor = prevColor;
    }    
}