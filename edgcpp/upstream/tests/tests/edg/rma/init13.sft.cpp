//options_all:-r -x -tused
//options: --strict;cn

struct A {const int i;};
A a = {1};
A a1;
int i;
struct B {int &ri;};
B b = {i};
B b1;
struct C {const int i; int &ri; };
C c = {1, i};
C c1 = {1};
C c2;
C ca[2] = {1, i, 1, i};
C ca1[2] = {1, i};
C ca2[2] = {1, i, 1};
struct D { int i; const int j; int k; };
D d = { 1, 1, 1 };
D d1;
D d2 = { 1 };                    // Now okay -- j is default initialized
D da[2] = { 1, 1, 1, 1, 1, 1 };
D da1[2] = { 1, 1, 1, 1, 1 };    // Okay
D da2[2] = { 1, 1, 1, 1 };       // Now okay -- j is default initialized
D da3[2] = { 1, 1, 1 };
struct E { int i; int& ri; int j; };
E e = { 1, i, 1 };
E e1;
E e2 = { 1 };
E ea[2] = { 1, i, 1, 1, i, 1 };
E ea1[2] = { 1, i, 1, 1, i };    // Okay
E ea2[2] = { 1, i, 1, 1 };
E ea3[2] = { 1, i, 1 };
struct F { A a; B b; C c; };
F f = { 1, i, 1, i };
F f1;
F f2 = { 1, i, 1 };
struct G { A a[2]; };
G g = { 1, 1 };
G g1;
G g2 = { 1 };

