using System.Runtime.InteropServices;
using System.Text;

namespace DotObj;

internal sealed partial class Sqlite
{
    // Never opens an existing path for writing, and never changes ObjectFile's
    // readonly/query-only connection. A failed import remains available as evidence.
    internal static Sqlite CreateCatalog(string path)
    {
        using(var reservation=new FileStream(path,FileMode.CreateNew,FileAccess.Write,FileShare.None)) {}
        return new Sqlite(path,true);
    }
    internal static Sqlite OpenCatalogForResolution(string path) => new(path, true);
    internal sealed partial class Statement
    {
        public void Bind(params object?[] values)
        {
            if(values.Length!=Native.sqlite3_bind_parameter_count(statement))
                throw new InvalidOperationException("Catalog SQL binding count mismatch");
            for(int index=0;index<values.Length;index++)
            {
                int ordinal=index+1;int status;
                switch(values[index])
                {
                    case null: status=Native.sqlite3_bind_null(statement,ordinal);break;
                    case string text:
                        byte[] utf8=Encoding.UTF8.GetBytes(text+"\0");
                        status=Native.sqlite3_bind_text(statement,ordinal,utf8,utf8.Length-1,(nint)(-1));break;
                    case byte[] bytes: status=bytes.Length==0?Native.sqlite3_bind_zeroblob(statement,ordinal,0):
                        Native.sqlite3_bind_blob(statement,ordinal,bytes,bytes.Length,(nint)(-1));break;
                    case bool flag: status=Native.sqlite3_bind_int64(statement,ordinal,flag?1:0);break;
                    case int number: status=Native.sqlite3_bind_int64(statement,ordinal,number);break;
                    case long number: status=Native.sqlite3_bind_int64(statement,ordinal,number);break;
                    default: throw new InvalidOperationException("Unsupported catalog SQL parameter");
                }
                if(status!=0)throw new InvalidDataException(owner.Error);
            }
        }
        public void ExecuteBound(params object?[] values)
        {
            Bind(values);
            if(Read())throw new InvalidDataException("Catalog update unexpectedly returned a row");
            if(Native.sqlite3_reset(statement)!=0 || Native.sqlite3_clear_bindings(statement)!=0)
                throw new InvalidDataException(owner.Error);
        }
    }
    private static partial class Native
    {
        [DllImport(Library,CallingConvention=CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_parameter_count(nint statement);
        [DllImport(Library,CallingConvention=CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_null(nint statement,int ordinal);
        [DllImport(Library,CallingConvention=CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_int64(nint statement,int ordinal,long value);
        [DllImport(Library,CallingConvention=CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_text(nint statement,int ordinal,byte[] value,int length,nint destructor);
        [DllImport(Library,CallingConvention=CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_blob(nint statement,int ordinal,byte[] value,int length,nint destructor);
        [DllImport(Library,CallingConvention=CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_zeroblob(nint statement,int ordinal,int length);
        [DllImport(Library,CallingConvention=CallingConvention.Cdecl)]
        internal static extern int sqlite3_reset(nint statement);
        [DllImport(Library,CallingConvention=CallingConvention.Cdecl)]
        internal static extern int sqlite3_clear_bindings(nint statement);
    }
}
