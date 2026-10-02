//type:fp
//options_all:--c++23
//remark:[6.4] C++23: auto(x) and auto {x}
// 5/11/22  [EDGcpfe/25075]
//
// C++23: auto(x) and auto {x}
//
// In C++23 mode, the front end now accepts functional-style casts to type "auto",
// which primarily has the effect of converting glvalues to prvalues, including
// decaying array and function glvalues to pointer prvalues.
//
// This feature was added to C++23 through the standardization committee's
// paper P0849R8.  The macro __cpp_auto_cast is defined with the value 202110L
// when the feature is enabled.
struct S {};
S& g();
int f(S&);  // (1)
int f(S&&); // (2)
int x = f(g()),        // Calls (1).
    y = f(auto(g()));  // Calls (2), materializing a temporary.
