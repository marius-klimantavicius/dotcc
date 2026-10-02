//type:fp
//remark:[4.9] Spurious error on calls to implicitly-declared trivial destructor
// 11/19/13 [EDGcpfe/14127]
//
// Spurious error on calls to implicitly-declared trivial destructor
//
// The front end previously issued a spurious error on a call with an implied
// "this->" to an implicitly-declared trivial destructor.
//
// This is now fixed.
struct S { void d(); };
void S::d() {
  this->S::~S();  // Always okay.
  S::~S();        // Previously a spurious error; now accepted.
}
