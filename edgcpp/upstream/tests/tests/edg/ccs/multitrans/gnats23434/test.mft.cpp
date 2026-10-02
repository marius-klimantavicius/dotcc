//type:fp
//options::--microsoft
//options_all:--multi_trans -tused --c++14
//source_files:gnats23434-p2.C
//require:COMPILE_MULTIPLE_TRANSLATION_UNITS 1

template<typename> struct X {
  template<typename> void func();
};

template<typename T>
struct Y : public X<T> {
  using X<T>::func;
};

