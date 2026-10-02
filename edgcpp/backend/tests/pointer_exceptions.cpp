// Compare against EDG's portable C runtime, including pointer_buffer rethrows.
enum First { one=11, two=12 };
enum Second { other=11 };
enum class Small : unsigned char { value=201 };
enum class Wide : unsigned long long { value=0xfedcba9876543210ULL };
typedef First Alias;
typedef decltype(nullptr) Null;
struct Box { int value; };
struct Guard { int value; Guard(int n):value(n){} ~Guard(); };
unsigned long long trace;
Guard::~Guard() { trace=trace*10+value; }
unsigned long long read_trace() { return trace; }
int owned=37;
int* owned_address() { return &owned; }
void throw_owned() { throw &owned; }
void again() { throw; }
int catch_owned(void (*hook)(),int* expected) {
  try { hook(); } catch(int* p) { return p==expected && *p==37; }
  return 0;
}
int threaded(int value,void (*hook)()) {
  int replacement=99;
  try { try { throw &value; } catch(int*& p) { p=&replacement; hook(); again(); } }
  catch(int* p) { return *p; }
  return -1;
}
int run_case(int which,void (*hook)()) {
  trace=0;
  int a=17,b=23;
  int* p=&a; int** pp=&p; int*** ppp=&pp;
  const int* cp=&a; const int** cpp=&cp;
  Box box={41};
  if(which==0) { try { Guard g(1); throw p; } catch(int* q) { hook(); return *q; } }
  if(which==1) { try { throw p; } catch(long*) { return -1; } catch(int* q) { return *q; } }
  if(which==2) { try { throw p; } catch(const int* q) { hook(); return *q; } }
  if(which==3) { try { throw cp; } catch(int*) { return -1; } catch(const int* q) { return *q; } }
  if(which==4) { try { throw p; } catch(void* q) { return q==p; } }
  if(which==5) { try { throw cp; } catch(void*) { return -1; } catch(const void* q) { return q==cp; } }
  if(which==6) { try { throw pp; } catch(const int**) { return -1; } catch(int** q) { return **q; } }
  if(which==7) { try { throw pp; } catch(const int* const* q) { hook(); return **q; } }
  if(which==8) { try { throw pp; } catch(int* const* q) { return **q; } }
  if(which==9) { try { throw cpp; } catch(int**) { return -1; } catch(const int** q) { return **q; } }
  if(which==10) { try { throw ppp; } catch(const int***) { return -1; } catch(const int* const* const* q) { return ***q; } }
  if(which==11) { try { throw pp; } catch(int*) { return -1; } catch(void* q) { return q==pp; } }
  if(which==12) { try { throw cpp; } catch(void* q) { return q==cpp; } }
  if(which==13) { try { throw (int*)nullptr; } catch(Null) { return -1; } catch(int* q) { return q==nullptr; } }
  if(which==14) { try { throw nullptr; } catch(int* q) { hook(); return q==nullptr; } }
  if(which==15) { try { throw nullptr; } catch(int** q) { return q==nullptr; } }
  if(which==16) { try { throw nullptr; } catch(Null) { return 16; } }
  if(which==17) { try { throw nullptr; } catch(const void* q) { return q==nullptr; } }
  if(which==18) { try { throw 0; } catch(int*) { return -1; } catch(int n) { return n+18; } }
  if(which==19) { try { throw &box; } catch(Box* q) { hook(); return q->value; } }
  if(which==20) { try { throw &box; } catch(const void* q) { return q==&box; } }
  if(which==21) { try { throw one; } catch(int) { return -1; } catch(Second) { return -2; } catch(First n) { return n; } }
  if(which==22) { try { throw Small::value; } catch(unsigned char) { return -1; } catch(Small n) { return (int)n; } }
  if(which==23) { try { throw Wide::value; } catch(Wide n) { return n==Wide::value; } }
  if(which==24) { try { throw one; } catch(const Alias& n) { hook(); return n; } }
  if(which==25) { try { try { throw one; } catch(First& n) { n=two; hook(); throw; } } catch(First n) { return n; } }
  if(which==26) { try { try { throw p; } catch(int*& q) { q=&b; hook(); again(); } } catch(int* q) { return *q; } }
  if(which==27) { try { try { throw p; } catch(const int*& q) { q=&b; hook(); throw; } } catch(const int* q) { return *q; } }
  if(which==28) { try { try { throw nullptr; } catch(int*& q) { q=&b; hook(); throw; } } catch(Null) { return 28; } }
  if(which==29) { try { try { throw p; } catch(int* q) { q=&b; throw; } } catch(int* q) { return *q; } }
  if(which==30) {
    try { throw p; } catch(int*& q) {
      int** address=&q; q=&b; hook();
      try { throw &box; } catch(Box* r) { hook(); trace=r->value; }
      hook(); return *address==&b && **address==23;
    }
  }
  if(which==31) {
    try { Guard outer(1); try { Guard inner(2); throw p; } catch(long*) { return -1; } }
    catch(const int* q) { hook(); return *q; }
  }
  if(which==32) { First n=one; try { throw &n; } catch(int*) { return -1; } catch(First* q) { return *q; } }
  if(which==33) { try { throw other; } catch(First) { return -1; } catch(Second n) { return n; } }
  if(which==34) { enum Local { x=34 }; try { throw x; } catch(Local n) { return n; } }
  if(which==35) { enum Local { x=35 }; try { throw x; } catch(Local n) { return n; } }
  if(which==36) { int* const fixed=p; try { throw fixed; } catch(int* const& q) { hook(); return *q; } }
  if(which==37) { try { try { throw p; } catch(...) { hook(); throw; } } catch(int* q) { return *q; } }
  if(which==38) { const int* const* q=pp; try { throw q; } catch(int**) { return -1; } catch(const int* const* r) { return **r; } }
  if(which==39) { try { throw ppp; } catch(const int* const**) { return -1; } catch(const int* const* const* q) { return ***q; } }
  if(which==40) { try { throw ppp; } catch(int** const* q) { return ***q; } }
  if(which==41) { try { throw &box; } catch(int*) { return -1; } catch(Box* q) { return q->value; } }
  if(which==42) { try { throw (void*)p; } catch(int*) { return -1; } catch(void* q) { return q==p; } }
  if(which==43) { try { try { throw nullptr; } catch(int*& q) { q=&b; hook(); throw; } } catch(int* q) { return q==nullptr; } }
  if(which==44) { const First n=one; try { throw &n; } catch(First*) { return -1; } catch(const First* q) { return *q; } }
  if(which==45) { try { throw (int*)nullptr; } catch(long*) { return -1; } catch(void* q) { return q==nullptr; } }
  if(which==46) {
    try { throw p; } catch(int*& q) {
      q=&b;
      try { throw; } catch(int*& r) { if(*r!=17)return -1; r=nullptr; hook(); }
      hook(); return *q;
    }
  }
  if(which==47) { try { throw p; } catch(Box*) { return -1; } catch(const int* const& q) { hook(); return *q; } }
  return -99;
}
