using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RouteNav.Avalonia.Error;

public static class ExceptionFormatter
{
    public static string ToString(Exception ex)
    {
        if (ex == null)
            throw new ArgumentNullException(nameof(ex));

        var stringBuilder = new StringBuilder();
        ToStringInternal(ex, stringBuilder);

        return stringBuilder.ToString();
    }

    #region FormatExceptionMessage

    private static void ToStringInternal(Exception ex, StringBuilder stringBuilder, int indentLevel = 0)
    {
        if (ex == null)
            throw new ArgumentNullException(nameof(ex));

        // Special treatment for TargetInvocationException
        var targetInvocationException = ex as TargetInvocationException;
        if (targetInvocationException?.InnerException != null)
        {
            stringBuilder.AppendLine($"{Indent(indentLevel)}TargetInvocationException");
            ToStringInternal(targetInvocationException.InnerException, stringBuilder, indentLevel);
            return;
        }

        // Print exception message
        stringBuilder.AppendLine($"{Indent(indentLevel)}{ex.GetType().Name}");
        stringBuilder.AppendLine($"{Indent(indentLevel)}Message = {ex.Message}");
        stringBuilder.AppendLine(); // Newline

        try
        {
            // Print 'Source'
            var source = ex.GetType().GetProperty("Source")?.GetValue(ex, null);
            if (source != null)
                stringBuilder.AppendLine($"{Indent(indentLevel)}Source = {source}");

            // Print 'StackTrace'
            var stackTrace = ex.GetType().GetProperty("StackTrace")?.GetValue(ex, null);
            if (stackTrace != null)
                stringBuilder.AppendLine($"{Indent(indentLevel)}StackTrace ={Environment.NewLine}{stackTrace}");

            stringBuilder.AppendLine(); // Newline
        }
        catch
        {
            // Don't throw on exception properties (just skip them)
        }

        if (ex is AggregateException aggregateException && aggregateException.InnerExceptions.Count > 0)
        {
            // AggregateException
            var i = 0;
            foreach (var innerException in aggregateException.InnerExceptions)
            {
                stringBuilder.AppendLine($"{Indent(indentLevel)}=== Inner Exception {i++} ===");
                ToStringInternal(innerException, stringBuilder, indentLevel + 1);
                stringBuilder.AppendLine(); // Newline
            }
        }
        else if (ex.InnerException != null)
        {
            // 'Flat' (normal) exception
            stringBuilder.AppendLine($"{Indent(indentLevel)}=== Inner Exception ===");
            ToStringInternal(ex.InnerException, stringBuilder, indentLevel + 1);
            stringBuilder.AppendLine(); // Newline
        }
    }

    private static string Indent(int indentLevel)
    {
        return String.Join("", Enumerable.Repeat(" ", indentLevel));
    }

    #endregion
}