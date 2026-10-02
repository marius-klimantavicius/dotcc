//type: fp
//options: --c++20
// PR c++/93595
// { dg-do compile { target c++20 } }

template<int>
struct bad {
  template<class T, auto = []{ return T(); }>
  static void f(T) { }
};

int main() {
  bad<0>::f(0);
}
