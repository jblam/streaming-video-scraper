using System;
using System.Threading;

namespace JBlam.NetflixScrape.Core.Models
{
    public struct Commando
    {
        // TODO: consider making everything bool TryGetXYZ(out T xyz);
        // TODO: add source/sequence

        Commando(int source, int sequence, CommandAction action, string parameter)
        {
            Source = source;
            Sequence = sequence;
            Action = action;
            this.parameter = parameter;
        }
        public int Source { get; }
        public int Sequence { get; }
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
            ? $"{Source} {Sequence} {Action}"
            : $"{Source} {Sequence} {Action} {parameter}";

        public static bool TryParse(string serialisedForm, out Commando command)
        {
            // COMMANDO = SOURCE SPACE SEQUENCE SPACE ACTION [ SPACE ARGUMENT ]
            // SOURCE = [0-9]+
            // SEQUENCE = [0-9]+
            // SPACE = ' '
            // ACTION = [A-Za-z]+
            // ARGUMENT = [A-Za-z]+
            var tokens = serialisedForm.Split(new[] { ' ' }, 4);
            string argument;
            if (tokens.Length < 3)
            {
                command = default(Commando);
                return false;
            }
            else if (tokens.Length == 3)
            {
                argument = null;
            }
            else
            {
                argument = tokens[3];
            }
            command = new Commando(int.Parse(tokens[0]), int.Parse(tokens[1]), (CommandAction)Enum.Parse(typeof(CommandAction), tokens[2]), argument);
            return true;
        }
        public static Commando Parse(string serialisedForm) => TryParse(serialisedForm, out var command) ? command : throw new FormatException();

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
        

        public class Builder
        {
            public Builder(int sourceIdentifier)
            {
                this.sourceIdentifier = sourceIdentifier;
            }
            readonly int sourceIdentifier;
            // First-returned sequence identifier should be 0.
            int sequenceIdentifier = -1;
            int NextSequence() => Interlocked.Add(ref sequenceIdentifier, 1);

            public Commando Create(CommandAction action) => IsCompatible(GetParamaterness(action))
                ? new Commando(sourceIdentifier, NextSequence(), action, null)
                : throw new ArgumentException("Action requires a parameter");

            public Commando Create<T>(CommandAction action, T parameter) => IsCompatible(GetParamaterness(action), parameter)
                ? new Commando(sourceIdentifier, NextSequence(), action, parameter.ToString())
                : throw new ArgumentException("Action cannot take a parameter");

            public Commando Create<T>(CommandAction action, T parameter, int quantity) => IsCompatible(GetParamaterness(action), parameter, quantity)
                ? new Commando(sourceIdentifier, NextSequence(), action, $"{parameter},{quantity}")
                : throw new ArgumentException("Action cannot take a parameter and an integer");

            public Commando Create(CommandAction action, int x, int y) => IsCompatible(GetParamaterness(action), x, y)
                ? new Commando(sourceIdentifier, NextSequence(), action, $"{x},{y}")
                : throw new ArgumentException("Action cannot take two integers");

            public Commando Create(CommandAction action, string s) => IsCompatible(GetParamaterness(action), s)
                ? new Commando(sourceIdentifier, NextSequence(), action, s)
                : throw new ArgumentException("Action cannot take a string");
        }
    }
}
