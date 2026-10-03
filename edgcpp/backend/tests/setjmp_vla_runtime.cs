using System;
using Edg.Runtime;

unsafe class Program
{
    static void Check(bool okay,string what){if(!okay)throw new Exception(what);}
    static void Child(NonlocalJump.Buffer* environment,int value)
    {
        nint mark=Vla.Mark();NonlocalJump.Activation activation=default;
        NonlocalJump.Enter(&activation,1);
        try {void* array=null;Vla.Allocate(&array,4096,64);NonlocalJump.LongJump(environment,value);}
        finally {NonlocalJump.Leave(&activation);Vla.Restore(mark);}
    }
    static void Run(bool before,bool bufferInArray)
    {
        nint entry=Vla.Mark();NonlocalJump.Activation activation=default;
        NonlocalJump.Buffer stackBuffer=default;NonlocalJump.Buffer* environment=&stackBuffer;
        int* surviving=null;
        if(before){Vla.Allocate(&surviving,32,16);*surviving=41;}
        if(bufferInArray){void* array=null;Vla.Allocate(&array,(long)sizeof(NonlocalJump.Buffer)*3,32);environment=(NonlocalJump.Buffer*)array+1;}
        nint checkpoint=Vla.Mark();NonlocalJump.Enter(&activation,1);
        try
        {
            NonlocalJump.Arm(&activation,environment,1);int pc=0,value=0,visits=0;
            for(;;)
            {
                try
                {
                    if(pc==1)
                    {
                        Check(Vla.Mark()==checkpoint,"destination did not restore exact checkpoint");
                        Check(!before || *surviving==41,"VLA live at setjmp was reclaimed");
                        Check(environment->Version==2,"VLA-resident jump buffer was reclaimed");
                        Check(value==(visits==0?1:17),"jump value");
                        if(++visits==4)return;
                    }
                    void* array=null;Vla.Allocate(&array,1234,32);((byte*)array)[0]=73;
                    Child(environment,visits==0?0:17);
                }
                catch(NonlocalJump.Transfer transfer) when(NonlocalJump.Matches(transfer,&activation))
                {value=NonlocalJump.Resume(transfer,&activation);pc=(int)transfer.Site;}
            }
        }
        finally {NonlocalJump.Leave(&activation);Vla.Restore(entry);}
    }
    static void AllocationFree()
    {
        NonlocalJump.Activation activation=default;NonlocalJump.Buffer environment=default;
        NonlocalJump.Enter(&activation,1);
        try
        {
            for(int i=0;i<100;++i)NonlocalJump.Arm(&activation,&environment,1);
            long before=GC.GetAllocatedBytesForCurrentThread();
            for(int i=0;i<20000;++i)NonlocalJump.Arm(&activation,&environment,1);
            Check(GC.GetAllocatedBytesForCurrentThread()==before,"checkpoint capture allocated managed storage");
        }
        finally {NonlocalJump.Leave(&activation);}
    }
    static void Invalid(string mode)
    {
        NonlocalJump.Activation activation=default;NonlocalJump.Buffer environment=default;
        NonlocalJump.Enter(&activation,1);void* array=null;Vla.Allocate(&array,128,16);
        NonlocalJump.Arm(&activation,&environment,1);
        if(mode=="escaped")Vla.Release(array);
        if(mode=="generation")++environment.VlaIdentity;
        if(mode=="version")environment.Version=1;
        if(mode=="resume")
        {
            try {NonlocalJump.LongJump(&environment,1);}
            catch(NonlocalJump.Transfer transfer) when(NonlocalJump.Matches(transfer,&activation))
            {Vla.Release(array);NonlocalJump.Resume(transfer,&activation);}
        }
        else NonlocalJump.LongJump(&environment,1);
        throw new Exception("invalid checkpoint returned");
    }
    static void Main(string[] args)
    {
        Check(sizeof(NonlocalJump.Buffer)==32,"version2 buffer ABI");
        if(args.Length!=0)Invalid(args[0]);
        nint entry=Vla.Mark();int* caller=null;Vla.Allocate(&caller,16,16);*caller=99;
        nint callerMark=Vla.Mark();
        for(int flags=0;flags<4;++flags)
        {
            Run((flags&1)!=0,(flags&2)!=0);
            Check(Vla.Mark()==callerMark && *caller==99,"caller VLA lifetime");
        }
        AllocationFree();Vla.Restore(entry);Check(Vla.Mark()==entry,"activation VLA reclamation");
        Console.WriteLine("nonlocal VLA checkpoint checks passed");
    }
}
