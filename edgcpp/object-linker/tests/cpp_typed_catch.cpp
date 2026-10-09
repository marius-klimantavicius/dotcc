extern "C" void record_event(int);
extern "C" int should_fail(int);
struct TypedGuard {
    int value;
    explicit TypedGuard(int n):value(n) { record_event(n); }
    ~TypedGuard() { record_event(-value); }
};
struct Prefix { int prefix; Prefix():prefix(11) {} };
struct TypedBase {
    int value;
    explicit TypedBase(int n):value(n) { record_event(20); }
    TypedBase(const TypedBase &other):value(other.value) { record_event(21); if(should_fail(21)) throw 99; }
    ~TypedBase() { record_event(-20); }
};
struct TypedDerived:Prefix,TypedBase {
    int extra;
    TypedDerived():TypedBase(7),extra(8) { record_event(30); }
    ~TypedDerived() { record_event(-30); }
};
extern "C" int typed_case(int mode) {
    TypedGuard outer(9);
    if(mode==0) {
        try { throw 5; }
        catch(double wrong) { return 100; }
        catch(int value) { record_event(40); return value; }
        catch(...) { return 101; }
    }
    if(mode==1) {
        try { try { throw 6; } catch(int &value) { ++value; record_event(41); throw; } }
        catch(int value) { return value; }
    }
    if(mode==2 || mode==3) {
        try { throw TypedDerived(); }
        catch(const TypedBase &value) { TypedGuard local(2); record_event(42); return value.value; }
        catch(...) { return 102; }
    }
    if(mode==4 || mode==5) {
        try { throw TypedDerived(); }
        catch(TypedBase value) { TypedGuard local(3); record_event(43); if(mode==5) throw; return value.value; }
    }
    if(mode==6) {
        TypedDerived object;
        try { throw &object; }
        catch(TypedBase *value) { return value->value; }
    }
    if(mode==7) {
        int value=12;
        try { throw &value; }
        catch(const int *pointer) { return *pointer; }
    }
    if(mode==8) {
        try { throw 1.5; }
        catch(float wrong) { return 103; }
        catch(double value) { return (int)(value*10); }
    }
    if(mode==9) {
        try { throw 17U; }
        catch(int wrong) { return 104; }
        catch(...) { return 18; }
    }
    if(mode==10) {
        try { throw TypedDerived(); }
        catch(TypedBase &base) {
            base.value=22;
            try { throw; } catch(const TypedDerived &value) { return value.value; }
        }
    }
    if(mode==11) {
        TypedDerived object;
        try {
            try { throw &object; }
            catch(TypedBase *value) { value=0; throw; }
        } catch(TypedDerived *value) { return value->extra; }
    }
    if(mode==12) {
        try { throw TypedDerived(); }
        catch(const TypedDerived &) { return 23; }
        catch(const TypedBase &) { return 105; }
    }
    if(mode==13) {
        try { throw 19L; }
        catch(int wrong) { return 106; }
    }
    return 0;
}
