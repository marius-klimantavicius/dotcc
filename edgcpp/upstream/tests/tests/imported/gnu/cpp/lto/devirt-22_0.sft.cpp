//type: fp
//options: 
# 0 "./lto/devirt-22_0.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/devirt-22_0.C"



# 1 "./lto/../ipa/devirt-22.C" 1


class A {};
class B {
public:
  A &operator[](int);
};
class C : B {
public:
  virtual int m_fn1() { return 0; }
  A &operator[](int p1) {
    int a;
    a = m_fn1();
    static_cast<void>(__builtin_expect(a, 0) ?: 0);
    return B::operator[](p1);
  }
};

C b;
int *e;
static void sort(C &p1, C &p2) {
  for (int i=0;; i++) {
    A c, d = p2[0];
    p1[0] = c;
    p2[0] = d;
  }
}

void lookupSourceDone() { b[0]; }

void update_sources() {
  if (e) {
    C f;
    sort(f, b);
  }
}
# 5 "./lto/devirt-22_0.C" 2
