//type:cp
//options_all:--c++11 -tused --multi_trans_unit
//source_files:gnats21866-2.C
//require:COMPILE_MULTIPLE_TRANSLATION_UNITS 1

template<typename>
struct B {
  enum class ENUM { i };
  virtual ENUM func();
};
B<int> a;
