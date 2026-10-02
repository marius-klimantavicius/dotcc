//type:fp
//options_all:--c++11
//remark:[4.11] __is_pod
// 8/28/15  [EDGcpfe/16464]
//
// __is_pod
//
// Previously, a user-declared constructor always made a class type non-POD (the
// C++03 definition of POD requires a class type to be an aggregate type) and the
// __is_pod type trait helper reflected that.  Now, in C++11 mode, the C++11
// definition is used for __is_pod, and that definition does allow some
// constructors.
//
// The is_POD flag in a_class_symbol_supplement has been renamed to is_cpp03_POD.
// Except for the renaming, it is not affected by this change (it reflects the
// C++03 definition in all modes); this ensures that the ABI implementations are
// unaffected.  A new function is_pod_class is now available to test whether a
// class is a POD in the current language mode (i.e., in C++11 mode it tests for
// the C++11 definition of POD, and in other modes for the C++03 definition of
// POD).
struct POD {
  POD() = default;
  POD(int);
};
static_assert(__is_pod(POD), "Unexpected!");
     // Now accepted in C++11 modes that support the __is_pod helper.
