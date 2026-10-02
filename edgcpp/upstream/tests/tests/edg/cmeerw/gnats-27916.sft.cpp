//type:fp
//options:--c++ --gn 150100:--c++ --clang_version 190100

namespace minimal
{
  void *v = __builtin_operator_new(4);
}

namespace std
{
  enum class align_val_t : decltype(sizeof 0);
}

namespace overloaded
{
  void *v = __builtin_operator_new(4, std::align_val_t(8));
  auto l1 = [] () { __builtin_operator_delete(v); };
  auto l2 = [] () { __builtin_operator_delete(v, std::align_val_t(8)); };
};
