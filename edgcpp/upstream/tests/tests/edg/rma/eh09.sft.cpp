//options_all:-r -x -tused
//options: --strict;cn

class A {
  A(A&);
};
void f() {
  try { }
  catch (A) { }
}

