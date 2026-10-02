//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn:;rp

extern "C" int printf(char*, ...);

class B;
class D;

class B {
 public:
    B() { }
    B( const B& ) { printf("in cctor (#1)\n"); }     // #1
    B( const D& ) { printf("in non-cctor (#2)\n"); } // #2
};

class D: public B {
} dd;

class Q {
 public:
    operator D () { return dd; }
};

void func1(B) { }

void func2() {
    D d;
    Q q;

    B b( d );         // case 1: #1 or #2?
    B b2 = d;         // case 2: #1 or #2?
    B b3 = q;         // case 3: #1 or #2?

    func1( d );       // case 4: #1 or #2?

    func1( q );       // case 5: #1 or #2?
}

main() {
  func2();
}

