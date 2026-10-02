//type:cp
//options::--microsoft_version 1924
//options_all:--c++11 -tused --multi_trans_unit
//source_files:gnats23243-2.C
//require:COMPILE_MULTIPLE_TRANSLATION_UNITS 1

template <typename T>
class B {
  void operator-();
};

template <typename T>
class D : public B<T> {
  using B<T>::operator-;
};
