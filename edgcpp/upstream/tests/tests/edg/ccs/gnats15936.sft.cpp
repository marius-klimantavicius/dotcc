//type:cp
//options::--microsoft:--g++ -DUNSIGNED:--clang -DUNSIGNED
//options_all:--c++11

template<typename,typename> struct same;
template<typename T> struct same<T, T>{};

enum A {};
enum B { b };
enum class C {};
enum class D { d };

void f() {
#if UNSIGNED
  same<__underlying_type(A), unsigned int>();
  same<__underlying_type(B), unsigned int>();
#else
  same<__underlying_type(A), int>();
  same<__underlying_type(B), int>();
#endif
  same<__underlying_type(C), int>();
  same<__underlying_type(D), int>();
}
