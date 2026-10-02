//options_all:-r -x -tused
//options: --strict --diag_suppress=1055 --diag_suppress=2458;cp

static union {
  struct B;
};
struct B {};
class X {
  union {
    struct B;
  };
  struct B { };
};
class Y {
  union {
    union {
      union {
        struct B;
      };
    };
  };
  struct B { };
};
void f() {
  union {
    struct B;
  };
  struct B { };
  class X {
    union {
      struct B;
    };
    struct B { };
  };
}

