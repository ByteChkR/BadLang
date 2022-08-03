using System.Reflection;
using System.Text;

namespace LFC.Commandline;

public static class CommandlineParser<T> where T : class, new()
{
    public static string[] Parse(object obj, string[] args)
    {
        Type t = obj.GetType();
        List<string> argNamesList = new List<string>();
        Dictionary<SwitchAttribute, PropertyInfo> switches = new Dictionary<SwitchAttribute, PropertyInfo>();
        Dictionary<OptionAttribute, PropertyInfo> options = new Dictionary<OptionAttribute, PropertyInfo>();
        foreach (PropertyInfo property in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            CommandlineAttribute? attribute = property.GetCustomAttribute<CommandlineAttribute>();


            if (attribute is SwitchAttribute sw)
            {
                argNamesList.Add(attribute.Name);
                if (attribute.ShortName != null)
                {
                    argNamesList.Add(attribute.ShortName);
                }

                if (property.PropertyType != typeof(bool))
                {
                    throw new Exception($"Switch property '{attribute.Name}' must be of type bool");
                }

                switches[sw] = property;
            }
            else if (attribute is OptionAttribute op)
            {
                argNamesList.Add(attribute.Name);
                if (attribute.ShortName != null)
                {
                    argNamesList.Add(attribute.ShortName);
                }

                options[op] = property;
            }
            else if (attribute != null)
            {
                throw new Exception($"Unknown attribute type '{attribute.GetType().Name}'");
            }
        }

        List<string> arguments = new List<string>(args);

        string[] argNames = argNamesList.ToArray();

        foreach (KeyValuePair<OptionAttribute, PropertyInfo> option in options)
        {
            int index = arguments.IndexOf(option.Key.Name);
            if (index == -1 && option.Key.ShortName != null)
            {
                index = arguments.IndexOf(option.Key.ShortName);
            }

            if (index == -1)
            {
                continue;
            }

            string[] optionValues = GetArguments(arguments, index + 1, argNames).ToArray();

            if (option.Value.PropertyType.IsArray)
            {
                Array array = Array.CreateInstance(option.Value.PropertyType.GetElementType()!, optionValues.Length);
                for (int i = 0; i < optionValues.Length; i++)
                {
                    array.SetValue(Convert(optionValues[i], option.Value.PropertyType.GetElementType()!), i);
                }

                option.Value.SetValue(obj, array);
            }
            else if (option.Value.PropertyType.IsEnum && option.Key is EnumOptionAttribute enumOption && enumOption.AllowMultiple)
            {
                int flag = 0;
                foreach (string value in optionValues)
                {
                    flag |= (int)Enum.Parse(option.Value.PropertyType, value);
                }

                option.Value.SetValue(obj, Enum.ToObject(option.Value.PropertyType, flag));
            }
            else
            {
                if (optionValues.Length != 1)
                {
                    optionValues = optionValues.Take(1).ToArray();
                }

                option.Value.SetValue(obj, Convert(optionValues[0], option.Value.PropertyType));
            }

            arguments.RemoveRange(index, optionValues.Length + 1);
        }

        foreach (KeyValuePair<SwitchAttribute, PropertyInfo> @switch in switches)
        {
            int index = arguments.IndexOf(@switch.Key.Name);
            if (index == -1 && @switch.Key.ShortName != null)
            {
                index = arguments.IndexOf(@switch.Key.ShortName);
            }

            if (index == -1)
            {
                continue;
            }

            @switch.Value.SetValue(obj, true);
            arguments.RemoveAt(index);
        }

        return arguments.ToArray();
    }

    private static IEnumerable<string> GetArguments(IEnumerable<string> args, int start, string[] names)
    {
        foreach (string arg in args.Skip(start))
        {
            if (names.Contains(arg))
            {
                break;
            }

            yield return arg;
        }
    }

    private static object Convert(string o, Type target)
    {
        if (target == typeof(bool))
        {
            return bool.Parse(o);
        }

        if (target == typeof(string))
        {
            return o;
        }

        if (target.IsEnum)
        {
            return Enum.Parse(target, o, true);
        }

        if (target == typeof(byte) ||
            target == typeof(sbyte) ||
            target == typeof(short) ||
            target == typeof(ushort) ||
            target == typeof(int) ||
            target == typeof(uint) ||
            target == typeof(long) ||
            target == typeof(ulong) ||
            target == typeof(float) ||
            target == typeof(double) ||
            target == typeof(decimal))
        {
            decimal d = decimal.Parse(o);

            return System.Convert.ChangeType(d, target);
        }

        throw new Exception($"Unknown type '{target.Name}'");
    }

    public static CommandlineParseResult<T> Parse(string[] args)
    {
        T t = new T();

        return new CommandlineParseResult<T>(t, Parse(t, args));
    }

    public static string GetHelpText()
    {
        StringBuilder sb = new StringBuilder();
        Type t = typeof(T);
        foreach (PropertyInfo property in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            CommandlineAttribute? attribute = property.GetCustomAttribute<CommandlineAttribute>();

            if (attribute is SwitchAttribute sw)
            {
                sb.Append($"Switch: {sw.Name}");
                if (sw.ShortName != null)
                {
                    sb.Append($" / '{sw.ShortName}'");
                }

                if (sw.Description != null)
                {
                    sb.Append($" - {sw.Description}");
                }

                sb.AppendLine();
            }
            else if (attribute is OptionAttribute op)
            {
                sb.Append($"Option({property.PropertyType}) : '{op.Name}'");
                if (op.ShortName != null)
                {
                    sb.Append($" / '{op.ShortName}'");
                }

                if (op.Description != null)
                {
                    sb.Append($" - {op.Description}");
                }

                sb.AppendLine();
            }
            else if (attribute != null)
            {
                throw new Exception($"Unknown attribute type '{attribute.GetType().Name}'");
            }
        }

        return sb.ToString();
    }
}