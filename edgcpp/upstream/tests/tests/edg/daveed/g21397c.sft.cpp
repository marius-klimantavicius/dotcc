//remark:Pseudo-destructors
//options:-A;fn:;fp

  typedef int I;
  void g(I *p) {
    (p->~I)();  // Always an error in versions 6.0 and 6.1 of the front end.
  }             // Now okay in nonstrict modes.
