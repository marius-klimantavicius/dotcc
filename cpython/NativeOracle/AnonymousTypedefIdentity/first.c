typedef struct { int value; } module_state;
int first(void) {
    module_state state = {7};
    return (int)sizeof(state) * 100 + state.value;
}
