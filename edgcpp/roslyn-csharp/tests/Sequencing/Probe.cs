using RoslynObj;
using System.Globalization;
using System.Text.Json;

if (args.Length != 1) throw new ArgumentException("Expected genuine input object path.");
using var input = RoslynObject.Open(args[0]);
var functions = input.TranslationUnit.Functions.Where(f => f.Body is not null).ToArray();
var bodies = functions.Select((function, ordinal) => new
{
    Locator = $"function:{ordinal + 1}",
    Name = function.CurrentName,
    Operations = function.Body!.Root.DescendantOperations().ToArray()
}).ToArray();
var operators = bodies.SelectMany(b => b.Operations).OfType<OperatorOperation>().ToArray();
Console.WriteLine(JsonSerializer.Serialize(new
{
    InventoryOnly = true,
    GeneratedExecutionAccepted = false,
    Artifact = input.Manifest.ArtifactIdentity,
    Signatures = input.TranslationUnit.Functions.Select((function, ordinal) => new
    {
        Locator = $"signature:{ordinal + 1}",
        Name = function.CurrentName,
        Variadic = function.Signature.IsVariadic,
        function.Signature.HasPrototype,
        function.Signature.HasThisParameter,
        function.Signature.ReturnsValueAsParameter,
        function.Signature.ReturnsValueViaCopyConstructor,
        SelectedSignatureParameters = function.Signature.Parameters.Select(parameter => new
        {
            parameter.Ordinal,
            Role = parameter.Role.Name,
            parameter.Descriptor.Details.PassedViaCopyConstructor,
            Type = TypeFacts(parameter.Type)
        }),
        Return = TypeFacts(function.ReturnType),
        Parameters = function.Parameters.Select(parameter => new
        {
            parameter.Ordinal, parameter.Name,
            Role = parameter.Role.Name,
            parameter.Descriptor.Details.PassedViaCopyConstructor,
            Type = TypeFacts(parameter.Type)
        })
    }),
    Functions = bodies.Select(body => new
    {
        body.Locator, body.Name,
        Families = body.Operations.GroupBy(n => n is OperatorOperation op ? op.OperatorKind.Name : n.Kind.Name)
            .OrderBy(g => g.Key, StringComparer.Ordinal).Select(g => new { Kind = g.Key, Count = g.Count() }),
        Calls = body.Operations.OfType<CallOperation>().Select((call, ordinal) => new
        {
            Locator = $"{body.Locator}/call:{ordinal + 1}",
            Kind = call.CallKind.Name,
            LeftToRight = call.Operator.EvalLeftToRight,
            RightToLeft = call.Operator.EvalRightToLeft,
            CalleeKind = call.Callee is OperatorOperation op ? op.OperatorKind.Name : call.Callee.Kind.Name,
            Target = call.TargetMethod?.CurrentName,
            RawOperands = call.Operands.Count,
            Arguments = call.Arguments.Count,
            HasReceiver = call.Receiver is not null,
            HasReturnSlot = call.Call.ReturnSlot is not null,
            ArgumentsMatchRawOperands = call.Operands.Count == call.Arguments.Count + 1
                && call.Operands[0].Equals(call.Callee)
                && call.Arguments.Select((argument, index) => call.Operands[index + 1].Equals(argument.Value)).All(equal => equal),
            CalleeNestedCalls = call.Callee.DescendantOperations().OfType<CallOperation>().Count()
        }),
        Copies = body.Operations.OfType<OperatorOperation>().Where(op => op.OperatorKind.Name is "eok_assign" or "eok_bassign"
            && op.Operands.Count == 2 && op.Operands[0].Type is not ArithmeticType).Select(op => new
        {
            Kind = op.OperatorKind.Name,
            LeftToRight = op.Operator.EvalLeftToRight,
            RightToLeft = op.Operator.EvalRightToLeft,
            ResultType = op.Type.Kind.Name,
            LeftType = op.Operands[0].Type.Kind.Name,
            RightType = op.Operands[1].Type.Kind.Name,
            LeftSize = op.Operands[0].Type.Size?.ToString(CultureInfo.InvariantCulture),
            RightSize = op.Operands[1].Type.Size?.ToString(CultureInfo.InvariantCulture)
        }),
        Temporaries = body.Operations.OfType<TemporaryOperation>().Select(temporary => new
        {
            Kind = temporary.Kind.Name,
            Initialization = temporary.Initialization?.Kind.Name,
            TargetName = temporary.Target?.Name,
            Type = temporary.Type.Kind.Name,
            SourceType = temporary.SourceType?.Kind.Name
        })
    }),
    OperandConstraints = operators.GroupBy(op => (op.Operator.EvalLeftToRight, op.Operator.EvalRightToLeft))
        .OrderBy(g => g.Key.EvalLeftToRight).ThenBy(g => g.Key.EvalRightToLeft)
        .Select(g => new { LeftToRight = g.Key.EvalLeftToRight, RightToLeft = g.Key.EvalRightToLeft, Count = g.Count() }),
    BitFields = input.TranslationUnit.Types.OfType<RecordType>().Where(record => record.Layout is not null)
        .SelectMany(record => record.Layout!.Fields).Where(field => field.Field.Details.IsBitField).Select(field => new
        {
            Name = field.Field.Name,
            Signed = field.Field.Details.BitFieldIsSigned,
            Offset = field.Offset?.ToString(CultureInfo.InvariantCulture),
            BitOffset = field.BitOffset?.ToString(CultureInfo.InvariantCulture),
            BitWidth = field.BitWidth?.ToString(CultureInfo.InvariantCulture)
        })
}, new JsonSerializerOptions { WriteIndented = true }));

static object TypeFacts(CppType type)
{
    var references = new List<object>();
    var seen = new HashSet<CppType>();
    var current = type;
    while (current is TypeReference reference && seen.Add(current))
    {
        references.Add(Dimensions(current));
        current = reference.UnderlyingType;
    }
    return new
    {
        Effective = Dimensions(type),
        TypeReferenceChain = references,
        Unwrapped = Dimensions(current),
        ReferenceCycle = current is TypeReference,
        Arithmetic = current is ArithmeticType arithmetic ? new
        {
            Kind = arithmetic.Arithmetic.ArithmeticKind.Name,
            arithmetic.Arithmetic.Bits,
            arithmetic.Arithmetic.Signed,
            arithmetic.Arithmetic.IsBool
        } : null
    };
}

static object Dimensions(CppType type) => new
{
    Kind = type.Kind.Name,
    Size = type.Size?.ToString(CultureInfo.InvariantCulture),
    Alignment = type.Alignment?.ToString(CultureInfo.InvariantCulture),
    DescriptorSize = type.Details.DescriptorSize?.ToString(CultureInfo.InvariantCulture),
    DescriptorAlignment = type.Details.DescriptorAlignment?.ToString(CultureInfo.InvariantCulture)
};
