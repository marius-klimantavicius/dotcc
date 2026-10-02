//type:fn
//options_all:--c++20 -tused -A
 namespace K {
    template <typename T, typename U = char> struct A { };
    A<short> *a;
  }

  template <typename T> using A = K::A<short, T>;

  int main() {
    K::a->~A<char>();
  }

//cwg: 1908
//title: Dual destructor lookup and template-ids
//meeting: Virtual 11/20*
//edg_status: Passes
