//type:cp
//options::--microsoft
//options_all:--parse_templates

template<class T>
struct A {
  A(int x=0);
};
template <class T>
struct B : A<T> {
  void f();
};
template <class T>
void B<T>::f() {
  A<T> a;
}
