//type:fp
//options_all:--c++20
//remark:[6.4] Spurious error with default concept arguments
// 2/22/22  [EDGcpfe/25097]
//
// Spurious error with default concept arguments
//
// The front end previously ignored default concept arguments in some situations,
// leading to spurious errors.
//
// That is now fixed.
template<typename, typename U = int> concept C = sizeof(U) > 1;
C auto x = 1;  // Previously a spurious error claiming the constraint
               // failed.  Now okay.
