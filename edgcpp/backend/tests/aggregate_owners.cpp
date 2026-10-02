struct State { int counter; long serial; int values[4]; State *self; };
State state = {10, 37, {2, 3, 5, 7}, &state};
int *interior = &state.values[2];
unsigned long previous = 0;
int inspect(State *p) { return p->values[2]; }
int main() {
    if(state.self!=&state || interior!=&state.values[2]) return 101;
    if(previous && previous!=reinterpret_cast<unsigned long>(&state)) return 102;
    previous=reinterpret_cast<unsigned long>(&state);
    State copy=state;
    copy.counter=99;
    if(copy.self!=&state || state.counter==99 || copy.serial!=37) return 103;
    for(int i=0;i<200000;++i) {
        if(inspect(state.self)!=5 || *interior!=5) return 104;
    }
    return ++state.counter;
}
