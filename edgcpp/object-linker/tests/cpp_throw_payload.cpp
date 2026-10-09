// Partially constructed exception objects: no standard-library dependency.
extern "C" void record_event(int);
extern "C" int should_fail(int);
struct PayloadPart {
    int value;
    explicit PayloadPart(int id) : value(id) {
        record_event(id);
        if (should_fail(id)) throw id;
    }
    ~PayloadPart() { record_event(-value); }
};
struct PairPayload {
    PayloadPart first, second;
    ~PairPayload() { record_event(-30); }
};
struct TriplePayload {
    PayloadPart first, second, third;
    ~TriplePayload() { record_event(-40); }
};
struct NestedPayload {
    PairPayload pair;
    PayloadPart tail;
    ~NestedPayload() { record_event(-50); }
};
struct DerivedPayload : PairPayload {
    PayloadPart tail;
    ~DerivedPayload() { record_event(-60); }
};
struct SinglePayload {
    PayloadPart only;
    ~SinglePayload() { record_event(-70); }
};
struct EmptyPayloadBase {
    ~EmptyPayloadBase() { record_event(-80); }
};
struct EmptyBasePayload : EmptyPayloadBase {
    PayloadPart first, second;
    ~EmptyBasePayload() { record_event(-90); }
};
extern "C" void throw_payload(int kind) {
    PayloadPart outer(9);
    if (kind == 0) throw PairPayload{PayloadPart(1), PayloadPart(2)};
    if (kind == 1) throw TriplePayload{PayloadPart(1), PayloadPart(2), PayloadPart(3)};
    if (kind == 2) throw NestedPayload{{PayloadPart(1), PayloadPart(2)}, PayloadPart(3)};
    if (kind == 3) throw DerivedPayload{{PayloadPart(1), PayloadPart(2)}, PayloadPart(3)};
    if (kind == 4) throw SinglePayload{PayloadPart(1)};
    if (kind == 5) throw PairPayload{PayloadPart(1), PayloadPart((throw 77, 2))};
    if (kind == 6) throw EmptyBasePayload{{}, PayloadPart(1), PayloadPart(2)};
}
// No local cleanup table is needed for the sole/final payload member.
extern "C" void throw_single_payload() { throw SinglePayload{PayloadPart(1)}; }
