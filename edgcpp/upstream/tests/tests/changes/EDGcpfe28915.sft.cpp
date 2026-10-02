//type:fn
//options_all:-A --c++20 -W
//remark:Diagnosis of non-C-like unnamed classes
// 7/9/26   [EDGcpfe/28915]
//
// Diagnosis of non-C-like unnamed classes
//
// The front end now more consistently issues a diagnostic in C++ modes when an
// unnamed class type containing non-C-like constructs acquires a name for
// linkage purposes.
//
// The diagnostic is a discretionary error in strict mode and in non-permissive
// Microsoft C++ modes with microsoft_version >= 1926.  In other modes, the
// diagnostic is a warning.
typedef struct {
  struct N {
    void f(void) {}
  };
} S;  // Now elicits a diagnostic.
