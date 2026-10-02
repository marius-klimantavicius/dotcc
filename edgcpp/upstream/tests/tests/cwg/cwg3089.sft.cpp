//type:fp
//options:--c++11:--c++26 --set_flag reflection -DREFLECTION
//options_all:-A -tused

constexpr decltype(nullptr) np;

#if defined(REFLECTION)
namespace std::meta {
  using info = decltype(^^::);
}

constexpr std::meta::info mi;
#endif

//cwg: 3089
//title: const-default-constructible improperly handles std::meta::info
//meeting: Kona 11/25
//edg_status: EDGcpfe/28547
