extern "C" void record_event(int);
extern "C" int should_fail(int);
struct CatchGuard {
    int value;
    explicit CatchGuard(int id) : value(id) { record_event(id); if (should_fail(id)) throw id; }
    ~CatchGuard() { record_event(-value); }
};
struct CatchPayload {
    int value;
    explicit CatchPayload(int id) : value(id) { record_event(30); }
    ~CatchPayload() { record_event(-30); }
};
void rethrow_from_call() { throw; }
extern "C" int catch_all_case(int mode) {
    CatchGuard outer(9);
    try {
        CatchGuard inner(1);
        if (mode == 1) return 11;
        if (mode != 0) throw CatchPayload(7);
    } catch (...) {
        CatchGuard handler(2);
        record_event(100);
        if (mode == 2) throw;
        if (mode == 3) throw 8;
        if (mode == 4) rethrow_from_call();
        if (mode == 5) {
            try { CatchGuard nested(3); throw; }
            catch (...) { CatchGuard again(4); record_event(101); }
            record_event(102);
        }
        if (mode == 6) return 12;
        if (mode == 7) {
            try { throw 10; }
            catch (...) { record_event(103); }
        }
        if (mode == 8) goto done;
    }
    record_event(200);
done:
    return 13;
}
extern "C" int catch_all_without_locals(int mode) {
    try { if (mode) throw 14; }
    catch (...) { record_event(104); return 15; }
    return 16;
}
extern "C" int catch_all_loop(int mode) {
    int count = 0;
    for (int i = 0; i != 3; ++i) {
        try { CatchGuard item(1); throw 15; }
        catch (...) {
            CatchGuard item(2); ++count;
            if (mode) break;
            continue;
        }
    }
    return count;
}
struct CatchExitGuard {
    explicit CatchExitGuard(int) { record_event(6); }
    ~CatchExitGuard() noexcept(false) { record_event(-6); if (should_fail(6)) throw 6; }
};
// EDG places the outer destruction after the try-exit marker on this return.
// Its exception must escape, even though that call appears inside C#'s try.
extern "C" int catch_exit_boundary() {
    CatchExitGuard outer(6);
    try { return 42; }
    catch (...) { record_event(105); return 43; }
}
extern "C" int catch_handler_exit_failure() {
    try { throw CatchPayload(7); }
    catch (...) { CatchExitGuard inner(6); return 44; }
}
