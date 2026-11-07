using System.Numerics;
using Blink=Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

internal static unsafe partial class FullNativeMemory
{
    internal static bool IsMaskedStoreRequest(FullBlockFrame* frame) => frame!=null&&frame->StackMemory!=null&&
        ((FullMaskedStoreDescriptor*)frame->StackMemory)->TagVersion==FullMaskedStoreDescriptor.CurrentTagVersion&&
        ((FullMaskedStoreDescriptor*)frame->StackMemory)->Kind==FullMaskedStoreDescriptor.SparseKind;

    // Identity validation, not implementation of a guest store. Generated code
    // computed the mask and snapshots; these checks bind them to the exact plan
    // and still-current worker state before any pointer can be published.
    internal static bool AuthenticateMaskedStore(FullBlockFrame* frame,in JitInstruction instruction,Blink.Machine* machine)
    {
        if(frame==null||machine==null||frame->Machine!=machine||!IsMaskedStoreRequest(frame)||
            !FullBlockEmitter.CanInlineMaskedStore(instruction))return false;
        try{FullBlockEmitter.ValidateMaskedStore(instruction);}catch(ArgumentException){return false;}
        var d=(FullMaskedStoreDescriptor*)frame->StackMemory;
        if(d->Site!=frame->ResumeIndex||d->Ip!=instruction.Ip||machine->ip!=instruction.Ip||
            d->Shape!=FullBlockEmitter.MaskedStoreShape(instruction)||
            d->State is <FullMaskedStoreState.Request or >FullMaskedStoreState.TicketReady)return false;
        ulong address=((ulong*)((byte*)machine+24))[7];
        if(instruction.Address.AddressWidth==32)address=(uint)address;
        if(instruction.Address.Segment==4)address=unchecked(address+machine->fs.@base);
        if(instruction.Address.Segment==5)address=unchecked(address+machine->gs.@base);
        if(d->BaseEA!=address)return false;
        byte* source=(byte*)machine+160+instruction.Source.Register*16;
        byte* mask=(byte*)machine+160+instruction.Extra.Register*16;
        if(d->DataLo!=*(ulong*)source||d->MaskLo!=*(ulong*)mask||
            d->DataHi!=(instruction.Width==128?*(ulong*)(source+8):0)||
            d->MaskHi!=(instruction.Width==128?*(ulong*)(mask+8):0))return false;
        uint enabled=0;for(int index=0;index<instruction.Width/8;++index)
            if((mask[index]&128)!=0)enabled|=1u<<index;
        if(enabled==0||d->EnabledBits!=enabled)return false;
        SelectedMaskedPages(in *d,out ulong page0,out ulong page1,out uint selected0,out uint selected1);
        return d->Page0==page0&&d->Page1==page1&&d->Selected0==selected0&&d->Selected1==selected1;
    }

    private static void SelectedMaskedPages(in FullMaskedStoreDescriptor d,out ulong page0,out ulong page1,
        out uint selected0,out uint selected1)
    {
        page0=page1=0;selected0=selected1=0;
        for(int index=0;index<16;++index)
        {
            uint bit=1u<<index;if((d.EnabledBits&bit)==0)continue;
            ulong page=unchecked(d.BaseEA+(uint)index)&~4095UL;
            if(selected0==0||page==page0){page0=page;selected0|=bit;}
            else{page1=page;selected1|=bit;}
        }
    }

    private static void ValidateMaskedStoreRequest(FullBlockFrame* f)
    {
        if(f==null||f->Machine==null||!IsMaskedStoreRequest(f))throw new InvalidOperationException("Missing sparse native request.");
        var d=(FullMaskedStoreDescriptor*)f->StackMemory;uint bytes=d->Shape&255;
        const uint allowedShape=0x010fffff;
        if(d->State!=FullMaskedStoreState.Request||d->Site!=f->ResumeIndex||d->Ip!=((Blink.Machine*)f->Machine)->ip||
            f->MemoryResume!=1||f->MemorySite!=f->ResumeIndex||f->MemoryAccess!=FullMemoryAccess.Write||f->MemoryWidth!=8||
            bytes is not(8 or 16)||(d->Shape&~allowedShape)!=0||
            (d->Shape&FullMaskedStoreDescriptor.NoDataAddressFaultsForZeroMask)==0||
            ((d->Shape>>17)&7)>6||bytes==8&&(((d->Shape>>8)&15)>7||((d->Shape>>12)&15)>7||d->DataHi!=0||d->MaskHi!=0)||
            d->EnabledBits==0||(d->EnabledBits&~((1u<<(int)bytes)-1))!=0)
            throw new InvalidOperationException("Invalid sparse native request grammar.");
        uint maskBits=0;
        for(int index=0;index<bytes;++index)
        {
            ulong mask=index<8?d->MaskLo:d->MaskHi;
            if(((mask>>(8*(index&7)))&128)!=0)maskBits|=1u<<index;
        }
        if(maskBits!=d->EnabledBits)throw new InvalidOperationException("Sparse mask snapshot differs from selected bytes.");
        SelectedMaskedPages(in *d,out ulong page0,out ulong page1,out uint selected0,out uint selected1);
        if(d->Page0!=page0||d->Page1!=page1||d->Selected0!=selected0||d->Selected1!=selected1)
            throw new InvalidOperationException("Invalid sparse native selected pages.");
    }

