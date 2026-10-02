//type:fp
//options_all:--c++20
//remark:[6.2] C++20: Ambiguity with rewritten reversed-parameters comparison candidate
// 12/14/20 [EDGcpfe/23515,EDGcpfe/23643]
//
// C++20: Ambiguity with rewritten reversed-parameters comparison candidate
//
// The changes for EDGcpfe/23009 relax the overload resolution rules in nonstrict
// modes when a member operator== is ambiguous because of differences in const-
// qualifiers between the explicit parameter and the "this" parameter.  Now, that
// relaxation is also applied when an operator!= can be rewritten in terms of such
// an operator==.
struct X {
  bool operator!=(const X&);
  bool operator==(const X&);
};
bool b = X() != X();  // Previously ambiguous in C++20 mode.  Now selects
                      // operator!= in nonstrict C++20 modes (with a
                      // warning).
