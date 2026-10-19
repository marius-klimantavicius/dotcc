typedef struct { long first; long second; } module_state;
int second(void) {
    module_state state = {11, 13};
    return (int)sizeof(state) * 100 + (int)state.first + (int)state.second;
}
