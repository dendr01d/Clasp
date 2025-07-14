namespace ClaspCompiler.SchemeTypes
{
    internal record FunctionType(SchemeType OutputType, SchemeType[] InputTypes, SchemeType? PreType = null) : SchemeType
    {
        public SchemeType? LatentPredicate { get; init; } = null;

        public FunctionType(SchemeType outputType, params SchemeType[] types)
            : this(outputType, types, null)
        {
            if (outputType is null) throw new Exception();
        }

        public FunctionType(SchemeType outputType, IEnumerable<SchemeType> inputTypes, SchemeType? preType = null)
            : this(outputType, [.. inputTypes], preType)
        {
            if (outputType is null) throw new Exception();
        }

        public static FunctionType ConstructPredicate(SchemeType predicatedOn)
        {
            return new FunctionType(SchemeType.Boolean, SchemeType.Any)
            {
                LatentPredicate = predicatedOn
            };
        }

        public override string AsString => $"({FormatInputs(InputTypes, PreType)} → {OutputType})";

        private static string FormatInputs(SchemeType[] parameters, SchemeType? dottedParam)
        {
            if (dottedParam is null)
            {
                if (parameters.Length > 0)
                {
                    return $"{string.Join(' ', parameters.AsEnumerable())}";
                }
                else
                {
                    return SchemeType.Bottom.ToString();
                }
            }
            else
            {
                if (parameters.Length == 0)
                {
                    return dottedParam.ToString();
                }
                else
                {
                    return $"{string.Join(' ', parameters.AsEnumerable())} {dottedParam}*";
                }
            }
        }
    }
}