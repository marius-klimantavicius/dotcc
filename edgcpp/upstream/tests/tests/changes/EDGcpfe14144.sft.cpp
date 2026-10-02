//type:fp
//options_all:--c++14
//remark:[4.9] C++14: Deduced return types
// 2/21/14  [EDGcpfe/14144,EDGcpfe/14633]
//
// C++14: Deduced return types
//
// In C++14 mode the front end now accepts functions with deduced return types.
//
// This change to the C++ language was introduced by the committee's paper N3638.
auto forty_two() { return 42; }  // Return type deduced to "int".
auto pointer_42()->auto* {       // The first "auto" announces the trailing
  return forty_two;              // return type.  The second auto is the
}                                // placeholder type to be deduced.
