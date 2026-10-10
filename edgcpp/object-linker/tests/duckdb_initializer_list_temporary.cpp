namespace std {
template<class T> class initializer_list {
    const T *first;
    unsigned long length;
    constexpr initializer_list(const T *data, unsigned long size) : first(data), length(size) {}
public:
    constexpr initializer_list() : first(nullptr), length(0) {}
    constexpr unsigned long size() const { return length; }
    constexpr const T *begin() const { return first; }
    constexpr const T *end() const { return first + length; }
};
}
static int events[32];
static int count;
static int throwing;
struct Member {
    int value;
    Member(int n) : value(n) { events[count++] = n; if(n == throwing) throw n; }
    ~Member() { events[count++] = -value; }
};
struct Item { int tag; Member member; };
int consume(std::initializer_list<Item> values) {
    int sum = 0;
    for (const auto &value : values) sum += value.member.value;
    return sum;
}
int main() {
    int checks = 0;
    count = 0;
    if(consume({{0,1},{0,2},{0,3}}) == 6) ++checks;
    if(count == 6) ++checks;
    if(events[0] == 1 && events[1] == 2 && events[2] == 3) ++checks;
    if(events[3] == -3 && events[4] == -2 && events[5] == -1) ++checks;
    count = 0; throwing = 2;
    try { consume({{0,1},{0,2},{0,3}}); return 10; }
    catch(int n) { if(n == 2) ++checks; }
    if(count == 3) ++checks;
    if(events[0] == 1 && events[1] == 2 && events[2] == -1) ++checks;
    return checks == 7 ? 0 : 20 + checks;
}
