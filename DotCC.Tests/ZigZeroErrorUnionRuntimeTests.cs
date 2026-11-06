using System;
using System.Runtime.InteropServices;
using DotCC.Layout;
using DotCC.Libc;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed unsafe class ZigZeroErrorUnionRuntimeTests
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Container
    {
        public byte Prefix;
        public ZigErrorUnionZero<Unit> Result;
        public byte Tail;
    }

    [Fact]
    public void Code_only_carrier_matches_proved_layout_without_storing_clr_unit()
    {
        var model = new OffsetLayoutModel(_ => throw new InvalidOperationException());
        var layout = model.Type("e:a:0:p:1:1");
        layout.Size.ShouldBe(2);
        layout.Alignment.ShouldBe(2);
        sizeof(ZigErrorUnionZero<Unit>).ShouldBe(layout.Size);
        sizeof(ErrUnion<Unit>).ShouldBe(4);
        Container container = default;
        ((byte*)&container.Result - (byte*)&container).ShouldBe(2);
        ((byte*)&container.Tail - (byte*)&container).ShouldBe(4);
        sizeof(Container).ShouldBe(6);
        container.Result = ZigErrorUnionZero<Unit>.Err(123);
        (*(ushort*)&container.Result).ShouldBe((ushort)123);
        Should.Throw<OffsetLayoutException>(() => model.Type("e:a:0:p:2:2"));
    }

    [Fact]
    public void Effects_errors_and_curated_carrier_bridges_are_preserved_without_warm_allocations()
    {
        var effects = 0;
        var success = ZigErrorUnionZero<Unit>.Ok(Touch(ref effects));
        effects.ShouldBe(1);
        ErrUnion<Unit> ordinary = success;
        ZigErrorUnionZero<Unit> bridged = ordinary;
        bridged.IsErr.ShouldBeFalse();
        ErrUnion.Try(bridged);
        ErrUnion.Catch(bridged, Touch(ref effects));
        effects.ShouldBe(2);
        ordinary = ErrUnion<Unit>.Err(123);
        bridged = ordinary;
        ordinary = bridged;
        ordinary.Code.ShouldBe((ushort)123);
        Should.Throw<ZigErrorReturn>(() => ErrUnion.Try(bridged)).Code.ShouldBe((ushort)123);
        ErrUnion.Catch(bridged, Touch(ref effects));
        effects.ShouldBe(3);
        for (var i = 0; i < 1000; i++) ErrUnion.Try(success);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 10000; i++)
        {
            ordinary = success;
            success = ordinary;
            ErrUnion.Try(success);
        }
        (GC.GetAllocatedBytesForCurrentThread() - before).ShouldBe(0);
    }

    private static Unit Touch(ref int effects) { effects++; return default; }
}
