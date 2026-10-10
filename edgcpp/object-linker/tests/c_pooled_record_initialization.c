struct State { unsigned count; unsigned value; };
struct State state = { 4, 9 };
int main(void) {
    struct State local = { 13, 17 };
    state = (struct State){0};
    local = (struct State){0};
    if (state.count || state.value || local.count || local.value) return 1;
    local = (struct State){19, 23};
    return local.count != 19 || local.value != 23;
}
