//type:fn
//options_all:--c++17 -tused -A
//
template<typename T> struct A {
  enum class E { e1; };
};
template<> enum class A<int>::E;

//cwg: 1638
//title: Declaring an explicit specialization of a scoped enumeration
//meeting: Jacksonville 2/16
//edg_status: Passes
