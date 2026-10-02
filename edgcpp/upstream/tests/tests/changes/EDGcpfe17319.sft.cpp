//type:fp
//options_all:--c++11
//remark:[4.12] Repeated elements of constant arrays
// 10/3/16  [EDGcpfe/17319]
//
// Repeated elements of constant arrays
//
// In constant expression evaluation, the front end failed to allow for some
// cases in which the IL is created with a repeated constant in the
// initializer, resulting in incorrect values and/or spurious errors.  Such
// cases can arise with arrays of classes with default member initializers.
struct A { int i,j; };
struct X {
  A a = {1,1};
};
constexpr X table[2][2] = {{ {} }};
static_assert(table[1][1].a.i == 1, "");  // Previously element was
                                          // incorrectly set to 0
