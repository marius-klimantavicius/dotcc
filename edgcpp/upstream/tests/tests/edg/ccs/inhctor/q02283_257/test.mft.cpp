//remark:Inheriting constructors and multi-TU
//type:fp
//options_all:--c++11 -tused --multi_trans_unit
//source_files:m2.c
//require:COMPILE_MULTIPLE_TRANSLATION_UNITS 1

template<typename> struct B;
template<typename T> struct D: B<T> { 
  using B<T>::B;  // Dependent inheriting constructor.
};

