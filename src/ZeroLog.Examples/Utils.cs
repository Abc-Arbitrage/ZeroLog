using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Spectre.Console;
using ZeroLog.Formatting;

namespace ZeroLog.Examples;

internal static class Utils
{
    private static readonly Style _headerStyle = new(Color.LightSkyBlue1);

    public static void WriteHeader()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("👋 [bold LightSkyBlue1]Welcome to ZeroLog![/]");
        AnsiConsole.MarkupLine("   [LightSkyBlue1]Here are some usage examples and output styles:[/]");
    }

    public static IEnumerable<(DefaultStyle style, string name)> GetDefaultStyles()
    {
        return GetStyles(typeof(DefaultStyle), nameof(DefaultStyle));

        static IEnumerable<(DefaultStyle style, string name)> GetStyles(Type type, string typeName)
            => type.GetProperties(BindingFlags.Public | BindingFlags.Static)
                   .Where(p => p.PropertyType == typeof(DefaultStyle))
                   .Select(p => ((DefaultStyle)p.GetValue(null)!, $"{typeName}.{p.Name}"))
                   .Concat(
                       type.GetNestedTypes(BindingFlags.Public | BindingFlags.Static)
                           .SelectMany(t => GetStyles(t, $"{typeName}.{t.Name}"))
                   );
    }

    public static Exception GetExceptionWithStackTrace()
    {
        try
        {
            FirstLevel();
            throw new Exception("Unreachable");
        }
        catch (Exception ex)
        {
            return ex;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void FirstLevel()
            => SecondLevel();

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void SecondLevel()
            => ThirdLevel();

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThirdLevel()
            => throw new InvalidOperationException("Simulated exception");
    }

    public static void SectionTitle(string title)
    {
        NewLine();

        AnsiConsole.Write(new Panel(new Text(title, _headerStyle))
        {
            BorderStyle = _headerStyle,
            Border = BoxBorder.Double,
            Padding = new Padding(4, 1)
        });
    }

    public static void ItemTitle(string title)
    {
        NewLine();

        AnsiConsole.Write(new Panel(new Text(title, _headerStyle))
        {
            BorderStyle = _headerStyle,
            Border = BoxBorder.Rounded,
            Padding = new Padding(2, 0)
        });
    }

    public static void ShowPattern(string pattern)
    {
        AnsiConsole.MarkupLineInterpolated($"   [bold LightSkyBlue1]Pattern:[/] {pattern}");
        AnsiConsole.WriteLine();
    }

    public static void NewLine()
    {
        LogManager.Flush();
        AnsiConsole.WriteLine();
    }
}
