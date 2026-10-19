using System;
using System.Runtime.InteropServices;
using System.Text;
using Shouldly;
using Xunit;
using L = DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class CPythonR24ResolverTests
{
    private static void Address(byte* a)
    { new Span<byte>(a,16).Clear(); *(ushort*)a=2; a[3]=80; a[4]=127; a[7]=1; }

    [Fact]
    public void Unsigned_reverse_signature_preserves_numeric_buffers_and_error_indicators()
    {
        byte* a=stackalloc byte[16]; Address(a); byte* host=stackalloc byte[16]; byte* service=stackalloc byte[8];
        new Span<byte>(host,16).Fill(0xa5); new Span<byte>(service,8).Fill(0xa5);
        delegate*<void*,uint,byte*,uint,byte*,uint,int,int> call=&L.getnameinfo;
        L.errno=123; L.h_errno=99;
        call(a,16,host+1,10,service+1,3,3).ShouldBe(0);
        Marshal.PtrToStringUTF8((nint)(host+1)).ShouldBe("127.0.0.1");
        Marshal.PtrToStringUTF8((nint)(service+1)).ShouldBe("80");
        host[0].ShouldBe((byte)0xa5); host[11].ShouldBe((byte)0xa5); service[4].ShouldBe((byte)0xa5);
        L.errno.ShouldBe(123); L.h_errno.ShouldBe(99);
    }
    [Theory]
    [InlineData(1)] [InlineData(2)]
    public void Native_service_overflow_writes_a_truncated_terminated_service_after_the_host(int length)
    {
        byte* a=stackalloc byte[16]; Address(a); byte* host=stackalloc byte[16]; byte* service=stackalloc byte[8];
        new Span<byte>(service,8).Fill(0xa5); L.errno=123;
        L.getnameinfo(a,16,host,16,service+1,(uint)length,3).ShouldBe(-12);
        Marshal.PtrToStringUTF8((nint)host).ShouldBe("127.0.0.1");
        service[length].ShouldBe((byte)0); service[0].ShouldBe((byte)0xa5); service[length+1].ShouldBe((byte)0xa5);
        if(length==2) service[1].ShouldBe((byte)'8');
        L.errno.ShouldBe(123);
    }
    [Fact]
    public void Native_host_overflow_leaves_both_buffers_and_sets_native_ENOSPC()
    {
        byte* a=stackalloc byte[16]; Address(a); byte* host=stackalloc byte[16]; byte* service=stackalloc byte[8];
        new Span<byte>(host,16).Fill(0xa5); new Span<byte>(service,8).Fill(0xa5);
        L.errno=123; L.getnameinfo(a,16,host,9,service,8,3).ShouldBe(-12); L.errno.ShouldBe(28);
        for(int i=0;i<16;i++) host[i].ShouldBe((byte)0xa5);
        for(int i=0;i<8;i++) service[i].ShouldBe((byte)0xa5);
    }
    [Fact]
    public void Bad_flags_conflicting_name_requirement_and_output_omission_follow_native_contract()
    {
        byte* a=stackalloc byte[16]; Address(a); byte* h=stackalloc byte[32];
        L.errno=123;
        L.getnameinfo(a,16,h,32,null,0,256).ShouldBe(-1);
        L.getnameinfo(a,16,h,32,null,0,9).ShouldBe(-2);
        L.getnameinfo(a,16,null,uint.MaxValue,null,uint.MaxValue,3).ShouldBe(0);
        L.getnameinfo(a,15,h,32,null,0,3).ShouldBe(-6);
        L.errno.ShouldBe(123);
    }
    [Fact]
    public void Numeric_borrowed_hostent_preserves_both_errors_and_has_terminated_IPv4_vectors()
    {
        using var owner=new L.RuntimeContext(); using var bind=owner.Enter(); L.errno=123; L.h_errno=99;
        byte* h; fixed(byte* p="127.1\0"u8) h=(byte*)L.gethostbyname(p);
        ((nint)h).ShouldNotBe(0); L.errno.ShouldBe(123); L.h_errno.ShouldBe(99);
        Marshal.PtrToStringUTF8(*(nint*)h).ShouldBe("127.1");
        (*(int*)(h+16)).ShouldBe(2); (*(int*)(h+20)).ShouldBe(4);
        ((nint)(**(byte***)(h+8))).ShouldBe(0);
        byte** addresses=*(byte***)(h+24); new ReadOnlySpan<byte>(addresses[0],4).SequenceEqual(new byte[]{127,0,0,1}).ShouldBeTrue();
        ((nint)addresses[1]).ShouldBe(0);
    }
    [Fact]
    public void Failure_invalidates_borrowed_storage_and_keeps_errno_separate_from_h_errno()
    {
        using var owner=new L.RuntimeContext(); using var bind=owner.Enter();
        fixed(byte* name="localhost\0"u8) ((nint)L.gethostbyname(name)).ShouldNotBe(0);
        var storage=owner.ThreadState.Resolver!; storage.Allocation.ShouldNotBe(0);
        fixed(byte* name="invalid!host\0"u8) ((nint)L.gethostbyname(name)).ShouldBe(0);
        storage.Allocation.ShouldBe(0); storage.Capacity.ShouldBe((nuint)0);
        L.h_errno.ShouldBe(3); L.errno.ShouldBe(22);
    }
    [Fact]
    public void Context_disposal_reclaims_borrowed_storage_even_with_foreign_context_bound()
    {
        var first=new L.RuntimeContext(); using var second=new L.RuntimeContext(); L.ResolverStorage holder;
        using(first.Enter()) { fixed(byte* name="localhost\0"u8) ((nint)L.gethostbyname(name)).ShouldNotBe(0); holder=first.ThreadState.Resolver!; }
        using(second.Enter()) first.Dispose();
        holder.Allocation.ShouldBe(0); first.Dispose();
    }
    [Fact]
    public void Native_numeric_service_wrap_is_not_misreported_as_an_error()
    {
        // Independently measured by addrinfo_contract.c. The old unit assertion
        // treated this native success as EAI_SERVICE; real glibc wraps to port0.
        using var owner=new L.RuntimeContext(); using var binding=owner.Enter();
        byte* hints=stackalloc byte[48]; new Span<byte>(hints,48).Clear(); *(int*)hints=4; void* result=null; L.errno=123;
        fixed(byte* name="127.0.0.1\0"u8) fixed(byte* service="65536\0"u8) L.getaddrinfo(name,service,hints,&result).ShouldBe(0);
        L.errno.ShouldBe(123); owner.NativeAllocations.Count.ShouldBe(3);
        try
        {
            int count=0;
            for(byte* current=(byte*)result;current!=null;current=*(byte**)(current+40))
            {
                byte* address=*(byte**)(current+24); address[2].ShouldBe((byte)0); address[3].ShouldBe((byte)0);
                (*(int*)(current+8)).ShouldBe(++count); // stream, dgram, raw, in native order.
                (*(int*)(current+12)).ShouldBe(count==1?6:count==2?17:0);
                (*(uint*)(current+16)).ShouldBe(16U);
            }
            count.ShouldBe(3);
        }
        finally { L.freeaddrinfo(result); }
        owner.NativeAllocations.ShouldBeEmpty();
        result=(void*)1; L.errno=123;
        fixed(byte* name="127.0.0.1\0"u8) fixed(byte* service="invalid!service\0"u8)
            L.getaddrinfo(name,service,hints,&result).ShouldBe(-8);
        L.errno.ShouldBe(2); ((nint)result).ShouldBe(0); owner.NativeAllocations.ShouldBeEmpty();
        L.freeaddrinfo(result);
    }
}
