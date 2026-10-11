struct Empty { constexpr Empty(int) {} };
struct Pair {
    int first;
    int second;
    constexpr Pair(int left, int right) : first(left), second(right) {}
};
struct Packed : Empty {
    Pair pair;
    int tail;
    constexpr Packed(int left, int right) : Empty(1), pair(left, right), tail(17) {}
};
struct Plain { int first; int second; };
struct Holder {
    Pair direct = Pair(5, 7);
    Packed nested = Packed(11, 13);
    Plain partial = {19};
    int trailing = 23;
    Holder();
    explicit Holder(int chosen);
};
int provider_defaults();
