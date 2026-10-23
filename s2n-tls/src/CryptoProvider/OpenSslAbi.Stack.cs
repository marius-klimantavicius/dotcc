namespace Managed.S2nTls.Crypto;

public static unsafe partial class OpenSslAbi
{
    private sealed class StackState {public readonly List<nint> Values=new();}
    public static void* OPENSSL_sk_new_null(){try{return Add(new StackState());}catch{Error();return null;}}
    public static void OPENSSL_sk_free(void* stack){try{Remove(stack);}catch{Error();}}
    public static int OPENSSL_sk_num(void* stack){if(stack==null)return -1;try{return Get<StackState>(stack).Values.Count;}catch{Error();return -1;}}
    public static void* OPENSSL_sk_value(void* stack,int index)
    {try{if(stack==null)return null;var values=Get<StackState>(stack).Values;return index<0||index>=values.Count?null:(void*)values[index];}catch{Error();return null;}}
    public static int OPENSSL_sk_push(void* stack,void* value)
    {try{var values=Get<StackState>(stack).Values;values.Add((nint)value);return values.Count;}catch{Error();return 0;}}
    public static void OPENSSL_sk_pop_free(void* stack,delegate*<void*,void> free)
    {
        try
        {
            if(stack==null)return;var values=Get<StackState>(stack).Values;
            if(free!=null)for(int i=values.Count-1;i>=0;i--)if(values[i]!=0)free((void*)values[i]);
            values.Clear();Remove(stack);
        }
        catch{Error();}
    }
}
