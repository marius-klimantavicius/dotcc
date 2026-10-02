//type:cp
//options_all:--c++17 --multi_trans_unit
//source_files:gnats20997-p2.C
//require:COMPILE_MULTIPLE_TRANSLATION_UNITS 1

namespace std {
  typedef decltype(sizeof(0)) size_t;
  enum class align_val_t : size_t { };
}

void* operator new  ( std::size_t count, std::align_val_t al);
