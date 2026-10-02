//type:fp
//options::-DNEG;fn
//options_all:--c++20 --set_flag skip_module_imports

export module MOD;

export import MOD2; // Legal

#if NEG
export {
  import MOD3; // An export declaration cannot contain a module import declaration
  int randomvar;
}
export ; // Does not introduce any names
export {} // Does not introduce any names
export {
  namespace {} // Does not introduce any names
}
export { // Does not introduce any names
  #if FOO
  #endif
}
export {
  export int var; // Export inside export
}
export export {} // Export inside export + does not introduce any names
export { // Does not introduce any names
  export {} // Export inside export + does not introduce any names
}
export namespace {} // Does not introduce any names
export namespace {
  int a1; // Internal linkage (not currently diagnosed)
}
namespace {
  export int a2; // Internal linkage
}
export static int b; // Internal linkage
int export global_var = b + a1 + a2; // export in the wrong location
#endif

namespace N {
  int c1;
  export int f() { return 0; }
  static int f(int x) { return x; }
  export inline int h() { return f(); }
  inline int h(int x) { return f(x); }
  int g() { return f(0) + h() + h(0); }
}
export namespace N2 {} // Introduces "N2" as a name
namespace N3 {
  int c3;
}
namespace N4 {
  static int c4 = 0;
}
struct S;

#if NEG
export using N::f;  // N::f(int) has internal linkage
export using N::h;  // N::h(int) is inline that uses internal linkage item (not currently diagnosed)
#endif

export using N::c1; // Legal
export using T = S; // Introduces T as a name
export using N::g;  // Exported decl uses internal linkage item (legal)

#if NEG
export using namespace N2; // Does not introduce any names
#endif
export using namespace N3; // Introduces names (legal)
#if NEG
export using namespace N4; // N4::c4 has internal linkage
#endif

typedef S U;
export typedef S U; // Does not re-declare U (legal)
#if NEG
export struct S; // Exported decl follows non-exported decl (not currently diagnosed)
#endif

export struct S2; // Exported decl is first
struct S2 {}; // Implicitly exported (legal)

#if NEG
struct S3 {
  export private: // Not a declaration
  export int foo; // Exporting class member
};
#endif

int x = N4::c4;
