//type:fn
//options_all:--microsoft
//remark:[5.0] __edg_wchar_type__ now considered a type
// 9/25/17  [EDGcpfe/18448,EDGcpfe/18790]
//
// __edg_wchar_type__ now considered a type
//
// The changes for EDGcpfe/18155 introduced __edg_wchar_type__ as a new type,
// internal to the front end, used in declarations of builtin functions.
// That type was not, however, interpreted as a type in some cases resulting
// in errors (and even an assertion failure in enter_builtin_function) when
// using a builtin function with a wchar_t type.
extern "C" void __cdecl __annotation(wchar_t*, ...);
