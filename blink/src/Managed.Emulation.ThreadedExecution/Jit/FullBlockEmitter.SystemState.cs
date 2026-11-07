using Blink=Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    internal enum FullSystemStateOperation:uint
    {LoadMxcsr=1,StoreMxcsr,ReadFsBase,ReadGsBase,WriteFsBase,WriteGsBase,LoadFence,StoreFence,FullFence,CacheFlush}
    internal static bool CanInlineSystemState(JitInstruction i)=>i.Operation==JitOperation.SystemState;
    internal static bool ValidateSystemState(JitInstruction i)
    {
        if(!CanInlineSystemState(i))return false;
        var op=(FullSystemStateOperation)i.Options;var memory=new JitOperand(0,0,IsMemory:true);
        if(i.Length is <3 or >15||op is <FullSystemStateOperation.LoadMxcsr or >FullSystemStateOperation.CacheFlush||
            i.Target!=0||i.Condition!=0||i.Extra!=default)throw new ArgumentException("Native state metadata");
        bool valid=op switch
        {
            FullSystemStateOperation.LoadMxcsr=>i.Width==32&&i.SourceWidth==32&&i.Source==memory&&i.Destination==default,
            FullSystemStateOperation.StoreMxcsr=>i.Width==32&&i.SourceWidth==32&&i.Destination==memory&&i.Source==default,
            FullSystemStateOperation.CacheFlush=>i.Width==8&&i.SourceWidth==8&&i.Source==memory&&i.Destination==default,
            FullSystemStateOperation.ReadFsBase or FullSystemStateOperation.ReadGsBase=>i.Width is 32 or 64&&
                i.SourceWidth==0&&i.Source==default&&IntegerRegister(i.Destination,i.Width)&&i.Destination.Immediate==0&&i.Destination.ByteOffset==0,
            FullSystemStateOperation.WriteFsBase or FullSystemStateOperation.WriteGsBase=>i.Width is 32 or 64&&
                i.SourceWidth==0&&i.Destination==default&&IntegerRegister(i.Source,i.Width)&&i.Source.Immediate==0&&i.Source.ByteOffset==0,
            _=>i.Width==64&&i.SourceWidth==0&&i.Source==default&&i.Destination==default
        };
        if(!valid)throw new ArgumentException("Native state operand shape");
        if(i.Source.IsMemory||i.Destination.IsMemory)
        {
            var a=i.Address;
            if(a.Base is <-1 or >15||a.Index is <-1 or >15||a.Scale is <0 or >3||
                a.AddressWidth is not(32 or 64)||a.Segment is <-1 or >5||a.RipRelative&&(a.Base!=-1||a.Index!=-1))
                throw new ArgumentException("Native state address");
        }
        else if(i.Address!=default)throw new ArgumentException("Native state unused address");
        if(!SystemStateLayout.Supported)throw new PlatformNotSupportedException("Native state layout");return true;
    }
    private static class SystemStateLayout
    {
        internal static readonly bool Supported=Check();
        private static unsafe bool Check()
        {
            Blink.Machine m=default;byte* b=(byte*)&m;
            fixed(ulong* fs=&m.fs.@base,gs=&m.gs.@base)
                return (byte*)fs-b==528&&(byte*)gs-b==544&&(byte*)&m.mxcsr-b==688;
        }
    }

    private ref partial struct Writer
    {
        private void SystemStateGp()
        {
            Bytes(0x41,0xc7,0x82);U32(168);U32(13);
            Bytes(0x41,0xc7,0x82);U32(172);U32(128);
            Bytes(0x49,0xc7,0x82);U32(176);U32(0);JumpExit(FullBlockExit.GuestFault);
        }
        private bool TrySystemStateOperation(in JitInstruction i)
        {
            if(!CanInlineSystemState(i))return false;var op=(FullSystemStateOperation)i.Options;
            if(op==FullSystemStateOperation.LoadMxcsr)
            {
                ScalarMiscCheckCanonical(i,32);NativeMemoryPrepare(i,FullMemoryAccess.Read,32);NativeMemoryLoad(32);
                IntegerMov(1,0,32);IntegerConstant(4,1,0xffff0000);int valid=NearCondition(0x84);
                SystemStateGp();Patch(valid,position);Bytes(0x41,0x89,0x83);U32(688);NativeMemoryCount();
            }
            else if(op==FullSystemStateOperation.StoreMxcsr)
            {
                ScalarMiscCheckCanonical(i,32);NativeMemoryPrepare(i,FullMemoryAccess.Write,32);
                Bytes(0x45,0x8b,0x83);U32(688);NativeMemoryStore(32);NativeMemoryCount();
            }
            else if(op is FullSystemStateOperation.ReadFsBase or FullSystemStateOperation.ReadGsBase)
            {
                Bytes(0x4d,0x8b,0x83);U32(op==FullSystemStateOperation.ReadFsBase?528u:544u);
                if(i.Width==32)IntegerMov(8,8,32);
                Store(i.Destination,i.Width);
            }
            else if(op is FullSystemStateOperation.WriteFsBase or FullSystemStateOperation.WriteGsBase)
            {
                IntegerLoad(i.Source,i.Width,8);IntegerMov(1,8);IntegerShift(7,1,64,47);
                IntegerConstant(0,1,1,64);IntegerConstant(7,1,1,64);int valid=NearCondition(0x86);
                SystemStateGp();Patch(valid,position);
                bool fs=op==FullSystemStateOperation.WriteFsBase;
                Bytes(0x4d,0x89,0x83);U32(fs?528u:544u);IntegerFrameStore(fs?328:336,8);
            }
            else if(op==FullSystemStateOperation.CacheFlush)
            {
                ScalarMiscCheckCanonical(i,8);NativeMemoryPrepare(i,FullMemoryAccess.Read,8);
                // Generated guest data is already coherent host memory. The
                // cache hint has no additional guest byte effect; preserve its
                // ordering after the architecturally faulting access check.
                Bytes(0x0f,0xae,0xf0);NativeMemoryCount();
            }
            else Bytes(0x0f,0xae,op==FullSystemStateOperation.LoadFence?(byte)0xe8:
                op==FullSystemStateOperation.StoreFence?(byte)0xf8:(byte)0xf0);
            return true;
        }
    }
}
