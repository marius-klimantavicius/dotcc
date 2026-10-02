//type:fp
//options_all:--gn 90200
//remark:[6.3] GNU compatibility: Conversions to and from incomplete class types
// 3/3/21   [EDGcpfe/24010]
//
// GNU compatibility: Conversions to and from incomplete class types
//
// The changes for EDGcpfe/23274 (introduced in version 6.2) caused certain casts
// to incomplete class types to be accepted in GNU C++ modes if those casts appear
// in template-dependent contexts.  Now that change is limited to GNU C++ modes
// with gnu_version <= 100200 but extended to more situations, and it is also
// extended to conversions from (as opposed to "to") incomplete class types.
//
// Previously the cast "I(1)" was rejected in GNU modes that don't defer prototype
// instantiations because I is an incomplete type.  Now it is accepted when
// gnu_version <= 100200.  Similarly, the cast "int(I())" is now accepted in those
// modes.
struct I;
template<typename T> void f() {
  decltype(I(1))      *p1;
  decltype(int(I()))  *p2;
}
