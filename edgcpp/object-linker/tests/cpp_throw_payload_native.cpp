#include "cpp_throw_payload.cpp"
extern "C" int printf(const char *, ...);
extern "C" int puts(const char *);
static int selected, trace[128], count;
extern "C" void record_event(int value) { trace[count++] = value; }
extern "C" int should_fail(int value) { return value == selected; }
int main() {
    const int failures[] = {0, 1, 2, 3, 9};
    for (int kind = 0; kind != 8; ++kind)
        for (int failure : failures) {
            selected = failure; count = 0;
            try { if (kind == 7) throw_single_payload(); else throw_payload(kind); }
            catch (int value) { record_event(100 + value); }
            catch (const PairPayload &) { record_event(200); }
            catch (const TriplePayload &) { record_event(200); }
            catch (const NestedPayload &) { record_event(200); }
            catch (const SinglePayload &) { record_event(200); }
            catch (const EmptyBasePayload &) { record_event(200); }
            printf("%d:%d", kind, failure);
            for (int i = 0; i != count; ++i) printf(" %d", trace[i]);
            puts("");
        }
}
