//type:fn
//options_all:--c++17 -tused -A
  template<typename T> struct Friendly {
    template<typename U> friend int f(U) { return sizeof(T); }
  };
  Friendly<char> fc;
  Friendly<float> ff; // ill-formed: produces second definition of f(U)

//cwg: 2174
//title: Unclear rules for friend definitions in templates
//meeting: Kona 2/17
//edg_status: Passes
