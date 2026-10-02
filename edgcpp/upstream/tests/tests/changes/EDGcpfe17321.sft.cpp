//type:fn
//options_all:--c++11
//remark:[4.12] Missing diagnostic on duplicate enumerator name
// 6/19/16  [EDGcpfe/17321]
//
// Missing diagnostic on duplicate enumerator name
//
// In C++11 mode, when an opaque enum type is declared in a class definition and
// later defined outside that class definition, the front end failed to detect
// duplicate enumerator names in the enum type definition.
//
// This is now fixed.
struct S {
  enum E: int;
};
enum S::E: int { e, e };  // Previously accepted with no diagnostic.
                          // Now an error.
