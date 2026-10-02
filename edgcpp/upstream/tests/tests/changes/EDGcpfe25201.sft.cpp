//type:fp
//options_all:--c++14 -W
//remark:[6.4] Variable template "referenced" flag
// 4/7/22   [EDGcpfe/25201]
//
// Variable template "referenced" flag
//
// The a_template IL entry and its prototype instantiation were previously not
// marked as having been referenced when an instance of a variable template is
// referenced.  This failure resulted in spurious "declared but never
// referenced" warnings for static data member templates appearing in unnamed
// namespaces.  The "referenced" flag for these IL entries is now correctly
// maintained.
namespace {
struct S {
  template<typename T> static constexpr auto v = true;
};
static_assert(S::v<int>, "");  // Previously did not suppress the
                               // "never referenced" warning for S::v<T>
}
