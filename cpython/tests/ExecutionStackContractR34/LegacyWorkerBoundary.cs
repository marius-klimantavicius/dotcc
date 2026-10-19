using static DotCC.Libc.Libc;
internal static unsafe class LegacyWorkerBoundary
{
    static void* Exit(void* argument)
    {
        if(RuntimeContext.Current!=null) return null;
        _Exit(37); // Real legacy process termination, confined to this child.
        return argument;
    }
    static int Main()
    {
        long thread;
        if(pthread_create(&thread,null,&Exit,null)!=0) return 2;
        pthread_join(thread,null);
        return 3; // Either an incorrect binding or an incorrectly returning _Exit.
    }
}
