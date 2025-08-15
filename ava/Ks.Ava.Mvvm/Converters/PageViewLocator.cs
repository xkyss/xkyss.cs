using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace Ks.Ava.Mvvm.Converters;

public class PageViewLocator: IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
        {
            return null;
        }
        var name = param.GetType().Name.Replace("ViewModel", "");
        var typeName = param.GetType().FullName?
            .Replace(".ViewModels", ".Pages")
            .Replace("ViewModels", "");
        var type = FindType(typeName!);
        if (type == null)
        {
            return new TextBlock { Text = "Not Found: " + name };
        }

        var control = Activator.CreateInstance(type)!;
        return (Control) control;
    }

    public bool Match(object? data)
    {
        return true;
    }
    
    public static Type? FindType(string typeName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = asm.GetType(typeName, false, true);
            if (type != null)
            {
                return type;
            }
        }
        return null;
    }
}