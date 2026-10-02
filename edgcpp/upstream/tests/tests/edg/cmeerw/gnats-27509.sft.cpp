//type:fp
//options:--c++20:--c++20 --gn 140200:--c++20 --clang_version 190100:--ms_c++20 --microsoft_version 1942

namespace pr
{
  template<typename, typename>
  concept X  = true;

  template<typename ... Vs>
  struct C {
    template<typename ... Us> struct G {
      static int f() requires (X<Us, Vs> || ...);
    };

    template<typename ... Us>
    static int f() requires (X<Us, Vs> || ...);
  };

  int i = C<int, char>::G<int, short>::f();  // spurious error
  int j = C<int, short>::f<int, char>();     // okay
}
