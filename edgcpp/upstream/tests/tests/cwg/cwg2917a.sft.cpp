//type:fp
//options:--c++26
//options_all:-A

template<class ...>
struct C {
  struct Nested { };
};

template<class ... Us>
struct S {
  friend class C<Us>::Nested...;     // OK, not a template-declaration
};

//cwg: 2917
//title: Disallow multiple friend-type-specifiers for a friend template
//meeting: Kona 11/25
//edg_status: EDGcpfe/28535
