//type:fp
//options_all:--c++20 -tused -A
 template <class T> struct Z {
    typedef typename T::x xx;
  };
  template <class T> concept C = requires { typename T::A; };
  template <C T> typename Z<T>::xx f(void *, T); // #1
  template <class T> void f(int, T);             // #2
  struct A {} a;
  struct ZZ {
    template <class T, class = typename Z<T>::xx> operator T *();
    operator int();
  };
  int main() {
    ZZ zz;
    f(1, a);   // OK, deduction fails for #1 because there is no conversion from int to void*
    f(zz, 42); // OK, deduction fails for #1 because C<int> is not satisfied
  }

//cwg: 2369
//title: Ordering between constraints and substitution
//meeting: Virtual 11/20
//edg_status: EDGcpfe/23552 EDGcpfe/25863
