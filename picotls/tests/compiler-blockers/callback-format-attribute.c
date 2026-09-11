/* Next header failure after fixing chained ##: picotls.h:845. */
struct log_event {
    void (*cb)(const char *format, ...) __attribute__((format(printf, 1, 2)));
};
int main(void) { struct log_event event = {0}; return event.cb != 0; }
