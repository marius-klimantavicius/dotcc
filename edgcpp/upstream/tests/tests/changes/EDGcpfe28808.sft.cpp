//type:fp
//options_all:--c++23
//remark:cv-qualification in auto(x) and auto{x}
// 4/27/26  [EDGcpfe/28808]
//
// cv-qualification in auto(x) and auto{x}
//
// The front end previously erroneously preserved top-level cv-qualification on
// class prvalues produced by C++23 functional-notation casts to "auto".  That is
// now fixed.
struct S {};
S const s;
S &&r = auto(static_cast<S const&&>(s));  // Previously an error.
                                          // Now okay.
