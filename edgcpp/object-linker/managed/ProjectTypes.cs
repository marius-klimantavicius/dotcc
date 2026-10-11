using System.Globalization;

namespace DotObj;

internal sealed record RoutineType(long TypeId, long ReturnTypeId, bool Variadic, long[] Parameters, string ExceptionPolicy, string ParameterMode = "prototype",
    long? ObjectTypeId = null, bool ObjectIsConst = false, bool ObjectIsVolatile = false, string ObjectRefQualifier = "none");
internal sealed record ParameterSemantics(long EntityId, long Ordinal, string Role, long? SourceOrdinal);

internal static class RoutineTypes
{
    public static bool IsRoutine(string type) => type.StartsWith("routine(", StringComparison.Ordinal) && type.EndsWith(')');
    public static bool IsFunctionPointer(string type) => ScalarTypes.IsPointer(type) && IsRoutine(ScalarTypes.Pointee(type));
    public static string Key(string result, IEnumerable<string> parameters, bool variadic, string exceptionPolicy = "unwind") =>
        "routine(" + result + ";" + string.Join(',', parameters) + ";" + (variadic ? "1" : "0") + ";" + exceptionPolicy + ")";
    private static string[] Split(string value, char separator)
    {
        var values = new List<string>(); int depth = 0, start = 0;
        for (int index = 0; index < value.Length; index++)
        {
            if (value[index] == '(') depth++;
            else if (value[index] == ')') depth--;
            else if (value[index] == separator && depth == 0) { values.Add(value[start..index]); start = index + 1; }
            if (depth < 0) throw new InvalidDataException("Malformed routine signature");
        }
        if (depth != 0) throw new InvalidDataException("Malformed routine signature");
        values.Add(value[start..]); return values.ToArray();
    }
    public static (string Result, string[] Parameters, bool Variadic, string ExceptionPolicy) Signature(string type)
    {
        if (!IsRoutine(type)) throw new InvalidDataException("Expected routine type");
        string[] parts = Split(type[8..^1], ';');
        if (parts.Length != 4 || parts[2] is not ("0" or "1") || parts[3] is not ("unwind" or "terminate" or "dynamic")) throw new InvalidDataException("Malformed routine signature");
        return (parts[0], parts[1].Length == 0 ? [] : Split(parts[1], ','), parts[2] == "1", parts[3]);
    }
}

internal sealed partial class ObjectFile
{
    public Dictionary<long, RoutineType> Routines { get; } = [];
    public Dictionary<long, RoutineType> SourceRoutines { get; } = [];
    public Dictionary<long, RoutineType> EntitySourceRoutines { get; } = [];
    public Dictionary<(long EntityId, long Ordinal), ParameterSemantics> ParameterRoles { get; } = [];

    private void ReadRoutineMetadata(bool source)
    {
        string table = source ? "source_routine_types" : "routine_types";
        string parametersTable = source ? "source_routine_parameters" : "routine_parameters";
        Dictionary<long, RoutineType> target = source ? SourceRoutines : Routines;
        var parameters = new Dictionary<long, List<long>>();
        using (var query = database.Prepare($"SELECT type_id,ordinal,parameter_type_id FROM {parametersTable} ORDER BY type_id,ordinal"))
            while (query.Read())
            {
                long type = query.Integer(0);
                if (!parameters.TryGetValue(type, out var list)) parameters.Add(type, list = []);
                if (query.Integer(1) != list.Count || query.Integer(2) <= 0) throw new InvalidDataException("Invalid routine parameter identity/order");
                list.Add(query.Integer(2));
            }
        using (var query = database.Prepare($"SELECT type_id,return_type_id,variadic,exception_policy" + (source ? ",parameter_mode,object_type_id,object_is_const,object_is_volatile,object_ref_qualifier" : "") + $" FROM {table} ORDER BY type_id"))
            while (query.Read())
            {
                long id = query.Integer(0), result = query.Integer(1), variadic = query.Integer(2);
                if (id <= 0 || result <= 0 || variadic is not (0 or 1)) throw new InvalidDataException("Invalid routine metadata");
                string policy = query.Text(3);
                if (policy is not ("unwind" or "terminate" or "dynamic") && !(source && policy=="unsupported-dynamic")) throw new InvalidDataException("Invalid routine exception policy");
                string mode = source ? query.Text(4) : "prototype";
                if (mode is not ("prototype" or "unspecified") || mode == "unspecified" && (variadic != 0 || parameters.GetValueOrDefault(id, []).Count != 0))
                    throw new InvalidDataException("Unsupported original unspecified parameter signature");
                long? objectType = source ? query.OptionalInteger(5) : null;
                bool objectConst = source && query.Integer(6) == 1, objectVolatile = source && query.Integer(7) == 1;
                string objectRef = source ? query.Text(8) : "none";
                if (source && (query.Integer(6) is not (0 or 1) || query.Integer(7) is not (0 or 1)) ||
                    objectRef is not ("none" or "lvalue" or "rvalue") ||
                    (objectType is null ? objectConst || objectVolatile || objectRef != "none" : objectType <= 0 || mode != "prototype"))
                    throw new InvalidDataException("Invalid original member receiver metadata");
                target.Add(id, new(id, result, variadic != 0, parameters.GetValueOrDefault(id, []).ToArray(), policy, mode, objectType, objectConst, objectVolatile, objectRef));
            }
        if (parameters.Keys.Any(id => !target.ContainsKey(id))) throw new InvalidDataException("Routine parameters have no signature");
    }

    public string EntityRoutine(long entityId) => Type(Entities[entityId].RoutineTypeId);
}
