//type:fn
//options_all:--gn 110200 --c++17
//remark:[6.4] GNU compatibility: Deducing "T&" to "this"
// 6/27/22  [EDGcpfe/25274]
//
// GNU compatibility: Deducing "T&" to "this"
//
// Older versions of GCC included a nonstandard behavior that caused them to match
// a parameter of type T& (where T is a template parameter) to a "this" argument
// (which is normally invalid since "this" is a prvalue), but in doing so GCC
// deduced T to be a const type.  The changes for EDGcpfe/9874 made the front end
// emulate this behavior in all GNU and Clang C++ modes.  Now that emulation is
// limited to non-Clang GNU modes with gnu_version < 40900.
template<typename T> void g(T&);
struct S {
  void f() {
    g(this);  // Now an error in Clang modes and in GNU C++ modes with
  }           // gnu_version >= 40900.  Still accepted in other GNU C++
};            // modes.
