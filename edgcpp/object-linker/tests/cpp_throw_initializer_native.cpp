#include "cpp_throw_initializer.cpp"
extern "C" int printf(const char *, ...);
extern "C" int puts(const char *);

struct Failure { int point; };
static int selected;
static int trace[128], count;
extern "C" void record_event(int value) { trace[count++] = value; }
extern "C" void maybe_fail(int value) { if (value == selected) throw Failure{value}; }

int main() {
    const int failures[] = {0, 1, 2, 9, 30};
    for (int kind = 0; kind != 6; ++kind)
        for (int condition = 0; condition != 2; ++condition)
            for (int failure : failures) {
                selected = failure; count = 0;
                try {
                    if (kind == 5) throw_without_outer();
                    else throw_with_temporary(kind, condition);
                }
                catch (Failure value) { record_event(100 + value.point); }
                catch (const ExceptionPayload &value) { record_event(200 + value.value); }
                catch (int value) { record_event(300 + value); }
                printf("%d:%d:%d", kind, condition, failure);
                for (int i = 0; i != count; ++i) printf(" %d", trace[i]);
                puts("");
            }
}
