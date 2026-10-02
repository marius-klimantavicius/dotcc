//type:fp
//options_all:--c++17 -tused -A
//

  template <class T> struct Z {
    typedef typename T::x xx;
  };
  template <class T> typename Z<T>::xx f(void *, T); // #1
  template <class T> void f(int, T);                 // #2
  struct A {} a;
  int main() {
    f(1, a);                                         // OK, deduction fails for #1 because there is no conversion from int to void*
  }

//cwg: 1391
//title: Conversions to parameter types with non-deduced template arguments
//meeting: Kona 10/15
//edg_status: Passes
