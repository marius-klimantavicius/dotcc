//remark:MS properties with Clang ms-extensions
//options:--clang --ms_compatibility;fp

  struct S {
    __declspec(property(get=get_x, put=put_x)) int x[];
    int get_x(int i, int j);
    void put_x(int i, int j, int k);
  };
  int main() {
    S *p = 0;
    return p->x[1][2];
  }

