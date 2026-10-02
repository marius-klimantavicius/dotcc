//type:fp
//options_all:--c++20 -tused -A
 template <typename T> concept C = true;
  template <typename T> struct A;
  template <> struct A<bool> { using type = bool; };

  template <typename T>
  void f(A<decltype(C<T>)>::type); // error: needs typename to avoid vexing parse

//cwg: 2446
//title: Questionable type-dependency of concept-ids
//meeting: Prague 02/20
//edg_status: EDGcpfe/22365
