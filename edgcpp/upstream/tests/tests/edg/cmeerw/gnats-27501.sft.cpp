//type:fp
//options:--c++17:--c++20:--c++20 --gn 140100:--c++20 --clang_version 190100:--ms_c++20 --microsoft_version 1936
//options_all:-w

namespace minimal
{
  int i = [] (auto ... a) {
    return [&] (auto b) {
      return [&] (auto c) {
        return (a , ...);
      } (3);
    } (2);
  } (1);
}

namespace enclosing_function
{
  template<typename ... Ts>
  constexpr int f(Ts ... a) {
    return [&] (auto b) {
      return [&] (auto c) {
        return (a , ...);
      } (3);
    } (2);
  }
  static_assert(f(1) == 1);
}

namespace minimal_non_deduced_return
{
  static_assert([] (auto ... a) -> int {
    return [&] (auto b) -> int {
      return [&] (auto c) -> int {
        return (a , ...);
      } (3);
    } (2);
  } (1) == 1);
}

namespace PACK_AND_PACK
{
  static_assert([] (auto ... a) -> int {
    return [&] (auto ... b) -> int {
      return [&] (auto ... c) -> int {
        return (a , ...);
      } (3);
    } (2);
  } (1, 2, 3) == 3);
}

#ifdef __cpp_concepts
namespace PACK_AND_PACK_EXPL_PARAMS
{
  static_assert([]<typename ... As> (As ... a) -> int {
    return [&]<typename ... Bs> (Bs ... b) -> int {
      return [&]<typename ... Cs> (Cs ... c) -> int {
        (Cs(c) , ...);
        (Bs(b) , ...);

        (Cs(b) , ...);
        (Bs(c) , ...);

        return (As(a) , ...);
      } (3);
    } (2);
  } (1, 2, 3) == 3);
}
#endif

namespace PACK_AND_NON_PACK
{
  static_assert([] (auto ... a) -> int {
    return [&] (auto ... b) -> int {
      return [&] (auto c) -> int {
        return (a , ...);
      } (3);
    } (2);
  } (1, 2, 3) == 3);
}

namespace NON_PACK_AND_PACK
{
  static_assert([] (auto ... a) -> int {
    return [&] (auto b) -> int {
      return [&] (auto ... c) -> int {
        return (a , ...);
      } (3);
    } (2);
  } (1, 2, 3) == 3);
}

namespace EXPAND_INNER_PACKS
{
  static_assert([] (auto ... a) -> int {
    return [&] (auto ... b) -> int {
      return [&] (auto ... c) -> int {
        return ((b + c) + ...);
      } (3, 4);
    } (2, 3);
  } (1) == 12);
}
