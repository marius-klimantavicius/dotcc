//type: fp
//options: 
# 0 "./lto/devirt-19_0.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/devirt-19_0.C"



# 1 "./lto/../ipa/devirt-19.C" 1






struct A {
  void operator==(const A &);
};
class B {
public:
  A m_fn1();
  A m_fn2();
};
template <typename T, typename M> class C {
public:
  T Key;
  const M &m_fn2(const T &);
  virtual void m_fn1() {}
  B _map;
};

C<int, int> b;
template <typename T, typename M> const M &C<T, M>::m_fn2(const T &) {

  A a = _map.m_fn2();
  a == _map.m_fn1();
  m_fn1();
  static M m;
  return m;
}

void fn1() { b.m_fn2(0); }
# 5 "./lto/devirt-19_0.C" 2
