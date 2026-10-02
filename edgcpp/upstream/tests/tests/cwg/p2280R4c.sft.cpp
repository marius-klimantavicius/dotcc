//options_all:--c++20 -tused -A
#include <typeinfo>

typedef __EDG_SIZE_TYPE__ size_t;
template <typename T, size_t N>
constexpr size_t array_size(T (&)[N]) {
    return N;
}

void use_array(int const (&gold_medal_mel)[2]) {
    constexpr auto gold = array_size(gold_medal_mel); // ok
}

constexpr auto olympic_mile() {
  const int ledecky = 1500;
  return []{ return ledecky; };
}
static_assert(olympic_mile()() == 1500); // ok

struct Swim {
    constexpr int phelps() { return 28; }
    virtual constexpr int lochte() { return 12; }
    int coughlin = 12;
};

constexpr int how_many(Swim& swam) {
    Swim* p = &swam;
    return (p + 1 - 1)->phelps();
}

void splash(Swim& swam) {
    static_assert(swam.phelps() == 28);     // ok
    static_assert((&swam)->phelps() == 28); // ok
    Swim* pswam = &swam;
    static_assert(how_many(swam) == 28);    // ok
    static_assert(Swim().lochte() == 12);   // ok
}

extern Swim dc;
extern Swim& trident;

constexpr auto& sandeno   = typeid(dc);         // ok: can only be typeid(Swim)
