using System.Runtime.InteropServices;
using L = DotCC.Libc.Libc;

internal static unsafe class CloneFailure
{
    [DllImport("libresolver-clone-allocator-test.so",EntryPoint="resolver_test_arm")]
    private static extern void Arm(int after);
    [DllImport("libresolver-clone-allocator-test.so",EntryPoint="resolver_test_disarm")]
    private static extern void Disarm(int* calls,int* failures);
    [DllImport("libresolver-clone-allocator-test.so",EntryPoint="resolver_test_observe")]
    private static extern void Observe(int* arm,int* fault,void** caller);
    [DllImport("libresolver-clone-allocator-test.so",EntryPoint="resolver_test_caller_module")]
    private static extern byte* CallerModule();
    [DllImport("libc",EntryPoint="getaddrinfo")]
    private static extern int NativeResolve(byte* node,byte* service,void* hints,void* output);
    [DllImport("libc",EntryPoint="freeaddrinfo")]
    private static extern void NativeFree(void* output);
    public static int Run()
    {
        using var owner=new L.RuntimeContext(); using var binding=owner.Enter();
        byte* hints=stackalloc byte[48]; new Span<byte>(hints,48).Clear(); *(int*)hints=4;
        bool ok=true;
        fixed(byte* node="127.0.0.1\0"u8) fixed(byte* service="80\0"u8)
        {
            void* native=null; int nativeStatus=NativeResolve(node,service,hints,&native), nativeNodes=0;
            for(byte* p=(byte*)native;p!=null;p=*(byte**)(p+40)) nativeNodes++;
            NativeFree(native);
            void* warm=null; L.errno=123; if(L.getaddrinfo(node,service,hints,&warm)!=0) return 10;
            int warmError=L.errno,warmNodes=owner.NativeAllocations.Count; L.freeaddrinfo(warm);
            Console.WriteLine($"CLONECONTROL native_eai={nativeStatus} native_nodes={nativeNodes} copied_nodes={warmNodes} errno={warmError} remaining={owner.NativeAllocations.Count}");
            ok &= nativeStatus==0 && nativeNodes==3 && warmNodes==3 && warmError==123 && owner.NativeAllocations.Count==0;
            for(int after=0;after<2;after++)
            {
                void* output=(void*)1; int calls=0,failures=0; L.errno=123;
                Arm(after);
                int status=L.getaddrinfo(node,service,hints,&output);
                Disarm(&calls,&failures);
                int error=L.errno, remaining=owner.NativeAllocations.Count;
                int arm=0,fault=0; void* caller=null; Observe(&arm,&fault,&caller);
                string module=Marshal.PtrToStringUTF8((nint)CallerModule())!;
                Console.WriteLine($"CLONEOOM after={after} matched={calls} failures={failures} eai={status} errno={error} null={output==null} remaining={remaining} arm_tid={arm} fault_tid={fault} caller=0x{(nuint)caller:x} module={module}");
                ok &= calls==after+1 && failures==1 && status==-10 && error==12 && output==null && remaining==0;
                ok &= arm==fault && arm!=0 && caller!=null && !module.Contains("libc.so") && !module.Contains("libcoreclr");
                L.freeaddrinfo(output);
            }
        }
        return ok?0:1;
    }
}
