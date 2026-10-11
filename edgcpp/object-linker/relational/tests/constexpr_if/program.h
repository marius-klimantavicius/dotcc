extern int trace;
extern int alive;
int mark(int event);
int runtime_predicate();

constexpr bool predicate(bool value) {
    return value ? true : (false && runtime_predicate());
}

struct Guard {
    int event;
    explicit Guard(int selected);
    ~Guard();
};

template<bool Selected>
int choose() {
    if constexpr (predicate(Selected)) return mark(1);
    else return mark(2);
}

template<bool Selected>
int initialized(int event) {
    if constexpr (Guard guard(event); predicate(Selected)) return mark(5);
    else return mark(8);
}

template<bool Selected>
int throwing(int event) {
    if constexpr (Guard guard(event); predicate(Selected)) {
        mark(9);
        throw 17;
    } else return mark(8);
}
