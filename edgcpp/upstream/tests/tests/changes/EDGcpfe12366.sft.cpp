//type:fp
//remark:[4.4] C++-generating back end: Member access specifiers and Microsoft attributes
// 10/25/11 [EDGcpfe/12366]
//
// C++-generating back end: Member access specifiers and Microsoft attributes
//
// The C++-generating back end previously rendered an access specifier for a
// declaration after Microsoft attributes (enclosed in square brackets) for that
// declaration: The resulting syntax is invalid.
//
// This is now fixed.
class C {
public:
  [Any(0)] int f();  // Previously rendered as "[Any(0)] public: int f();"
};
