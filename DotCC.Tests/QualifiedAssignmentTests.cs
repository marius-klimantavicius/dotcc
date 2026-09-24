using DotCC;
using DotCC.Libc;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed class QualifiedAssignmentTests
{
    [Fact]
    public void Volatile_store_returns_converted_byte_and_update_preserves_old_value()
    {
        byte storage = 2;
        global::DotCC.Libc.Libc.VolatileValue.Store(ref storage, (byte)255).ShouldBe((byte)255);
        global::DotCC.Libc.Libc.VolatileValue.Update(ref storage, 2, static (old, amount) => unchecked((byte)(old + amount)), true).ShouldBe((byte)255);
        storage.ShouldBe((byte)1);
        global::DotCC.Libc.Libc.VolatileValue.Update(ref storage, 4, static (old, amount) => (byte)(old + amount), false).ShouldBe((byte)5);
    }

    [Fact]
    public void Generic_atomic_load_preserves_enum_type_and_atomic_barrier()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid()+".c");
        File.WriteAllText(path, "#include <stdatomic.h>\nenum E { A=1,B=2 }; _Atomic enum E state; int main(void) { atomic_store(&state,B); int result=atomic_load(&state); return result; }");
        try
        {
            string code = Compiler.EmitCSharp([path]);
            code.ShouldContain("Atomic.Load(ref");
            code.ShouldContain("Atomic.Store(ref");
            code.ShouldContain("(int)(Atomic.Load");
        }
        finally { File.Delete(path); }
    }
}
