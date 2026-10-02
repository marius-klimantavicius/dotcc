//type:fp
//options_all:--microsoft
//remark:[4.10] Microsoft compatibility: Exported generated members and default arguments
// 10/15/14 [EDGcpfe/15503]
//
// Microsoft compatibility: Exported generated members and default arguments
//
// In Microsoft mode, the front end forces the definition of generated special
// member functions for "dllexported" classes (see the entry of 3/21/12 for
// EDGcpfe/12772).  However, previously this generation occurred before some
// default arguments had been processed, which in turn triggered spurious errors.
//
// Previously, generating the definition of S::S() produced an error indicating
// that N has no default constructor because the default arguments of N's
// constructor were not recognized.  This is now fixed.
struct __declspec(dllexport) S {
  struct __declspec(dllexport) N {
    int p, q;
    N(int x = 0, int y = 0): p(x), q(y) {}
  } p1;
};
