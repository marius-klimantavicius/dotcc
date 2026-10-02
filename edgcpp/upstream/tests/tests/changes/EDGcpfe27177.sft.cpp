//type:fp
//options_all:--gn 100999 --c++17
//remark:[6.7] GNU C++ compatibility: Variable templates in alias template definitions
// 6/3/24   [EDGcpfe/27177]
//
// GNU C++ compatibility: Variable templates in alias template definitions
//
// GCC appears to not always substitute nondependent template arguments in
// variable templates if such a combination appears in an alias template
// definition.  The detailed behavior of GCC is unclear (and depends on the
// specific version of GCC), but the front end now accepts some of those cases
// in GNU C++ modes with gnu_version < 130000.
//
// This example is now accepted in some GNU C++ modes, even though the use of
// vart<int, 0> is invalid.
template<bool B> using Void = void;
template<typename T, typename T::whatever> bool vart;
template<typename> using Alias = Void<vart<int, 0>>;
