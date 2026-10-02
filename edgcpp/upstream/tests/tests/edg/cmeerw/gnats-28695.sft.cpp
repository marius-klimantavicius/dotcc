//type:fp
//options:--c++23:--c++23 --gn 150200:--c++23 --clang_version 210100
//options_all:-w

namespace minimal {
  auto l = []<typename> requires true -> void { };
}

namespace templ_params
{
  auto l1 = []<typename> { };
  auto l2 = []<typename> -> void { };

  auto l3 = []<typename> noexcept { };
  auto l4 = []<typename> noexcept -> void { };

  auto l5 = []<typename> [[]] { };
  auto l6 = []<typename> [[]] -> void { };

  auto l7 = []<typename> requires true { };
  auto l8 = []<typename> requires true -> void { };

  auto l9 = []<typename> requires true noexcept { };
  auto l10 = []<typename> requires true noexcept -> void { };

  auto l11 = []<typename> requires true [[]] { };
  auto l12 = []<typename> requires true [[]] -> void { };
}

namespace no_templ_params
{
  auto l1 = [] { };
  auto l2 = [] -> void { };

  auto l3 = [] noexcept { };
  auto l4 = [] noexcept -> void { };

  auto l5 = [] [[]] { };
  auto l6 = [] [[]] -> void { };
}
