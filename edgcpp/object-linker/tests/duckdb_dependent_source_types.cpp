// An unused template still describes a public dependent type and constant.
template<class Engine> struct Adapter {
    typedef typename Engine::result_type result_type;
    static constexpr result_type minimum = Engine::min();
};
struct ConcreteEngine {
    typedef unsigned result_type;
    static constexpr result_type min() { return 7; }
};
int main() { return Adapter<ConcreteEngine>::minimum == 7 ? 0 : 1; }
