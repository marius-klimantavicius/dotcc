//type:cp
//options_all:--multi_trans -tused --g++
//source_files:tmpl-class-mem-def2-p2.C tmpl-class-mem-def2-p3.C
//require:COMPILE_MULTIPLE_TRANSLATION_UNITS 1

template<int> class iv {};

template<int> struct X {
  enum { N = 0 };
  typedef iv<N> Base;
  void func() { X<5>(); }
};

void m() {
  X<4> s;
  s.func();
}
