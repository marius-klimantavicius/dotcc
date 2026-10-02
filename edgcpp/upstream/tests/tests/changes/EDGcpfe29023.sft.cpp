//type:fp
//options_all:--c++11
// 8/27/26  [EDGcpfe/29023]
//
// Spurious error for dependent member call with explicit template arguments
//
// A call with explicit template arguments to an operator member of a dependent
// class could be wrongly rejected when an unrelated template contained a plain
// dependent call to the same operator member.
template<typename T> T v();
template<typename T, class F>
auto f(F) -> decltype(v<F>().template operator()<1>());
struct C {
  template<int = 0> int operator()() const;
};
template<typename T>
int g() {
  return v<T>().operator()();
}
int i = f<int>(C{});
