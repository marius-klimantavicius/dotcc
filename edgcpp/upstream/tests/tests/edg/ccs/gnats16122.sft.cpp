//type:cp
//options::--gnu_version=40802
//options_all:--c++11 -tused


template <bool> struct A { typedef int type; };
template <class> struct B;
template <class Function, class... Args> struct B<Function(Args...)> {
  static bool const value = sizeof(char);
};
 
template <class> struct C { using type = int; };
template <class T> using t1 = typename C<T>::type;
template <class Function, class... Args>
typename A<B<Function(t1<Args>...)>::value>::type foo(Function);

void test1_b() {
  foo(test1_b);
}
