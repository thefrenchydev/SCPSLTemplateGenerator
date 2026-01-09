using System;

namespace SCPSLTemplateGenerator;

public static class ConsoleHelper
{
    public static void WriteSuccess(string message)
    {
        WriteWithColor(message, ConsoleColor.Green);
    }

    public static void WriteInfo(string message)
    {
        WriteWithColor(message, ConsoleColor.Cyan);
    }

    public static void WriteError(string message)
    {
        WriteWithColor(message, ConsoleColor.Red);
    }

    public static void WriteCreated(string message)
    {
        WriteWithColor(message, ConsoleColor.Green);
    }

    private static void WriteWithColor(string message, ConsoleColor color)
    {
        var originalColor = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
        }
        finally
        {
            Console.ForegroundColor = originalColor;
        }
    }
}