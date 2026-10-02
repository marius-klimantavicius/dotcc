//type:fp
//options_all:-tused -A --c++17
//
  typedef int T;
  typedef const T CT;

  void blah2(T *a) {
   a->CT::~T();
  }

//cwg: 1920
//title: Qualification mismatch in pseudo-destructor-name
//meeting: Lenexa 5/15
//edg_status: Passes
