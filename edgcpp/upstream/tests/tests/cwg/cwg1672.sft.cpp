//type:fp
//options_all:--c++17 -tused -A
  struct empty {};
  struct A { char a; };
  struct also_empty : empty {};
  struct C : empty, also_empty { char c; };
  union U {
    struct X { A a1, a2; } x;
    struct Y { C c1, c2; } y;
  } u;

  int main()
  {
      static_assert(!__is_standard_layout(U));
  }

//cwg: 1672
//title: Layout compatibility with multiple empty bases
//meeting: Urbana-Champaign 11/14
//edg_status: Passes
