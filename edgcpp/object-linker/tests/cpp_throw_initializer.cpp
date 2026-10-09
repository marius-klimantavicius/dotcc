// No standard library or source catch: the host observes propagation and cleanup.
extern "C" void record_event(int);
extern "C" void maybe_fail(int);

struct InitializerTemporary {
    int value;
    explicit InitializerTemporary(int id) : value(id) {
        record_event(id);
        maybe_fail(id);
    }
    ~InitializerTemporary() { record_event(-value); }
};

struct ExceptionPayload {
    int value;
    explicit ExceptionPayload(const InitializerTemporary &input) : value(input.value) {
        record_event(30);
        maybe_fail(30);
    }
    ~ExceptionPayload() { record_event(-30); }
};

int observe_temporary(const InitializerTemporary &value) { return value.value; }

extern "C" void throw_without_outer() {
    throw ExceptionPayload(InitializerTemporary(1));
}

extern "C" void throw_with_temporary(int kind, int condition) {
    InitializerTemporary outer(9);
    if (kind == 0) throw ExceptionPayload(InitializerTemporary(1));
    if (kind == 1) throw ExceptionPayload((observe_temporary(InitializerTemporary(1)), InitializerTemporary(2)));
    if (kind == 2) throw ExceptionPayload((condition ? observe_temporary(InitializerTemporary(1)) : 0, InitializerTemporary(2)));
    if (kind == 3) throw (observe_temporary(InitializerTemporary(1)), (observe_temporary(InitializerTemporary(2)), 71));
    if (kind == 4) throw (observe_temporary(InitializerTemporary(1)), (throw 72, 73));
}
