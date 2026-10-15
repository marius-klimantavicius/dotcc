using Application = Translated.Program;
using RoslynObj.CSharp.Runtime;

long[] expected = [2, 1, 4, 8, 10, 9, 10, 11, 96, 6, 25769803776, 3,
    -7, 11, 0, 2, -17, 7, 2, 1, 1, 10, 10, 13, 13, 2, 16];
var result = Application.fixture();
if (result != expected.Length || Application.memory_width_count() != expected.Length)
    throw new InvalidOperationException("Memory-width result/count changed.");
for (var index = 0; index < expected.Length; ++index)
    if (Application.memory_width_observe(index) != expected[index])
        throw new InvalidOperationException($"Memory-width exact observation {index} changed.");
Console.WriteLine($"result={result}\nobservations={expected.Length}");
for (var index = 0; index < expected.Length; ++index) Console.WriteLine($"{index}:{expected[index]}");

unsafe
{
    var storage = stackalloc byte[24];
    var aligned = (byte*)(((nuint)storage + 7) & ~(nuint)7);
    var checks = 0;
    try { TargetVolatileMemory.LoadInt32(aligned + 1); } catch (InvalidOperationException) { ++checks; }
    try { TargetVolatileMemory.StoreDouble(aligned + 4, 1.0); } catch (InvalidOperationException) { ++checks; }
    try { TargetArithmetic.ShiftLeft(1U, 32); } catch (InvalidOperationException) { ++checks; }
    if (checks != 3) throw new InvalidOperationException("Memory-width invalid-input policies changed.");
}
