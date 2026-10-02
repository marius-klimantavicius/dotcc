//type:fp
//options_all:--c++20
//remark:[5.1] C++20: no_unique_address attribute
// 1/9/19   [EDGcpfe/20019,EDGcpfe/20423]
//
// C++20: no_unique_address attribute
//
// Support has now been added for the no_unique_address attribute as documented
// in P0840R2.  In IA-64 ABI configurations, the layout mechanism specified in
// the IA-64 ABI documentation is used.  In Cfront configurations, fields with
// empty class type and the no_unique_address attribute are treated as empty
// base classes for the purposes of layout.
struct E {};
struct S {
  [[no_unique_address]] E e;
  int i;
};
static_assert(sizeof(S) == sizeof(int));
