//type:cp
//options_all:--il_display
//options:--c++03:--c++11:--c++20 -DTVAR
//filter:grep -e '^ *name_linkage' -e routine_name_linkage -e storage_class | edg-normalize-test-output --il

namespace {
  int var;
  void func() {}
  struct S {
    int mem_var;
    static int smem_var;
    void mem_func() { smem_func(); }
    static void smem_func() {}
  };
#if TVAR
  template<typename T> T tvar;
#endif
  template<typename T> void tfunc(T) {}
  template<typename T> struct TS {
    T mem_var;
    static T smem_var;
    void mem_func() { smem_func(); }
    static void smem_func() {}
  };
}

template<typename T> struct X {
  T mem_var;
};

template<typename T> void gtfunc(T) {}
#if TVAR
template<typename T> T gtvar;
#endif
void gfunc() {}
typedef void (*FP)();
template<FP> void gtnttfunc() {}

X<int> xi; // X<int> has external linkage
X<S> xs;  // X<S> has internal linkage.
X<TS<int> > xts; // X<TS<int>> has internal linkage.
int S::smem_var = 0;
template<>
int TS<int>::smem_var = 0;

int f() {
  int localvar = var + S::smem_var + TS<int>::smem_var;
#if TVAR
  localvar = tvar<int>;
  localvar = gtvar<int>;
  localvar = gtvar<S>.mem_var;
  localvar = gtvar<TS<int> >.mem_var;
#endif
  func();
  tfunc(0);
  gtfunc(0); // Has external linkage
  gtfunc(xs); // Has internal linkage
  gtfunc(xts); // Has internal linkage
  gtnttfunc<gfunc>(); // Has external linkage
  gtnttfunc<func>(); // Has internal linkage
  xs.mem_var.mem_func();
  xts.mem_var.mem_func();
  return localvar;
}
