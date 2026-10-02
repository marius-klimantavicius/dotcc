//type:fn
//options_all:--c++20 -tused
#include <typeinfo>
typedef __EDG_SIZE_TYPE__ size_t;
template <typename T, size_t N>
constexpr size_t array_size(T (&)[N]) {
    return N;
}


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
    Swim* pswam = &swam;
    static_assert(pswam->phelps() == 28);   // error: lvalue-to-rvalue conversion on a pointer
                                            // not usable in constant expressions
    static_assert(swam.lochte() == 12);     // error: invoking virtual function on reference
                                            // with constexpr-unknown dynamic type
    static_assert(swam.coughlin == 12);     // error: lvalue-to-rvalue conversion on an object
                                            // not usable in constant expressions
}

extern Swim dc;
extern Swim& trident;

constexpr auto& gallagher = typeid(trident);    // error: constexpr-unknown dynamic type
