//type:fp
//options_all:--g++ --c++11
//remark:[4.11] VLAs and constexpr constructors
// 11/30/15 [EDGcpfe/16680]
//
// VLAs and constexpr constructors
//
// The front end previously aborted with an internal error in num_array_elements
// ("array with unknown bound"; in types.c) when processing a VLA declaration
// whose elements are constructed using a constexpr constructor.
// with --g++ --c++11:
//
// This is now fixed (the constructor call is not constant-folded in such cases).
struct S { constexpr S() {} };
void g(int n) {
  S vla[n]; // Previously triggered an internal error; now okay.
}
