extern "C" void record_event(int);
struct ProductionShared {
    int value;
    explicit ProductionShared(int n):value(n) { record_event(10+n); }
    ~ProductionShared() { record_event(-10-value); }
};
extern "C" void production_cross_throw(int);
extern "C" int production_cross_catch(int);
