//type:fp
//remark:[4.1] #pragma pack(pop) with no corresponding "push"
// 7/27/09  [EDGcpfe/9963]
//
// #pragma pack(pop) with no corresponding "push"
//
// Previously, when a "#pragma pack(pop)" directive was encountered with no
// matching "push", a warning or error was issued, and any packing directive in
// effect was discarded.  Now, such an unmatched directive has no other effect
// but the diagnostic.  The new behavior matches that of GNU and Microsoft
// compilers.
#pragma pack(2)
#pragma pack(pop)  // Previously nullified the effect of the
                   // "#pragma pack(2)" directive.  Now, only a
                   // warning (in GNU or Microsoft mode) or an
                   // error (in other modes) is issued.
struct S {
  char a;
  double b;
} x;               // __alignof(x) is now 2 because #pragma pack(2)
                   // is still in effect.
