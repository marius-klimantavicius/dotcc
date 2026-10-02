//type:fp
//remark:[4.10] Spurious error for missing calling convention on explicit instantiation
// 10/13/14 [EDGcpfe/15500]
//
// Spurious error for missing calling convention on explicit instantiation
//
// An explicit instantiation directive that omits a calling convention specified
// on the original template declaration previously elicited a spurious error (in
// modes that accept calling convention specifiers or attributes; i.e., Microsoft
// and GNU modes).
//
// This is now fixed.  (Contradictory calling conventions are still diagnosed.)
template <typename T> void __fastcall g() {};
template void g<int>();  // Previously an error; now okay.
