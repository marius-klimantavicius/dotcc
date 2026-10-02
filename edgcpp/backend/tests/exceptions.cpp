// Header-free scalar EH: the original EDG C backend is the execution oracle.
unsigned long long trace;
void mark(int n) { trace=trace*10+n; }
unsigned long long read_trace() { return trace; }
struct Guard {
  int n;
  Guard(int value):n(value) { mark(n); }
  ~Guard() noexcept(false) { mark(n+4); }
};
struct ExitThrow {
  ~ExitThrow() noexcept(false) { mark(8); throw 81; }
};
struct RethrowDtor {
  ~RethrowDtor() noexcept(false) { mark(9); throw; }
};
void again() { throw; }
void throw_int(int n) { Guard g(2); throw n; }
int failed_operand() { Guard g(2); throw 48; }
int exit_try() {
  ExitThrow g;
  try { return 9; } catch(int) { mark(7); return 70; }
}
int exit_catch() {
  ExitThrow g;
  try { throw 1; } catch(int) { return 9; }
}
int run_case(int test, void (*hook)()) {
  trace=0;
  if(test==0) { try { throw 13; } catch(int n) { return n; } }
  if(test==1) { try { throw 13; } catch(unsigned int) { return 91; } catch(int n) { return n; } }
  if(test==2) { try { throw 13L; } catch(long long) { return 92; } catch(long n) { return (int)n; } }
  if(test==3) { try { throw 14LL; } catch(long) { return 93; } catch(long long n) { return (int)n; } }
  if(test==4) { try { throw (char)15; } catch(signed char) { return 94; } catch(char n) { return n; } }
  if(test==5) { try { throw (signed char)-15; } catch(char) { return 95; } catch(signed char n) { return n; } }
  if(test==6) { try { throw true; } catch(unsigned char) { return 96; } catch(bool n) { return n; } }
  if(test==7) { try { throw (wchar_t)17; } catch(int) { return 97; } catch(wchar_t n) { return n; } }
  if(test==8) { try { throw (char16_t)18; } catch(unsigned short) { return 98; } catch(char16_t n) { return n; } }
  if(test==9) { try { throw (char32_t)19; } catch(unsigned int) { return 99; } catch(char32_t n) { return n; } }
  if(test==10) { try { throw 4294967295U; } catch(unsigned int n) { return n==4294967295U?20:90; } }
  if(test==11) { try { throw -9223372036854775807LL; } catch(long long n) { return n==-9223372036854775807LL?21:90; } }
  if(test==12) {
    Guard outer(1);
    try { Guard inner(2); throw_int(22); }
    catch(const int& n) { hook(); mark(3); return n; }
  }
  if(test==13) {
    try { try { throw 23; } catch(int& n) { ++n; hook(); again(); } }
    catch(const int& n) { return n; }
  }
  if(test==14) {
    try { try { throw 25; } catch(int n) { n=99; again(); } }
    catch(int n) { return n; }
  }
  if(test==15) {
    try { try { throw 26; } catch(...) { Guard g(1); again(); } }
    catch(int n) { return n; }
  }
  if(test==16) {
    try { try { throw 27; } catch(int n) {
      Guard a(1);
      try { Guard b(2); throw 72L; } catch(long x) { mark(3); }
      hook(); again();
    } } catch(int n) { return n; }
  }
  if(test==17) {
    try { try { throw 28; } catch(int n) { Guard a(1); throw 29L; } }
    catch(int) { return 90; } catch(long n) { return (int)n; }
  }
  if(test==18) {
    Guard a(1);
    try { try { Guard b(2); throw 30; } catch(long) { return 90; } }
    catch(int n) { mark(3); return n; }
  }
  if(test==19) { try { return exit_try(); } catch(int n) { mark(3); return n; } }
  if(test==20) { try { return exit_catch(); } catch(int n) { mark(3); return n; } }
  if(test==21) {
    int sum=0;
    for(int i=0;i<4;++i) { try { Guard g(1); throw i; } catch(int n) { sum+=n; } }
    return sum;
  }
  if(test==22) {
    try { Guard g(1); throw 32; } catch(int) { goto out; }
    out: try { throw 33; } catch(int n) { return n; }
  }
  if(test==23) {
    try { Guard a(1); (test?throw 34:throw 90); } catch(int n) { return n; }
  }
  if(test==24) {
    try { throw (hook(),35); } catch(int n) { return n; }
  }
  if(test==25) {
    try { throw 36; } catch(int& n) { try { throw (n+=1,n); } catch(int& m) { m+=2; } return n; }
  }
  if(test==26) {
    try { throw 37; } catch(int) {
      try { { RethrowDtor d; } } catch(int n) { return n; }
    }
  }
  if(test==27) {
    try { throw (Guard(1),38); } catch(int n) { mark(3); return n; }
  }
  if(test==28) {
    try { throw (long)failed_operand(); } catch(long) { return 90; } catch(int n) { return n; }
  }
  if(test==29) {
    try { Guard outer(1); try { Guard inner(2); throw 41; } catch(int) { Guard handler(3); throw 42; } }
    catch(int n) { return n; }
  }
  if(test==30) {
    int total=0;
    again_label:
    try { throw total; } catch(int n) { if(n<3) { ++total; goto again_label; } return n; }
  }
  if(test==31) {
    try { throw 51; } catch(int& outer) {
      try { throw 52; } catch(int) { goto leave_inner; }
      leave_inner: hook();
      try { again(); } catch(int n) { return n; }
    }
  }
  return 89;
}
int foreign(void (*hook)()) {
  trace=0;
  try { Guard g(1); hook(); }
  catch(int) { return 90; }
  catch(...) { Guard g(2); again(); }
  return 91;
}
int threaded(int value, void (*hook)()) {
  try { try { throw value; } catch(int& n) { hook(); ++n; again(); } }
  catch(int n) { return n; }
}
void no_throw() noexcept { throw 42; }
int stale_return() { RethrowDtor d; try { throw 49; } catch(int) { return 7; } }
void terminate_case(int which) {
  if(which==0) { again(); }
  if(which==1) { no_throw(); }
  if(which==2) { ExitThrow g; throw 43; }
  if(which==3) { try { throw 44; } catch(int) {} again(); }
  if(which==4) { try { throw 45; } catch(int) { goto done; } done: again(); }
  if(which==5) { try { throw 46; } catch(int) { Guard g(1); } again(); }
  if(which==6) { stale_return(); }
}
