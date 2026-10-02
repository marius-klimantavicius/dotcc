//options_all:-r -x -tused
//options: --strict;fn

// from rfg
struct S1 {
  enum E1 { red, green, blue };
};
struct S2 : public S1 {
  class E1 *x;
//  friend class E1;
};

