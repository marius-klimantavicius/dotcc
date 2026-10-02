//type:fn
//options_all:--c++17 -tused -A
//
namespace std {
typedef decltype (sizeof(0)) size_t;

enum class align_val_t : std::size_t {};
}


struct S {
// Placement allocation function:
static void* operator new(std::size_t, std::size_t);
// Usual (non-placement) deallocation function:
static void operator delete(void*, std::size_t);
};
S* p = new (0) S; // ill-formed: non-placement deallocation function matches
// placement allocation function

//cwg: 2280
//title: Matching a usual deallocation function with placement new
//meeting: Belfast 11/19
//edg_status: Passes
