//type:fn
//options_all:--gn 120300
//remark:Severity of narrowing diagnostics
// 7/6/26   [EDGcpfe/26816]
//
// Severity of narrowing diagnostics
//
// In C++11 modes, the front end now diagnoses
//
// with an error instead of a warning.  Note that other similar cases are still
// diagnosed with a warning because existing practice for the severity of
// diagnostics reporting violations of C++11 narrowing rules varies widely.  In
// general, the front end prefers erring on the side of warnings since that is
// more likely to be helpful in various compatibility modes.
int x = { 42.0 };
