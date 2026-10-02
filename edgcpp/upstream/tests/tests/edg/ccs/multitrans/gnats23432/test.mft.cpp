//type:cp
//options::-DCONSTEXPR=const
//options_all:--c++14 --multi_trans_unit
//source_files:gnats23432-p2.C
//require:COMPILE_MULTIPLE_TRANSLATION_UNITS 1

#ifndef CONSTEXPR
#define CONSTEXPR constexpr
#endif
template<typename> bool X;
template<typename T> struct Y {
  static CONSTEXPR bool var = X<T>;
};
