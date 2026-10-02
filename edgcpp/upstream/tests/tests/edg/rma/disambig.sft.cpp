//options_all:-r -x -tused
//options: --strict;cn

void ff() {
  typedef int I;
  I(a);            // decl
  I(a)++;          // stmt
  I(f)();          // decl
  I(*g)(int);      // decl
  I(b) = 1;        // decl
  I(c)[2];         // decl
  I(a)<I(b);       // expr
}

