//type:fp
//options_all:--gnu_version=90300 --c++17
//remark:[6.7] Spurious GNU-mode incomplete-type error for static data member of template
// 1/2/24   [EDGcpfe/25475,EDGcpfe/26903]
//
// Spurious GNU-mode incomplete-type error for static data member of template
//
// Previously, this triggered an error in GNU C++ mode complaining that the type
// of arr is incomplete.  That is now fixed.
template<typename> struct S {
  constexpr static int arr[]{};
  static long const L = sizeof(arr);
  static void f() { long length = L; }
};
void g() { S<void>::f(); }
