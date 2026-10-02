//type:fp
//options_all:-W
//remark:[6.4] Variables defined in unnamed namespaces and never referenced
// 3/2/22   [EDGcpfe/24868]
//
// Variables defined in unnamed namespaces and never referenced
//
// The front end warns about variables defined in unnamed namespaces but never
// referenced, because such variables have internal linkage and thus cannot be
// referenced from other translation units and thus might represent an
// oversight on the part of the programmer.  The front end has now been
// changed to issue a remark instead of a warning if the initialization of
// such a variable has side effects, since it is possible that the programmer
// defined the variable solely for the side effects.
struct S {
  S();
};
namespace {
  S xx;  // Previously a warning, now a remark, because the initialization
         // has a side effect (calling the constructor)
}
