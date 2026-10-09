#include "cpp_catch_all.cpp"
extern "C" int printf(const char *, ...);
extern "C" int puts(const char *);
static int selected, trace[128], count;
extern "C" void record_event(int value) { trace[count++] = value; }
extern "C" int should_fail(int value) { return value == selected; }
int main() {
    const int failures[] = {0, 1, 2, 3, 4, 6, 9};
    for (int mode = 0; mode != 15; ++mode)
        for (int failure : failures) {
            selected = failure; count = 0;
            try {
                int result = mode < 9 ? catch_all_case(mode) : mode < 11 ? catch_all_without_locals(mode-9) : mode < 13 ? catch_all_loop(mode-11) : mode == 13 ? catch_exit_boundary() : catch_handler_exit_failure();
                record_event(300 + result);
            }
            catch (int value) { record_event(400 + value); }
            catch (const CatchPayload &value) { record_event(500 + value.value); }
            printf("%d:%d", mode, failure);
            for (int i = 0; i != count; ++i) printf(" %d", trace[i]);
            puts("");
        }
}
