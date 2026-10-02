//type:fn
//options_all:--c++17 -tused -A 
//
struct Banana { };
struct Enigma { operator const Banana(); };
struct Alaska { operator Banana&(); };
void enigmatic() {
typedef const Banana ConstBanana;
Banana &&banana1 = ConstBanana(); // ill-formed
Banana &&banana2 = Enigma(); // ill-formed
Banana &&banana3 = Alaska(); // ill-formed
}
const double& rcd2 = 2; // rcd2 refers to temporary with value 2.0
double&& rrd = 2; // rrd refers to temporary with value 2.0
const volatile int cvi = 1;
const int& r2 = cvi; // error: cv-qualifier dropped
struct A { operator volatile int&(); } a;
const int& r3 = a; // error: cv-qualifier dropped
// from result of conversion function
double d2 = 1.0;
double&& rrd2 = d2; // error: initializer is lvalue of related type
struct X { operator int&(); };
int&& rri2 = X(); // error: result of conversion function is lvalue of related type
int i3 = 2;
double&& rrd3 = i3; // rrd3 refers to temporary with value 2.0

//cwg: 1571
//title: cv-qualification for indirect reference binding via conversion function
//meeting: Urbana-Champaign 11/14
//edg_status: Passes
