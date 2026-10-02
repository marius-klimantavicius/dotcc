//type: fp
//options:  --c++20 --modules
# 0 "./modules/block-decl-3_a.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/block-decl-3_a.C"






module;
# 1 "./modules/block-decl-3.h" 1






auto gmf_n_i() {
  struct X { void f() {} };
  return X{};
}

inline auto gmf_i_i() {
  struct X { void f() {} };
  return X{};
}

auto gmf_n_i_i() {
  struct X {
    auto f() {
      struct Y {
 void g() {}
      };
      return Y{};
    }
  };
  return X{};
}

inline auto gmf_i_i_i() {
  struct X {
    auto f() {
      struct Y {
 void g() {}
      };
      return Y{};
    }
  };
  return X{};
}
# 9 "./modules/block-decl-3_a.C" 2

export module mod;

namespace {
  void internal() {}
}



export auto n_n() {
  internal();
  struct X { void f() { internal(); } };
  return X{};
}

export auto n_i() {
  internal();
  struct X { inline void f() {} };
  return X{};
}

export inline auto i_n() {

  struct X { void f() { internal(); } };
  return X{};
}

export inline auto i_i() {
  struct X { inline void f() {} };
  return X{};
}




export auto n_n_n() {
  struct X {
    auto f() {
      struct Y {
 void g() { internal(); }
      };
      return Y{};
    }
  };
  return X{};
}

export auto n_i_n() {
  struct X {
    inline auto f() {
      struct Y {
 void g() { internal(); }
      };
      return Y{};
    }
  };
  return X{};
}

export inline auto i_n_i() {
  struct X {
    auto f() {
      struct Y {
 inline void g() {}
      };
      return Y {};
    }
  };
  return X{};
}

export inline auto i_i_i() {
  struct X {
    inline auto f() {
      struct Y {
 inline void g() {}
      };
      return Y{};
    }
  };
  return X{};
}




export auto multi_n_n() {
  struct X {
    void f() { internal(); }
  };
  struct Y {
    X x;
  };
  return Y {};
}

export auto multi_n_i() {
  struct X {
    inline void f() {}
  };
  struct Y {
    X x;
  };
  return Y {};
}

export inline auto multi_i_i() {
  struct X {
    inline void f() {}
  };
  struct Y {
    X x;
  };
  return Y {};
};




export extern "C++" auto extern_n_i() {
  struct X {
    void f() {}
  };
  return X{};
};

export extern "C++" inline auto extern_i_i() {
  struct X {
    void f() {}
  };
  return X{};
};




export using ::gmf_n_i;
export using ::gmf_i_i;
export using ::gmf_n_i_i;
export using ::gmf_i_i_i;




auto only_used_in_impl() {
  struct X { void f() {} };
  return X{};
}
export void test_from_impl_unit();
