//type:fp
//options_all:--c++17 -A -tused
  struct B {
    int i;
  };

  struct D : B {
    int j;
    B b;
  };

  int main() {
    D d;

    B &br = d.b;
    D &dr = static_cast<D&>(br);  // Okay?
  }

//cwg: 2224
//title: Member subobjects and base-class casts
//meeting: Kona 2/17
//edg_status: Passes
