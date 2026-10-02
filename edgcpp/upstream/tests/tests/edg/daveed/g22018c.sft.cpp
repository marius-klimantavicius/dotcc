//remark:Multistage substitution
//options:--c++17;fp

  template<bool> struct B;
  template<typename> struct E {};
  struct S { template<typename> static bool sf(); };
  struct X {
    template<typename T, typename B<S::sf<T>()>::Type = true> X(E<T>);
    template<typename T> void operator=(E<T>);
  };
  X g();
  E<int> ei;
  int main() { g() = ei; }
