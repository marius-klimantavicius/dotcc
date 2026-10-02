  //type:fn
  //options_all:--c++20 -tused -A
  struct A {
    operator int() const { return 10; }
  };

  bool operator==(A, int);               // #1
  // built-in: bool operator==(int, int);   // #2
  bool b = 10 == A();                   // uses #1 with reversed order of arguments; previously used #2

  struct B {
    bool operator==(const B&);          // member function with no cv-qualifier
  };
  B b1;
  bool eq = (b1 == b1);                   // ambiguous; previously well-formed

//cwg: 2407
//title: Missing entry in Annex C for defaulted comparison operators
//meeting: Kona 11/22
//edg_status: Passes
