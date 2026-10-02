//options_all:-r -x -tused
//options: --strict;rp

namespace A {
  extern "C" {
    int* f(const int* x) { return const_cast<int*>(x); }
  }
}

const int* f(const int* x) { return A::f(x); }
      int* f(      int* x) { return A::f(x); }

int main()
{
  int i;
  f(&i);
  f((const int*) &i);
}

