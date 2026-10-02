//options_all:-r -x -tused
//options: --strict;cn

struct A { A(int); ~A(); };
void f()
{           // block a starts
  A x(1);
  A(2);
  {         // block b starts
    A y(3);
L:;
  }         // block a resumes
  A z(4);
  goto L;
}


