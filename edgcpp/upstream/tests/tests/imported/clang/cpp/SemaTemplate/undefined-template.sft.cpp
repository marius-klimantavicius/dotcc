//type: fp
//options:  --c++14
# 1 "SemaTemplate/undefined-template.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 461 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaTemplate/undefined-template.cpp" 2



template <class T> struct C1 {
  static char s_var_1;
  static char s_var_2;
  static void s_func_1();
  static void s_func_2();
  void meth_1();
  void meth_2();
  template <class T1> static char s_tvar_2;
  template <class T1> static void s_tfunc_2();
  template<typename T1> struct C2 {
    static char s_var_2;
    static void s_func_2();
    void meth_2();
    template <class T2> static char s_tvar_2;
    template <class T2> void tmeth_2();
  };
};

extern template char C1<int>::s_var_2;
extern template void C1<int>::s_func_2();
extern template void C1<int>::meth_2();
extern template char C1<int>::s_tvar_2<char>;
extern template void C1<int>::s_tfunc_2<char>();
extern template void C1<int>::C2<long>::s_var_2;
extern template void C1<int>::C2<long>::s_func_2();
extern template void C1<int>::C2<long>::meth_2();
extern template char C1<int>::C2<long>::s_tvar_2<char>;
extern template void C1<int>::C2<long>::tmeth_2<char>();

char func_01() {
  return C1<int>::s_var_2;
}

char func_02() {
  return C1<int>::s_var_1;

}

char func_03() {
  return C1<char>::s_var_2;

}

void func_04() {
  C1<int>::s_func_1();

}

void func_05() {
  C1<int>::s_func_2();
}

void func_06() {
  C1<char>::s_func_2();

}

void func_07(C1<int> *x) {
  x->meth_1();

}

void func_08(C1<int> *x) {
  x->meth_2();
}

void func_09(C1<char> *x) {
  x->meth_1();

}

char func_10() {
  return C1<int>::s_tvar_2<char>;
}

char func_11() {
  return C1<int>::s_tvar_2<long>;

}

void func_12() {
  C1<int>::s_tfunc_2<char>();
}

void func_13() {
  C1<int>::s_tfunc_2<long>();

}

char func_14() {
  return C1<int>::C2<long>::s_var_2;
}

char func_15() {
  return C1<int>::C2<char>::s_var_2;

}

void func_16() {
  C1<int>::C2<long>::s_func_2();
}

void func_17() {
  C1<int>::C2<char>::s_func_2();

}

void func_18(C1<int>::C2<long> *x) {
  x->meth_2();
}

void func_19(C1<int>::C2<char> *x) {
  x->meth_2();

}

char func_20() {
  return C1<int>::C2<long>::s_tvar_2<char>;
}

char func_21() {
  return C1<int>::C2<long>::s_tvar_2<long>;

}

void func_22(C1<int>::C2<long> *x) {
  x->tmeth_2<char>();
}

void func_23(C1<int>::C2<long> *x) {
  x->tmeth_2<int>();

}

namespace test_24 {
  template <typename T> struct X {
    friend void g(int);
    operator int() { return 0; }
  };
  void h(X<int> x) { g(x); }
}


# 1 "SemaTemplate/undefined-template.cpp" 1
# 157 "SemaTemplate/undefined-template.cpp" 3
template <typename T> struct SystemHeader { T meth(); };
# 148 "SemaTemplate/undefined-template.cpp" 2
void func_25(SystemHeader<char> *x) {
  x->meth();
}

int main() {
  return 0;
}
