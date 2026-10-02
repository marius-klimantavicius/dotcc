//type:fp
//options_all:--microsoft
//remark:[4.10.1] Microsoft calling conventions on x86-64 targets
// 5/14/15  [EDGcpfe/16136]
//
// Microsoft calling conventions on x86-64 targets
//
// In Microsoft modes with targ_supports_x86_64 set to TRUE, the front end now
// ignores calling conventions other than __vectorcall when determining type
// compatibility.
void __stdcall f() {}
void g() {
  void (*pf)() = &f;  // Previously an error in all Microsoft modes.
}                     // Now accepted in Microsoft modes with x86-64
                      // targets.
