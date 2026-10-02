template<typename>
struct A {
  enum class ENUM { e1 };
  A(int);
  void func();
  virtual void vfunc();
};
struct B {
  A<int> arr[1] { 0 };
};
template<typename T>
void A<T>::func() {
  ENUM::e1;
}
template<typename T>
void A<T>::vfunc() {
  ENUM::e1;
}
