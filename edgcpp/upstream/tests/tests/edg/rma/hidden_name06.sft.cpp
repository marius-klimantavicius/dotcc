//options_all:-r -x -tused
//options: --strict;cp

template <class T > void f(const T *) { }

struct X {
  void f();
};

//void g() { f((X*)0); }

void X::f() {
  ::f((X*)0);
}

