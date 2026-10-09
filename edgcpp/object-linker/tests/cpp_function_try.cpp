extern "C" void record_event(int);
extern "C" int should_fail(int);
extern "C" int current_mode();
struct FTGuard {
    int value;
    explicit FTGuard(int n):value(n) { record_event(n); }
    ~FTGuard() { record_event(-value); }
};
static void inspect_guard(const FTGuard &value) { record_event(value.value+1); }
struct FTMember {
    int value;
    explicit FTMember(int n):value(n) { record_event(n); if(should_fail(n)) throw n; }
    ~FTMember() noexcept(false) { record_event(-value); if(should_fail(value+20)) throw value+20; }
};
struct FTObject:FTMember {
    FTMember member;
    explicit FTObject(int mode) try:FTMember(1),member(2) {
        record_event(3); if(should_fail(3)) throw 3;
    } catch(int value) {
        FTGuard local(4); record_event(100+value);
        if(mode==1) throw 99;
        // Reaching the end of this handler implicitly rethrows.
    }
    ~FTObject() noexcept(false) try {
        record_event(10);
        if(should_fail(10) || current_mode()==3) throw 10;
    } catch(int value) {
        FTGuard local(5); record_event(200+value);
        if(current_mode()==3) return;
        if(current_mode()==1) throw 98;
        // Reaching the end implicitly rethrows after members/bases are gone.
    }
};
struct FTDerived:FTObject {
    FTMember last;
    explicit FTDerived(int mode):FTObject(mode),last(7) {}
    ~FTDerived() noexcept(false) {}
};
struct FTDelegate {
    FTMember member;
    FTDelegate():member(8) { record_event(80); }
    explicit FTDelegate(int) try:FTDelegate() {
        record_event(81);if(should_fail(3)) throw 3;
    } catch(int value) { record_event(500+value); }
    ~FTDelegate() noexcept(false) { record_event(82); }
};
struct FTNoexcept {
    FTMember member;
    FTNoexcept():member(8) {}
    ~FTNoexcept() noexcept try { record_event(70);throw 70; }
    catch(int value) { record_event(600+value);return; }
};
extern "C" int function_try_case(int mode) {
    FTGuard outer(9);
    inspect_guard(FTGuard(15));
    try {
        if(mode==4) { FTDelegate object(1);record_event(6); }
        else if(mode==5) { FTNoexcept object;record_event(6); }
        else if(mode==2) { FTDerived object(mode); record_event(6); }
        else { FTObject object(mode); record_event(6); }
    } catch(int value) {
        record_event(300+value);return value;
    }
    return 0;
}
