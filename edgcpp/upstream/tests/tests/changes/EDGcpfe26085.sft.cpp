//type:fp
//options_all:-W --c11
//remark:[6.5] C11: Duplicate typedefs
// 2/21/23  [EDGcpfe/26085]
//
// C11: Duplicate typedefs
//
// C11 adopted the C++ rule that a typedef can be re-defined if it doesn't change
// the underlying type.  Previously, the front end issued a warning (by default)
// or error (in strict mode) for such cases.  That is now fixed.
typedef int Int;
typedef int Int;  // Previously triggered a diagnostic in C11 mode.
                  // Now accepted without any diagnostic.
