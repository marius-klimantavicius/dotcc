using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using Application = Translated.Program;
using InputView = Translated.VectorBuffer_BorrowedReadView;

internal static unsafe partial class CapturedBindings
{
    internal static partial int ReadKind(void* input) => Application.GetVectorType((InputView*)input);

    [StructLayout(LayoutKind.Sequential)]
    private struct ViewPrefix
    {
        public byte Prefix;
        public InputView Value;
    }

    internal static partial JsonObject InspectPhysical(void* first, void* second)
    {
        InputView* views = stackalloc InputView[2];
        ViewPrefix prefix = default;
        var viewSize = sizeof(InputView);
        var kindOffset = (int)((byte*)&views[0].vector_type - (byte*)&views[0]);
        var alignmentAnchorOffset = (int)((byte*)&views[0].__borrowed_alignment - (byte*)&views[0]);
        var prefixOffset = (int)((byte*)&prefix.Value - (byte*)&prefix);
        var stride = (int)((byte*)&views[1] - (byte*)&views[0]);
        var inputsAligned = Aligned8(first) && Aligned8(second);
        var viewsAligned = Aligned8(&views[0]) && Aligned8(&views[1]) && Aligned8(&prefix.Value);
        if (viewSize != 56 || kindOffset != 24 || alignmentAnchorOffset != 0 || prefixOffset != 8 || stride != 56 ||
            !inputsAligned || !viewsAligned)
            throw new InvalidOperationException("The actual published borrowed view does not match captured size, field offset or alignment.");

        var firstValue = ((InputView*)first)->vector_type;
        var secondValue = ((InputView*)second)->vector_type;
        if (firstValue != 255 || secondValue != 0)
            throw new InvalidOperationException("The typed physical view does not expose the final byte mutations.");
        var finalKinds = new JsonArray();
        finalKinds.Add((JsonNode)JsonValue.Create((int)firstValue)!);
        finalKinds.Add((JsonNode)JsonValue.Create((int)secondValue)!);
        return new JsonObject
        {
            ["viewSize"] = viewSize,
            ["kindOffset"] = kindOffset,
            ["alignmentAnchorOffset"] = alignmentAnchorOffset,
            ["sequentialPrefixOffset"] = prefixOffset,
            ["elementStride"] = stride,
            ["actualInputAddressesAligned"] = inputsAligned,
            ["actualViewAddressesAligned"] = viewsAligned,
            ["finalTypedFieldValues"] = finalKinds
        };
    }

    private static bool Aligned8(void* address) => ((nuint)address) % 8 == 0;
}
