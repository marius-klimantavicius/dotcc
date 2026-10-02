//type:fn
//options_all:--microsoft
//remark:[4.4] Invalid in-class using-declarations in Microsoft mode
// 5/20/11  [EDGcpfe/11462]
//
// Invalid in-class using-declarations in Microsoft mode
//
// In Microsoft mode, an in-class using-declaration referring to a constructor or
// destructor only elicits a warning instead of an error (see Changes entry of
struct X { ~X(); };
struct Y {
  using X::~X;  // Previously triggered just a warning in Microsoft mode,
};              // even though X is not a base of Y.  Now an error is issued.
