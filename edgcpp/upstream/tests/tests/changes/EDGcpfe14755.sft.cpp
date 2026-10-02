//type:fn
//options_all:--microsoft
//remark:[4.9] Microsoft compatibility: Preferring move constructors in overload resolution
// 1/30/14  [EDGcpfe/14755]
//
// Microsoft compatibility: Preferring move constructors in overload resolution
//
// In Microsoft mode, the front end prefers copy constructors over other member
// functions during overload resolution (see the Changes entry of 8/14/03).  This
// rule is now extended to include move constructors.
struct D {
  D(D&&);
  D(int);
};
struct S {
  operator D();
  operator int();
};
S g();
D d(g());  // Accepted in Microsoft modes when the 4.9 change was made
           // (D(D&&) selected).  Later versions diagnose an ambiguity.
