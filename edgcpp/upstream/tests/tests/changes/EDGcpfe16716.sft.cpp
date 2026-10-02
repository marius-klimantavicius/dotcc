//type:fp
//options_all:--microsoft_version=1700
//remark:[4.11] Microsoft compatibility, C++-generating back end: "this" in member function
// 1/28/16  [EDGcpfe/16716]
//
// Microsoft compatibility, C++-generating back end: "this" in member function
// return type
//
// The front end previously had two problems with regard to implicit
// references to "this" appearing in the return type of a member function.
// First, the Microsoft compiler has accepted this usage since MSVC 11, but
// the front end only implicitly enabled it with microsoft_version >= 1900,
// emulating MSVC 13.  Also, the C++-generating back end previously put out an
// explicit "this->" in such a reference in the generated code, but the
// Microsoft compiler disallowed the "this" keyword in that context up through
// early versions of MSVC 13.  These are both now fixed.
// --microsoft_version=1700:
struct S {
  int f();
  auto g()->decltype(f()); // Previously rejected, now accepted; also,
                           // previously generated as decltype(this->f()),
                           // which was rejected by MSVC.
};
