using Application = Translated.Program;

ulong[] expected = [18446744073709551614UL, 1UL, 1305938385386173474UL, 2552847189736476416UL,
    103079215103UL, 4294967292UL, 1311768467463790320UL, 18364758544493064720UL];
if (Application.fixture() != expected.Length) throw new InvalidOperationException("Unsigned128 fixture count changed.");
Console.WriteLine("result=8");
for (var index = 0; index < expected.Length; ++index)
{
    var value = Application.uint128_observe(index);
    if (value != expected[index]) throw new InvalidOperationException("Unsigned128 observation " + index + ": " + value);
    Console.WriteLine($"{index}:{value}");
}
