//type:fp
//options_all:--microsoft
//remark:[5.0] Microsoft compatibility: __declspec(empty_bases)
// 3/29/18  [EDGcpfe/18118,EDGcpfe/19499]
//
// Microsoft compatibility: __declspec(empty_bases)
//
// The "empty_bases" __declspec attribute is now accepted in Microsoft emulation
// mode.  The presence of the attribute is recorded in the IL but has no effect
// (as the EDG front end does not emulate the layout of the Microsoft compiler).
struct S1 {};
struct S2 {};
struct __declspec(empty_bases) S3 : S2, S1 {
  char c;
};
