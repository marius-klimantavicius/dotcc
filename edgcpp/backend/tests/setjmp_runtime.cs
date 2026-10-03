using System;
using System.Threading;
using Edg.Runtime;

unsafe class Program
{
    static void Check(bool okay, string what) { if (!okay) throw new Exception(what); }
    static void Callee(NonlocalJump.Buffer* target, int value)
    {
        NonlocalJump.Activation child = default;
        NonlocalJump.Enter(&child, 1);
        try
        {
            try { NonlocalJump.LongJump(target, value); }
            catch (NonlocalJump.Transfer error) when (NonlocalJump.Matches(error, &child))
            { throw new Exception("wrong activation intercepted transfer"); }
        }
        finally { NonlocalJump.Leave(&child); }
    }
    static void Run()
    {
        NonlocalJump.Activation activation = default;
        NonlocalJump.Buffer* buffers = stackalloc NonlocalJump.Buffer[3];
        NonlocalJump.Enter(&activation, 3);
        try
        {
            for (int i = 0; i < 2; ++i) Check(NonlocalJump.Arm(&activation, buffers + i, 1) == 0, "initial return");
            NonlocalJump.Arm(&activation, buffers + 2, 2);
            int pc = 0, value = 0, attempts = 0;
            while (true)
            {
                try
                {
                    switch (pc)
                    {
                        case 0: Callee(buffers, 0); break;
                        case 1:
                            Check(value == (attempts == 0 ? 1 : 17), "return value/zero normalization");
                            if (++attempts < 3) Callee(buffers + 1, 17);
                            Callee(buffers + 2, -9); break;
                        case 2: Check(value == -9 && attempts == 3, "distinct continuation"); return;
                    }
                    throw new Exception("longjmp returned");
                }
                catch (NonlocalJump.Transfer error) when (NonlocalJump.Matches(error, &activation))
                { pc = (int)error.Site; value = error.Value; }
            }
        }
        finally { NonlocalJump.Leave(&activation); }
    }
    static void AllocationFree()
    {
        NonlocalJump.Activation activation = default; NonlocalJump.Buffer buffer = default;
        for (int i = 0; i < 100; ++i) { NonlocalJump.Enter(&activation, 2); NonlocalJump.Arm(&activation, &buffer, 1); NonlocalJump.Leave(&activation); }
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 20000; ++i)
        {
            NonlocalJump.Enter(&activation, 2);
            NonlocalJump.Arm(&activation, &buffer, 1); NonlocalJump.Arm(&activation, &buffer, 2);
            NonlocalJump.Leave(&activation);
        }
        Check(GC.GetAllocatedBytesForCurrentThread() == before, "setjmp activation/arming allocated managed storage");
    }
    static void Invalid(string mode)
    {
        NonlocalJump.Activation activation = default; NonlocalJump.Buffer buffer = default;
        if (mode == "unarmed") NonlocalJump.LongJump(&buffer, 1);
        NonlocalJump.Enter(&activation, 2); NonlocalJump.Arm(&activation, &buffer, 1);
        if (mode == "stale") { NonlocalJump.Leave(&activation); NonlocalJump.LongJump(&buffer, 1); }
        if (mode == "reuse")
        {
            NonlocalJump.Leave(&activation); NonlocalJump.Enter(&activation, 2);
            NonlocalJump.LongJump(&buffer, 1);
        }
        if (mode == "thread")
        {
            nint address = (nint)(&buffer);
            var thread = new Thread(() => NonlocalJump.LongJump((void*)address, 1)); thread.Start(); thread.Join();
        }
        if (mode == "site") { ((uint*)&buffer)[2] = 3; NonlocalJump.LongJump(&buffer, 1); }
        throw new Exception("invalid nonlocal jump did not terminate");
    }
    static void Main(string[] args)
    {
        if (args.Length != 0) Invalid(args[0]);
        Run(); Run(); AllocationFree();
        Exception? failure = null;
        var thread = new Thread(() => { try { Run(); AllocationFree(); } catch (Exception e) { failure = e; } });
        thread.Start(); thread.Join(); if (failure != null) throw failure;
        Console.WriteLine("nonlocal jump transport checks passed");
    }
}
