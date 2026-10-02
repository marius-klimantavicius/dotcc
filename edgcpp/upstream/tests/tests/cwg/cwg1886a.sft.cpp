//type:fn
//options_all:-tused -A --c++17
//
  int main() {}
  namespace N { extern "C" int main = 0; }

//cwg: 1886
//title: Language linkage for main()
//meeting: Lenexa 5/15
//edg_status: Passes
