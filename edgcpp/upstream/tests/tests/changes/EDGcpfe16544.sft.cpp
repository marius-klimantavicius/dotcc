//type:fp
//options_all:--microsoft
//remark:[4.11] Microsoft compatibility: Add __is_assignable type trait
// 11/25/15 [EDGcpfe/16544]
//
// Microsoft compatibility: Add __is_assignable type trait
//
// Recent versions of Microsoft Visual Studio have implemented a new
// type trait: __is_assignable.
struct A {
  operator int();
};
static_assert(__is_assignable(int&, int), "ERROR");
static_assert(__is_assignable(int&, A), "ERROR");
