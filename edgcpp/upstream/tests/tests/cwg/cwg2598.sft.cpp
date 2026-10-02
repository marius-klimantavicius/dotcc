//type:fp
//options_all:--c++20 -tused -A
  struct A { A(); };
  union U {
    A a;
    constexpr U() {}
    constexpr ~U() {}
  };

//cwg: 2598
//title: Unions should not require a non-static data member of literal type
//meeting: Kona 11/22
//edg_status: Passes
