//type:fp
//options_all:--microsoft
//remark:[4.2] Microsoft C++ compatibility: Multiple extern "C" definitions
// 6/18/10  [EDGcpfe/10561]
//
// Microsoft C++ compatibility: Multiple extern "C" definitions
//
// In Microsoft C++ bugs mode, the front end now accepts multiple definitions of
// the same extern "C" function in different namespaces (each such definition
// is associated with a different a_routine entry).
//
// Cases like these are likely to result in a linker error, but if the functions
// are declared "inline", an error may be avoided.
namespace N { extern "C" void f() { } }
namespace M { extern "C" void f() { } }  // Previously always an error; now
                                         // accepted in Microsoft bugs mode.
