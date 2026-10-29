/* Ordinary C consumer of the authored, algorithm-free delivery ABI adapter. */
#include <ghostty/vt.h>
#include <stddef.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

int dotcc_ghostty_new(void **out, uint16_t columns, uint16_t rows);
void dotcc_ghostty_free(void *terminal);
void dotcc_ghostty_write(void *terminal, const uint8_t *bytes, size_t count);
void dotcc_ghostty_reset(void *terminal);
int dotcc_ghostty_resize(void *terminal, uint16_t columns, uint16_t rows);
int dotcc_ghostty_position(void *terminal, uint16_t *columns, uint16_t *rows, uint16_t *x, uint16_t *y);
int dotcc_ghostty_screen(void *terminal, int *screen);
int dotcc_ghostty_snapshot(void *terminal, uint8_t *bytes, size_t capacity, size_t *written);
int dotcc_ghostty_out_of_space(void);
size_t dotcc_ghostty_pointer_size(void);
size_t dotcc_ghostty_formatter_options_size(void);

#define CHECK(c) do { if (!(c)) { \
    fprintf(stderr, "FAIL boundary:%d: %s\n", __LINE__, #c); exit(1); \
} } while (0)
#define OK(c) CHECK((c) == GHOSTTY_SUCCESS)

static void feed(void *t, const char *s) {
    dotcc_ghostty_write(t, (const uint8_t *)s, strlen(s));
}

static void snapshot(void *t, const char *expected) {
    size_t size = SIZE_MAX, written = SIZE_MAX;
    int status = dotcc_ghostty_snapshot(t, NULL, 0, &size);
    CHECK(status == GHOSTTY_OUT_OF_SPACE);
    CHECK(size == strlen(expected));
    uint8_t *bytes = malloc(size ? size : 1);
    CHECK(bytes != NULL);
    if (size) {
        CHECK(dotcc_ghostty_snapshot(t, bytes, size - 1, &written) == GHOSTTY_OUT_OF_SPACE);
        CHECK(written == size);
    }
    OK(dotcc_ghostty_snapshot(t, bytes, size, &written));
    CHECK(written == size && memcmp(bytes, expected, size) == 0);
    free(bytes);
}

int main(void) {
    CHECK(dotcc_ghostty_pointer_size() == sizeof(void *));
    CHECK(dotcc_ghostty_formatter_options_size() == sizeof(GhosttyFormatterTerminalOptions));
    CHECK(dotcc_ghostty_out_of_space() == GHOSTTY_OUT_OF_SPACE);
    printf("PASS boundary-abi pointer=%zu formatter-options=%zu allocator=%zu vtable=%zu\n",
           sizeof(void *), sizeof(GhosttyFormatterTerminalOptions),
           sizeof(GhosttyAllocator), sizeof(GhosttyAllocatorVtable));
    for (int n = 0; n < 2; n++) {
        void *t = NULL, *other = NULL;
        OK(dotcc_ghostty_new(&t, 10, 4)); CHECK(t != NULL);
        OK(dotcc_ghostty_new(&other, 10, 4)); CHECK(other != NULL && other != t);
        feed(t, "hello\r\n\xe4\xb8\xad");
        snapshot(t, "hello\n\xe4\xb8\xad");
        uint16_t cols, rows, x, y;
        OK(dotcc_ghostty_position(t, &cols, &rows, &x, &y));
        CHECK(cols == 10 && rows == 4 && x == 2 && y == 1);
        feed(other, "second"); snapshot(other, "second");
        feed(t, "\x1b[?1049h\x1b[HALT");
        int screen = -1; OK(dotcc_ghostty_screen(t, &screen));
        CHECK(screen == GHOSTTY_TERMINAL_SCREEN_ALTERNATE); snapshot(t, "ALT");
        feed(t, "\x1b[?1049l"); OK(dotcc_ghostty_screen(t, &screen));
        CHECK(screen == GHOSTTY_TERMINAL_SCREEN_PRIMARY); snapshot(t, "hello\n\xe4\xb8\xad");
        OK(dotcc_ghostty_resize(t, 12, 5));
        dotcc_ghostty_reset(t);
        OK(dotcc_ghostty_position(t, &cols, &rows, &x, &y));
        CHECK(cols == 12 && rows == 5 && x == 0 && y == 0);
        feed(t, "done"); snapshot(t, "done");
        dotcc_ghostty_free(t); snapshot(other, "second"); dotcc_ghostty_free(other);
    }
    void *invalid = NULL;
    CHECK(dotcc_ghostty_new(&invalid, 0, 4) == GHOSTTY_INVALID_VALUE);
    dotcc_ghostty_free(NULL);
    puts("PASS boundary-workflow cycles=2 utf8=wide snapshot=sized alternate=restored reset=preserves-resize independent=1");
    puts("RESULT boundary-cases=2 passed=2 failed=0");
    return 0;
}
