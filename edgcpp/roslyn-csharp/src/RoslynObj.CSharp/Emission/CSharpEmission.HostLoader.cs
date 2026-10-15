using System.Text;
using System.Text.Json;
using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private readonly List<NativeImport> nativeLoaderImports = [];
        private string? logicalDlerrorEntry;
        private string? loaderRegistrationEntry;
        private bool HostLoaderEnabled => HostProtocolEnabled && nativeLoaderImports.Count != 0;

        private void RequireHostLoaderImport(NativeImport import)
        {
            if (!HostProtocolEnabled) return;
            var name = import.Function.GnuSupplement?.AssemblyName ?? import.Function.CurrentName;
            if (name is not ("dlopen" or "dlsym" or "dlclose" or "dlerror")) return;
            if (import.Binding.EntryPoint != name)
                throw Limit("The selected loader provider requires its exact recorded native entry point.", "host-loader");
            var signature = import.Function.Signature;
            ValidateNativeScalarSignature(signature);
            var parameters = signature.Parameters.Select(parameter => Unwrap(parameter.Type)).ToArray();
            var result = Unwrap(signature.ReturnType);
            bool Pointer(CppType type) => type is PointerType pointer && Unwrap(pointer.PointeeType).Kind.Name == "tk_void";
            bool Bytes(CppType type) => type is PointerType pointer && Unwrap(pointer.PointeeType) is
                ArithmeticType { Arithmetic.Bits: 8, Arithmetic.Signed: true };
            bool Int32(CppType type) => type is ArithmeticType { Arithmetic.Bits: 32, Arithmetic.Signed: true };
            var accepted = !signature.IsVariadic && (name switch
            {
                "dlerror" => parameters.Length == 0 && Bytes(result),
                "dlopen" => parameters.Length == 2 && Bytes(parameters[0]) && Int32(parameters[1]) && Pointer(result),
                "dlsym" => parameters.Length == 2 && Pointer(parameters[0]) && Bytes(parameters[1]) && Pointer(result),
                "dlclose" => parameters.Length == 1 && Pointer(parameters[0]) && Int32(result),
                _ => false
            });
            if (!accepted) throw Limit("The selected loader signature is outside the exercised ordinary Linux scalar/pointer contract.", "host-loader");
            if (nativeLoaderImports.Any(item => item.Function.Equals(import.Function))) return;
            nativeLoaderImports.Add(import);
            _ = RequireGeneratedAbiLibrary();
            logicalDlerrorEntry ??= InternalName("__roslynobj_application_dlerror");
            loaderRegistrationEntry ??= InternalName("__roslynobj_register_loader_entries");
        }

        private bool IsLogicalDlerrorProvider(FunctionSymbol function)
            => HostProtocolEnabled && nativeLoaderImports.Any(item => item.Function.Equals(function) &&
                (item.Function.GnuSupplement?.AssemblyName ?? item.Function.CurrentName) == "dlerror");

        private void EmitHostLoaderSupport(CodeWriter initialization)
        {
            if (!HostLoaderEnabled) return;
            if (!nativeLoaderImports.Any(item => (item.Function.GnuSupplement?.AssemblyName ?? item.Function.CurrentName) == "dlerror"))
                throw Limit("The selected loader protocol requires its captured, explicitly bound dlerror provider.", "host-loader");
            methods.Append("    [DllImport(\"").Append(NativeAbiLibraryName).Append("\", EntryPoint = \"").Append(loaderRegistrationEntry)
                .AppendLine("\", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]")
                .Append("    private static extern void ").Append(loaderRegistrationEntry)
                .AppendLine("(nint open, nint symbol, nint close, nint error);").AppendLine();
            var arguments = new[] { "dlopen", "dlsym", "dlclose", "dlerror" }.Select(name =>
            {
                var import = nativeLoaderImports.SingleOrDefault(item => (item.Function.GnuSupplement?.AssemblyName ?? item.Function.CurrentName) == name);
                return import is null ? "0" : $"NativeLibrary.GetExport({import.LibraryField}, {JsonSerializer.Serialize(import.Binding.EntryPoint)})";
            });
            initialization.Line($"{Owner}.{loaderRegistrationEntry}({string.Join(", ", arguments)});");
        }

        private void AppendHostLoaderOutgoingStart(StringBuilder source)
        {
            if (!HostLoaderEnabled) return;
            source.AppendLine("    address = __roslynobj_loader_address(address);")
                .AppendLine("    int const __loader_call = __roslynobj_loader_operation(address);")
                .AppendLine("    if (__loader_call) __roslynobj_loader_prepare();");
        }

        private void AppendHostLoaderOutgoingEnd(StringBuilder source)
        {
            if (!HostLoaderEnabled) return;
            source.AppendLine("    if (__loader_call) {")
                .AppendLine("        __roslynobj_loader_capture();")
                .AppendLine("        *__native_error = __captured_error;")
                .AppendLine("    }");
        }

        private void AppendHostLoaderPreamble(StringBuilder source)
        {
            if (!HostLoaderEnabled) return;
            source.AppendLine("#include <dlfcn.h>").AppendLine("#include <pthread.h>").AppendLine("#include <string.h>");
            source.AppendLine("/* Selected glibc loader contract: error byte snapshots and ABI forwarding only. */");
            source.AppendLine("ABI_EXPORT char *" + logicalDlerrorEntry + "(void);");
            source.AppendLine("""
#if !defined(__GLIBC__)
#error The selected loader contract requires the proven glibc provider.
#endif
static void *__roslynobj_loader_open;
static void *__roslynobj_loader_symbol;
static void *__roslynobj_loader_close;
static void *__roslynobj_loader_error;
struct __roslynobj_loader_state {
    char *bytes;
    size_t capacity;
    int pending;
};
static _Thread_local struct __roslynobj_loader_state *__roslynobj_loader_tls;
static pthread_once_t __roslynobj_loader_once = PTHREAD_ONCE_INIT;
static pthread_key_t __roslynobj_loader_key;
static void __roslynobj_loader_destroy(void *value) {
    struct __roslynobj_loader_state *state = value;
    if (__roslynobj_loader_tls == state) __roslynobj_loader_tls = NULL;
    free(state->bytes);
    free(state);
}
static void __roslynobj_loader_create_key(void) {
    if (pthread_key_create(&__roslynobj_loader_key, __roslynobj_loader_destroy) != 0) abort();
}
static struct __roslynobj_loader_state *__roslynobj_loader_state(void) {
    if (__roslynobj_loader_tls) return __roslynobj_loader_tls;
    if (pthread_once(&__roslynobj_loader_once, __roslynobj_loader_create_key) != 0) abort();
    struct __roslynobj_loader_state *state = calloc(1, sizeof(*state));
    if (!state || pthread_setspecific(__roslynobj_loader_key, state) != 0) abort();
    __roslynobj_loader_tls = state;
    return state;
}
static int __roslynobj_loader_operation(void *address) {
    return address && (address == __roslynobj_loader_open || address == __roslynobj_loader_symbol || address == __roslynobj_loader_close);
}
static char *__roslynobj_loader_read_native(void) {
    if (!__roslynobj_loader_error) abort();
    return ((char *(*)(void))__roslynobj_loader_error)();
}
static void __roslynobj_loader_prepare(void) {
    struct __roslynobj_loader_state *state = __roslynobj_loader_state();
    state->pending = 0;
    (void)__roslynobj_loader_read_native();
}
static void __roslynobj_loader_capture(void) {
    struct __roslynobj_loader_state *state = __roslynobj_loader_tls;
    if (!state) abort();
    char const *message = __roslynobj_loader_read_native();
    state->pending = 0;
    if (!message) return;
    size_t length = strlen(message);
    if (length == SIZE_MAX) abort();
    if (state->capacity < length + 1) {
        char *bytes = realloc(state->bytes, length + 1);
        if (!bytes) abort();
        state->bytes = bytes;
        state->capacity = length + 1;
    }
    memcpy(state->bytes, message, length + 1);
    state->pending = 1;
}
""");
            source.AppendLine("ABI_EXPORT char *" + logicalDlerrorEntry + "(void) {")
                .AppendLine("    int *const native_error = __errno_location();")
                .AppendLine("    int const saved_error = *native_error;")
                .AppendLine("    struct __roslynobj_loader_state *state = __roslynobj_loader_tls;")
                .AppendLine("    char *result = state && state->pending ? state->bytes : NULL;")
                .AppendLine("    if (state) state->pending = 0;")
                .AppendLine("    *native_error = saved_error;")
                .AppendLine("    return result;")
                .AppendLine("}");
            source.AppendLine("static void *__roslynobj_loader_address(void *address) {")
                .Append("    if (__roslynobj_loader_error && address == __roslynobj_loader_error) return (void *)&").Append(logicalDlerrorEntry).AppendLine(";")
                .AppendLine("    return address;")
                .AppendLine("}");
            source.AppendLine("ABI_EXPORT void " + loaderRegistrationEntry + "(void *open, void *symbol, void *close, void *error) {")
                .AppendLine("    __roslynobj_loader_open = open;")
                .AppendLine("    __roslynobj_loader_symbol = symbol;")
                .AppendLine("    __roslynobj_loader_close = close;")
                .AppendLine("    __roslynobj_loader_error = error;")
                .AppendLine("}");
        }
    }
}
