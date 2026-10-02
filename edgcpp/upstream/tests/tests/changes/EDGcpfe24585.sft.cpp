//type:fp
//options_all:--c++20
//remark:[6.6] Abort on class-scope enumerator using-declaration
// 7/20/23  [EDGcpfe/24585,EDGcpfe/26502]
//
// Abort on class-scope enumerator using-declaration
//
// C++20 allows a using-declaration in class scope to also name enumerators.
// However, the front end previously aborted with an internal error in
// access_for_symbol (symbol_tbl.c) when such a declaration was named in a derived
// class.
enum E { E1 };
struct B {
  using E::E1;
};
struct D : B {
  void f() {
    E1;  // Previously triggered an internal error.  Now okay.
  }
};
