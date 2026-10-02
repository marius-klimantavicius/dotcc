//type:fp
//options_all:--c++11
//remark:[4.11] Function calls and decltype
// 1/19/16  [EDGcpfe/16785]
//
// Function calls and decltype
//
// Early drafts of the decltype feature specified that when applied to a call
// expression it produced the return type of the function.  This was later
// changed (before final standardization in C++11) but the front end was never
// updated, causing nonstandard results in some cases.
//
// This is now fixed.
template<typename T> T f();
decltype(f<int const>()) x; // Previously a spurious error because x was
                            // declared with type "int const".  Now okay;
                            // the type of x is "int".
