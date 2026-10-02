//type:fp
//options_all:--g++
//remark:[4.8] GNU C++ compatibility: Instantiating a reference to array of unknown bound
// 8/1/13   [EDGcpfe/14353]
//
// GNU C++ compatibility: Instantiating a reference to array of unknown bound
//
// In GNU C++ mode, the front end now accepts a reference to an array of unknown
// bound when it occurs in a template instantiation context.
//
// (See also the entry of 6/14/13 for EDGcpfe/14129, which covers a variation of
// this behavior for Microsoft mode.)
struct S { template<typename T> S(T&); };
extern char const x[];
S s(x);  // Now accepted in GNU C++ mode.
