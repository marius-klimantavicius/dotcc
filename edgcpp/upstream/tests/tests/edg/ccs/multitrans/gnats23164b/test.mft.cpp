//type:fn
//options_all:--c++11 -tused --multi_trans_unit
//source_files:gnats23164b-p2.C
//require:COMPILE_MULTIPLE_TRANSLATION_UNITS 1

namespace std {
template <typename, typename> class vector;
template <typename a> class vector<bool, a> { template <typename> struct b; };
template <typename> struct hash;
}
