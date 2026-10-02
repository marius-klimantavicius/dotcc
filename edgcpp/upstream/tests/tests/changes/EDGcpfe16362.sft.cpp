//type:fp
//options_all:--g++
//remark:[4.11] GNU C++ compatibility: Class definitions in __builtin_offsetof
// 7/31/15  [EDGcpfe/16362]
//
// GNU C++ compatibility: Class definitions in __builtin_offsetof
//
// In GNU C++ mode (but not in Clang C++ mode) the front end now accepts class
// type definitions in __builtin_offsetof constructs.
void g() {
  long a = (long)__builtin_offsetof(struct { char x; int y; }, y);
}                                         // Now accepted in GNU C++ mode.
