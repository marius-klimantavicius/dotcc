//options_all:-r -x -tused
//options: --strict;cn:;cn

struct A { A(int); };
A& f(int i) 
{
  switch (i) {
    case 1: return A(i);
    case 2: return (A(i));
    case 3: return A(1);
    case 4: return (A(1));
    case 5: return A(i,1);
    case 6: return (A(i,1));
    case 7: return A();
    case 8: return (A());
    case 9: return A(int(i));
    case 10: return (A(int(i)));
    case 11: return A((int)i);
    case 12: return (A((int)i));
  }
}