    private static FullMemoryAssistResult ResolveMaskedStore(Blink p,Blink.Machine* machine,FullBlockFrame* f,
        FullCodeAuthority authority,ref FullMaskedStoreDescriptor d,ulong epoch,bool permissions,out bool barrier)
    {
        barrier=false;
        // Canonicality of EVERY enabled byte precedes all PTE/host queries.
        // Disabled bytes and their pages are never probed or materialized.
        for(int index=0;index<16;++index)
        {
            if((d.EnabledBits&(1u<<index))==0)continue;
            ulong address=unchecked(d.BaseEA+(uint)index);
            if(d.BaseEA>ulong.MaxValue-(uint)index||!Canonical(address))
            {
                f->FaultTrap=((d.Shape>>17)&7)==3?12u:13u;
                f->FaultCode=128;f->FaultAddress=0;f->MemoryResume=0;
                return FullMemoryAssistResult.GuestFault;
            }
        }
        f->MemoryAddress=unchecked(d.BaseEA+(uint)BitOperations.TrailingZeroCount(d.Selected0));
        var result=ResolveStackPage(p,machine,f,authority,f->MemoryAddress,epoch,permissions,ref barrier,out byte* host0);
        if(result!=FullMemoryAssistResult.Resolved)return result;
        byte* host1=null;
        if(d.Selected1!=0)
        {
            f->MemoryAddress=unchecked(d.BaseEA+(uint)BitOperations.TrailingZeroCount(d.Selected1));
            result=ResolveStackPage(p,machine,f,authority,f->MemoryAddress,epoch,permissions,ref barrier,out host1);
            if(result!=FullMemoryAssistResult.Resolved)return result;
        }
        d.HostPage0=host0;d.HostPage1=host1;
        return FullMemoryAssistResult.Resolved;
    }

    private static FullMemoryAssistResult TryFillMaskedStore(Blink p,Blink.Machine* machine,FullBlockFrame* f,FullCodeAuthority authority)
    {
        ValidateMaskedStoreRequest(f);++f->MemoryAssists;
        var value=*(FullMaskedStoreDescriptor*)f->StackMemory;ulong epoch=authority.Epoch;
        var result=ResolveMaskedStore(p,machine,f,authority,ref value,epoch,true,out bool barrier);
        if(result!=FullMemoryAssistResult.Resolved)return result;
        if(authority.Epoch!=epoch||f->ObservedGlobalEpoch!=epoch)return FullMemoryAssistResult.Retry;
        if(barrier)return FullMemoryAssistResult.WriteBarrier;
        value.Epoch=epoch;value.Ticket=0;value.State=FullMaskedStoreState.Ready;
        *(FullMaskedStoreDescriptor*)f->StackMemory=value;f->MemoryEpoch=epoch;
        return FullMemoryAssistResult.Resolved;
    }

    internal static FullMemoryAssistResult InspectMaskedStoreTicket(Blink p,Blink.Machine* machine,FullBlockFrame* f,
        FullCodeAuthority authority,out nint page0,out nint page1)
    {
        ValidateMaskedStoreRequest(f);page0=page1=0;
        var value=*(FullMaskedStoreDescriptor*)f->StackMemory;ulong epoch=authority.Epoch;
        var result=ResolveMaskedStore(p,machine,f,authority,ref value,epoch,false,out _);
        if(result!=FullMemoryAssistResult.Resolved)return result;
        if(authority.Epoch!=epoch||f->ObservedGlobalEpoch!=epoch)return FullMemoryAssistResult.Retry;
        page0=(nint)value.HostPage0;page1=(nint)value.HostPage1;
        // Inspection alone never makes these pointers executable.
        value.Epoch=0;value.Ticket=0;value.State=FullMaskedStoreState.Request;
        *(FullMaskedStoreDescriptor*)f->StackMemory=value;
        return FullMemoryAssistResult.Resolved;
    }

    internal static void PublishMaskedStoreTicket(FullBlockFrame* f,ulong epoch,ulong ticket)
    {
        ValidateMaskedStoreRequest(f);var d=(FullMaskedStoreDescriptor*)f->StackMemory;
        if(ticket==0||epoch!=f->ObservedGlobalEpoch||d->HostPage0==null||
            (d->Selected1!=0)!=(d->HostPage1!=null))throw new InvalidOperationException("Invalid sparse write ticket.");
        d->Epoch=epoch;d->Ticket=ticket;d->State=FullMaskedStoreState.TicketReady;
        f->MemoryEpoch=epoch;f->MemoryFlags=FullMemoryFlags.BarrierTicket;
    }
}
