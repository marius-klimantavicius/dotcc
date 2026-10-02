//options_all:--c++20 -A
  void g() noexcept;
   int h(void (&)() noexcept); // #1
   int h(void (&)());          // #2
   int j = h(g);               // calls #1, an exact match, rather than #2, a function pointer conversion

//cwg: 2815
//title: Overload resolution for references/pointers to noexcept functions
//meeting: Wroclaw 11/24
//edg_status: EDGcpfe/27748
