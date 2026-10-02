//type:fp
//options_all:--microsoft_version 1915
//remark:[6.0] Spurious Microsoft-mode error on constexpr initialization in template
// 8/2/19   [EDGcpfe/21569]
//
// Spurious Microsoft-mode error on constexpr initialization in template
//
// In Microsoft C++ modes, the front end sometimes issued a spurious error about a
// constexpr static data member in a class template not having a constant
// initializer.
//
// The problem was triggered during the "nonreal instantiation" (a Microsoft-mode-
// specific operation) of the dependent base class of D<T>.  That is now fixed.
template<typename> struct V { static const int v = 42; };
template<typename T> constexpr int v = V<T>::v;
template<typename T> struct B {
  static constexpr int v = v<T>;  // Previously a spurious error.
};
template <typename T> struct D: B<T> {};
