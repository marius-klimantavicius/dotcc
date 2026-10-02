//remark:__builtin_expect and control-flow diagnostics
//options:--g++ --diag_error=940;fp

  int f() {
    if (__builtin_expect(true, 0)) throw 0;
  }
  int g() {
    if (true) throw 0;
  }
