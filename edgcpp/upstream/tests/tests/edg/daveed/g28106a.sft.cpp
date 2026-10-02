//remark:Copying zero-length arrays
//options:--g++;rp

  int N = 0;
  struct I {
    int x;
    I(): x(0) {}
    I(I const &orig): x(orig.x) { N++; }
  };
  struct S {
    int m;
    I i[0];
  };
  int main() {
    S s;
    S copy(s);
    return N != 0;
  }
