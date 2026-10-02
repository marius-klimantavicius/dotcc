//options_all:--c++20 -tused -A
  template <class S1, class S2> struct C {
    C(...);
  };

  template<class T1> C(T1) -> C<T1, T1>;
  template<class T1, class T2> C(T1, T2) -> C<T1 *, T2>;

  template<class V1, class V2> using A = C<V1, V2>;

  C c1{""};
  A a1{""};

  C c2{"", 1};
  A a2{"", 1};

//cwg: 2664
//title: Deduction failure in CTAD for alias templates
//meeting: Issaquah 2/23
//edg_status: EDGcpfe/25901
//fixed_in: 6.5
