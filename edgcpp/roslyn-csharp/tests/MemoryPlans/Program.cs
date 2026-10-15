using Application = Translated.Program;
using RoslynObj.CSharp.Runtime;

long[] expected = [5, 3, 2, 2, 2, 1, 2, 1, 25769803776, 6, 1, 1,
    65530, 0x12345678, 17, 23, 1, 24, 17, 19, 4, 1234, 12, 8];
var result = Application.fixture();
if (result != expected.Length || Application.memory_observation_count() != expected.Length)
    throw new InvalidOperationException("Memory-plan result/count changed.");
for (var index = 0; index < expected.Length; ++index)
    if (Application.memory_observe(index) != expected[index])
        throw new InvalidOperationException($"Memory-plan observation {index} changed.");
if (Application.memory_event_count() != 4) throw new InvalidOperationException("Selected branch event count changed.");
for (var index = 0; index < 4; ++index)
    if (Application.memory_event(index) != index + 1) throw new InvalidOperationException("Selected branch prefix order changed.");
Console.WriteLine($"result={result}\nobservations={expected.Length}");
for (var index = 0; index < expected.Length; ++index) Console.WriteLine($"{index}:{expected[index]}");
Console.WriteLine("events=4");
for (var index = 0; index < 4; ++index) Console.WriteLine($"event{index}:{index + 1}");

// Invalid counts and misaligned addresses have explicit runtime policy;
// the independent native oracle never executes these undefined C accesses.
unsafe
{
    var storage = stackalloc byte[16];
    var aligned = (byte*)(((nuint)storage + 3) & ~(nuint)3);
    var checks = 0;
    try { TargetVolatileMemory.LoadUInt32(aligned + 1); } catch (InvalidOperationException) { ++checks; }
    try { TargetVolatileMemory.StoreUInt16(aligned + 1, 7); } catch (InvalidOperationException) { ++checks; }
    try { TargetArithmetic.ShiftLeft(1UL, 64); } catch (InvalidOperationException) { ++checks; }
    try { TargetArithmetic.ShiftRight(1UL, -1); } catch (InvalidOperationException) { ++checks; }
    if (checks != 4) throw new InvalidOperationException("Memory/shift invalid-input policy changed.");
}
