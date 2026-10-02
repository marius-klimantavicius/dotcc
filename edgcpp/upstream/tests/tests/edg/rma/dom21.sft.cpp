//options_all:-r -x -tused
//options: --strict;cp

// Internal error
struct F;
struct A {
  virtual operator F* ();
};

struct C : virtual  A {};

struct B : A {};

struct D : virtual  B, virtual  C {};

struct E : virtual  D {};

struct F : D {
  operator F *();
};

struct G : E, virtual F {};

