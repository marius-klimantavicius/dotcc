//type:fn
//options_all:--c++17
//remark:[4.14] Abort in expand_statement_inline
// 6/2/17   [EDGcpfe/18435]
//
// Abort in expand_statement_inline
//
// A segfault (in expand_statement_inline) could occur in some cases when inlining
// a function that has an empty block statement.
template <int i> bool f();
struct A {
    int x = f<[]{ return 29; {} }()>();  // The 4.14 change stopped an abort
                                         // here; later versions diagnose a
                                         // lambda in this context.
};
