using System;
using Edg.Runtime;

static unsafe class ArrayProbe
{
    static int* start;
    static int constructorFailure, destructorFailure;
    static string mode = "";
    sealed class Failure(int value) : Exception { public readonly int Value = value; }
    static void Trace(string text) { Console.Write(text); Console.Out.Flush(); }
    static void Construct(void* pointer)
    {
        int index = (int)((int*)pointer - start);
        Trace($"c{index},");
        if (index == constructorFailure)
        {
            if (mode == "jump") throw new NonlocalJump.Transfer(1, 1, 1, default);
            if (mode == "thread") throw new CThreadExitTransfer(new object(), 1, 7);
            throw new Failure(17);
        }
        *(int*)pointer = index + 10;
    }
    static void Destroy(void* pointer)
    {
        int index = (int)((int*)pointer - start);
        if (*(int*)pointer != index + 10) throw new Failure(99);
        Trace($"d{index},");
        try
        {
            if ((mode == "fatal-ctor" && index == 1) || (mode == "fatal-dtor" && index == 2))
                throw new Failure(29);
            if (index == destructorFailure) { destructorFailure = -1; throw new Failure(23); }
        }
        finally
        {
            if ((mode == "fatal-ctor" && index == 1) || (mode == "fatal-dtor" && index == 2))
                Trace("BUG:cleanup-unwound,");
        }
    }
    static int Main(string[] arguments)
    {
        int* values = stackalloc int[4]; start = values;
        mode = arguments.Length == 0 ? "" : arguments[0];
        if (mode is "jump" or "thread" or "fatal-ctor")
        {
            constructorFailure = 2; destructorFailure = -1;
            try { DotObjArrays.__edg_vec_ctor(values, 4, sizeof(int), &Construct, &Destroy); return 10; }
            catch (NonlocalJump.Transfer) when (mode == "jump") { Console.WriteLine("jump,"); return 0; }
            catch (CThreadExitTransfer) when (mode == "thread") { Console.WriteLine("thread,"); return 0; }
        }
        if (mode == "fatal-dtor")
        {
            for (int i = 0; i != 4; ++i) values[i] = i + 10;
            destructorFailure = 3;
            DotObjArrays.__edg_vec_dtor(values, 4, sizeof(int), &Destroy);
            return 11;
        }
        for (int failure = -1; failure != 4; ++failure)
        {
            constructorFailure = failure; destructorFailure = -1;
            Trace($"construct{failure}:");
            try
            {
                DotObjArrays.__edg_vec_ctor(values, 4, sizeof(int), &Construct, &Destroy);
                DotObjArrays.__edg_vec_dtor(values, 4, sizeof(int), &Destroy);
            }
            catch (Failure error) { if (error.Value != 17) return 1; Trace("caught,"); }
            Console.WriteLine();
        }
        for (int failure = 0; failure != 4; ++failure)
        {
            for (int i = 0; i != 4; ++i) values[i] = i + 10;
            destructorFailure = failure; Trace($"destroy{failure}:");
            try { DotObjArrays.__edg_vec_dtor(values, 4, sizeof(int), &Destroy); return 2; }
            catch (Failure error) { if (error.Value != 23) return 3; Console.WriteLine("caught,"); }
        }
        DotObjArrays.__edg_vec_ctor(null, 0, sizeof(int), &Construct, &Destroy);
        DotObjArrays.__edg_vec_dtor(null, 0, sizeof(int), &Destroy);
        DotObjArrays.__edg_vec_ctor(null, 4, sizeof(int), null, null);
        DotObjArrays.__edg_vec_dtor(null, 4, sizeof(int), null);
        Console.WriteLine("empty/null callbacks OK");
        return 0;
    }
}
