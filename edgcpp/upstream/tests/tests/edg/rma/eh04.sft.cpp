//options_all:-r -x -tused
//options: --strict;cn

class A;
template <class T> class B;
A* pa;
B<int>* pbi;
void f(int i) {
  class C {} *pc;
  switch (i) {
    case 0:  throw *pa;  break;
    case 1:  throw *pbi; break;
    default: throw *pc;  break;
  }
}
void g(int i) {
  class C {};
  try {
    f(i);
  }
  catch (A) { }
  catch (B<int>) { }
  catch (C) { }
}

