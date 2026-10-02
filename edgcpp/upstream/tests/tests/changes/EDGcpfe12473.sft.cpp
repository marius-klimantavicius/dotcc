//type:fp
//options_all:--c --gcc
//remark:[4.4] Qualified function types in C mode
// 12/1/11  [EDGcpfe/12473]
//
// Qualified function types in C mode
//
// In C++ modes, the front end ignores type qualifiers on function types (with
// a warning, unless the qualification occurs through template substitution).
// This matches the requirements of the C++ standard.  The C standard, however,
// deems type qualifiers on function types (which can happen through typedefs,
// or in GNU C mode, through typeof) to be undefined behavior.  The front end
// previously silently accepted such qualifiers and retained them in the IL
// representation, which could affect type compatibility later on.
//
// The C mode behavior has been changed to match that of C++ modes.
//
// The prior C mode behavior triggered an internal error in form_type_first_part
// ("qualifier on function type") in GNU C mode diagnostics that involved a
// qualified function type formed by adding qualifiers to a typeof construct.
//
// This is fixed by the change described above.  The IL-to-string routines have
// also been modified to gracefully handle qualified function types when
// rendering code that is not meant to be compiled (diagnostics in particular).
typedef void F();
F const *pf;  // Previously silently accepted in C mode.  C++ modes
              // ignore the "const" with a warning.
void g(F *p) {
  p = pf;     // Previously triggered a type incompatibility error in
}             // C modes because pf was treated as a pointer to const.

void f();
void (*p)() = (const typeof(f)*)f;  // Previously aborted.
