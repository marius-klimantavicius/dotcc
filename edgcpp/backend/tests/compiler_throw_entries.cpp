#include <typeinfo>
static int events;
struct Guard { Guard(){events=events*10+1;} ~Guard(){events=events*10+2;} };
struct Base { virtual int kind(){return 0;} };
struct Derived : Base {};
extern "C" [[noreturn]] void __cxa_bad_cast() { throw 71; }
extern "C" [[noreturn]] void __cxa_bad_typeid() { throw 72; }
extern "C" [[noreturn]] void __cxa_throw_bad_array_new_length() { throw 73; }
static Derived& require(Base& value) { return dynamic_cast<Derived&>(value); }
int main() {
 Base value; Derived derived;
 if(&require(derived)!=&derived)return 1;
 events=0;
 try { Guard guard; require(value); return 2; } catch(int n) { if(n!=71 || events!=12)return 3; }
 Base* missing=nullptr;events=0;
 try { Guard guard; (void)typeid(*missing); return 4; } catch(int n) { if(n!=72 || events!=12)return 5; }
 return 0;
}
