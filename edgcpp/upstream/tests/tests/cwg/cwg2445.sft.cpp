//type:fp
//options_all:--c++20 -tused -A
  template <typename> constexpr bool F = false;
  template <typename T> struct A { };

  template <typename T, typename U>
  bool operator==(T, A<U *>);             // 1a

  template <typename T, typename U>
  bool operator!=(A<T>, U) {              // 2
   static_assert(F<T>, "Isn't this less specialized?");
   return false;
  }

  bool f(A<int> ax, A<int *> ay) { return ay != ax; }

//cwg: 2445
//title: Partial ordering with rewritten candidates
//meeting: Prague 02/20
//edg_status: EDGcpfe/22346
//fixed_in: 6.2
