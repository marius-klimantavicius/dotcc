//remark:MS properties with Clang ms-extensions
//options:--clang --ms_compatibility;fp

class S {
public:
  __declspec(property(get=GetX,put=PutX)) int x[];
  int GetX(int i, int j) { return i+j; }
  void PutX(int i, int j, int k) { j = i = k; }
};


void test() {
  S *p1 = 0;
  int j = (p1->x)[223][11];
}
