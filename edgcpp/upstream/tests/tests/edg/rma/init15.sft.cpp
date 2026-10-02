//options_all:-r -x -tused
//options: --cfront_3.0;cn

// The stuff in brackets indicates our current behavior and cfront's.  "-"
// means "no diagnostic".  I think I'm inclined to treat the ones marked
// "okay??" as being legal.

// Aggregate class with const member
struct A {
  const int i;
};                       // Warning on no ctor        [edg: W, CC3: E]
A a1 = { 1 };            // Okay
A a2 = a1;               // Okay??                    [edg: -, CC3: E]
A a3;                    // Error                     [edg: E, CC3: -]

//   Note that cfront disallows initializing a2 by a1.  That should be okay
//   for aggregates -- see 8.4.1.  However, 8.4.1 refers to 12.8, and 12.8
//   gives rules for generating copy constructors that would seem to disallow
//   this case, even though this is an aggregate and therefore has no
//   constructors.

//   We deviate from cfront in that there is no error on the class definition
//   but rather on the constructor-less definition of a3.


// Aggregate class with ref member
struct B {
  int& ri;
};                       // Warning on no ctor        [edg: W, CC3: E]
int x = 0;
B b1 = { x };            // Okay??                    [edg: -, CC3: E]
B b2 = b1;               // Error??                   [edg: -, CC3: -]
B b3;                    // Error                     [edg: E, CC3: -]

//   Cfront's error on b1 is a "sorry not implemented" case.

//   If cfront disallows the initialization of a2, shouldn't it also disallow
//   b2?  Or if it allows b2 shouldn't it allow a2?  I think there's more
//   justification for a2 than for b2. . . .

//   We deviate from cfront in that there is no error on the class definition
//   but rather on the constructor-less definition of a3.


// Class with const member and user-defined constructor
struct C {
  const int i;
  C(int ii) : i(ii) { }
};
C c1 = 1;                 // Okay
C c2 = c1;                // Error on C::C(const C&)  [edg: -, CC3: -]
C c3;                     // Error -- C::C()          [edg: E, CC3: E]

//   Neither we nor cfront issues an error on the generation of the copy
//   constructor.  It seems to me that an error is called for.

struct D {
  const int i;
  D(int) { }              // Error                    [edg: W, CC3: E]
};
D d1 = 1;                 // Okay??                   [edg: -, CC3: -]

//   We should change our warning to an error.

struct E {
  const int i;
  E(int);
};
E e1 = 1;                 // Okay??                    [edg: -, CC3: -]
E::E(int) { };            // Error                     [edg: W, CC3: E]
E e2 = 1;                 // Okay??                    [edg: -, CC3: -]

//   We should change our warning to an error.


// Class with ref member and user-defined constructor
struct F {
  int& ri;
  F(int& ii) : ri(ii) { }
};
F f1 = x;                 // Okay
F f2 = f1;                // Error on F::F(const F&)  [edg: -, CC3: -]
F f3;                     // Error -- no F::F()       [edg: E, CC3: E]

//   Neither we nor cfront issues an error on the generation of the copy
//   constructor.  It seems to me that an error is called for.

struct G {
  int& ri;
  G(int&) { }             // Error                    [edg: W, CC3: E]
};
G g1 = x;                 // Okay??                   [edg: -, CC3: -]

//   We should change our warning to an error.

struct H {
  int& ri;
  H(int&);
};
H h1 = x;                 // Okay??                   [edg: -, CC3: -]
H::H(int&) { }            // Error                    [edg: W, CC3: E]
H h2 = x;                 // Okay??                   [edg: -, CC3: -]

//   We should change our warning to an error.

