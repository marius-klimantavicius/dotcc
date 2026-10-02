//type:fp
//remark:[4.9] Abort on type traits helper that checks access in template argument
// 3/21/14  [EDGcpfe/14938]
//
// Abort on type traits helper that checks access in template argument
//
// Certain type traits helpers (like __is_destructible) check the accessibility
// of special member functions.  When such a construct was used in a context that
// ordinarily requires the deferral of accessibility checks (in particular,
// template arguments), the front end aborted in record_access_error (with
// message "access check result needed immediately but access check deferral in
// effect", in symbol_tbl.c).
//
// This is now fixed.
class C { ~C(); };          // Private destructor.
template<bool> struct X {};
X<__is_destructible(C)> x;  // Previously aborted; now okay.
