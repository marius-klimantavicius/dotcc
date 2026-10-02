//remark:Vector assignment
//options:--c11 --gnu=70300;fp

  typedef long long VLL1 __attribute((vector_size(8)));
  typedef int VI2 __attribute((vector_size(8)));
  void g(VLL1 x, VI2 y) {
    x = y;  // Previously an error.  Now okay.
  }
