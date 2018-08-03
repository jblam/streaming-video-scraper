using System;

namespace JBlam.NetflixScrape.Core.Models
{
    public struct Commando
    {
        // TODO: consider making everything bool TryGetXYZ(out T xyz);
        // TODO: add source/sequence

        Commando(CommandAction action, string parameter)
        {
            Action = action;
            this.parameter = parameter;
        }
        public CommandAction Action { get; }
        readonly string parameter;
        (string first, string second) GetParamTokens()
        {
            if (parameter == null) { throw new MalformedCommandException(this); }
            var tokens = parameter.Split(new[] { ',' }, 2);
            if (tokens.Length != 2) { throw new MalformedCommandException(this); }
            // string.Split does not return null tokens
            return (tokens[0], tokens[1]);
        }

        public int GetInt() => int.TryParse(parameter ?? throw new MalformedCommandException(this), out var output)
            ? output
            : throw new MalformedCommandException(this);
        public int GetInt(int fallback) => parameter == null
            ? fallback
            : int.TryParse(parameter, out var result)
                ? result
                : throw new MalformedCommandException(this);
        public (int, int) GetInts()
        {
            var (first, second) = GetParamTokens();
            return int.TryParse(first, out var r1) && int.TryParse(second, out var r2)
                ? (r1, r2)
                : throw new MalformedCommandException(this);
        }

        public T GetEnum<T>() where T : struct => Enum.TryParse<T>(parameter ?? throw new MalformedCommandException(this), out var output)
            ? output
            : throw new MalformedCommandException(this);
        public (T, int) GetEnumInt<T>() where T : struct
        {
            var (first, second) = GetParamTokens();
            return Enum.TryParse<T>(first, out var r1) && int.TryParse(second, out var r2)
                ? (r1, r2)
                : throw new MalformedCommandException(this);
        }
        public (T, int) GetEnumInt<T>(int fallback) where T : struct
        {
            var (first, second) = GetParamTokens();
            return Enum.TryParse<T>(first, out var r1)
                ? (r1, int.TryParse(second, out var r2) ? r2 : fallback)
                : throw new MalformedCommandException(this);
        }
        public string GetString() => parameter ?? throw new MalformedCommandException(this);

        public override string ToString() => parameter == null
            ? Action.ToString()
            : Action.ToString() + " " + parameter.ToString();

        public static Commando Parse(string serialisedForm)
        {
            if (serialisedForm == null) { throw new ArgumentNullException(nameof(serialisedForm)); }
            if (serialisedForm.Length > 100) { throw new ArgumentException("Input too long"); }
            var separatorIndex = serialisedForm.IndexOf(' ');
            var actionString = separatorIndex < 0 ? serialisedForm : serialisedForm.Substring(0, separatorIndex);
            var paramString = separatorIndex < 0
                ? null
                : serialisedForm.Substring(separatorIndex + 1, serialisedForm.Length - separatorIndex - 1);
            if (Enum.TryParse<CommandAction>(actionString, out var action))
            {
                return new Commando(action, paramString);
            }
            return default(Commando);
        }

        enum Parameterness
        {
            NoParam,
            OptionalEnum,
            Enum,
            EnumOptionalInt,
            OptionalInt,
            OptionalString,
            String,
            IntPair
        }
        static bool IsCompatible(Parameterness parameterness) => parameterness == Parameterness.NoParam
            || parameterness == Parameterness.OptionalEnum
            || parameterness == Parameterness.OptionalInt
            || parameterness == Parameterness.OptionalString;
        static bool IsCompatible(Parameterness parameterness, string s) => parameterness == Parameterness.String;
        static bool IsCompatible<T>(Parameterness parameterness, T t) => parameterness == Parameterness.OptionalEnum
            || parameterness == Parameterness.Enum;
        static bool IsCompatible<T>(Parameterness parameterness, T t, int j) => parameterness == Parameterness.EnumOptionalInt;
        static bool IsCompatible(Parameterness parameterness, int i, int j) => parameterness == Parameterness.IntPair;
        static Parameterness GetParamaterness(CommandAction action)
        {
            switch (action)
            {
                case CommandAction.Unidentified:
                case CommandAction.State:
                case CommandAction.Browse:
                case CommandAction.Close:
                case CommandAction.Cancel:
                case CommandAction.Reload:
                case CommandAction.Navigate:
                case CommandAction.Pause:
                case CommandAction.Play:
                    return Parameterness.NoParam;
                case CommandAction.Select:
                case CommandAction.Info:
                    return Parameterness.OptionalString;
                case CommandAction.Skip:
                    return Parameterness.OptionalInt;
                case CommandAction.Jump:
                    return Parameterness.String;
                case CommandAction.MouseMove:
                case CommandAction.MouseSet:
                    return Parameterness.IntPair;
                case CommandAction.MouseClick:
                    return Parameterness.Enum;
                case CommandAction.MouseWheel:
                    return Parameterness.EnumOptionalInt;
                default:
                    throw new ArgumentException();
            }
        }
        
        public static Commando Create(CommandAction action) => IsCompatible(GetParamaterness(action))
            ? new Commando(action, null)
            : throw new ArgumentException("Action requires a parameter");
        public static Commando Create<T>(CommandAction action, T parameter) => IsCompatible(GetParamaterness(action), parameter)
            ? new Commando(action, parameter.ToString())
            : throw new ArgumentException("Action cannot take a parameter");
        public static Commando Create<T>(CommandAction action, T parameter, int quantity) => IsCompatible(GetParamaterness(action), parameter, quantity)
            ? new Commando(action, $"{parameter},{quantity}")
            : throw new ArgumentException("Action cannot take a parameter and an integer");
        public static Commando Create(CommandAction action, int x, int y) => IsCompatible(GetParamaterness(action), x, y)
            ? new Commando(action, $"{x},{y}")
            : throw new ArgumentException("Action cannot take two integers");
        public static Commando Create(CommandAction action, string s) => IsCompatible(GetParamaterness(action), s)
            ? new Commando(action, s)
            : throw new ArgumentException("Action cannot take a string");
    }
}
