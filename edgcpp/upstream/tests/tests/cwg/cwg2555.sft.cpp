//type:fp
//options:--c++23
//options_all:-A

struct B {
  void i();
  void j() = delete;
};

struct D : B {
  using B::i;
  void i(this B &) = delete;  // OK

  using B::j;
  void j(this D &);  // OK, D::j() hides B::j()
};

void k(D* p)
{
  p->i();         // calls B::i, because B::i as a member of D is a better match than D::i
  p->j();         // calls D::j
}

//cwg: 2555
//title: Ineffective redeclaration prevention for using-declarators
//meeting: Kona 11/25
//edg_status: EDGcpfe/28533
