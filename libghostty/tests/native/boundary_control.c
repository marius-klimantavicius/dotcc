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
int dotcc_ghostty_mode(void *terminal, uint16_t number, int ansi, int *enabled);
int dotcc_ghostty_cell(void *terminal, uint16_t x, uint16_t y, uint32_t *codepoint,
                     int *flags, int *underline, int *foreground_palette, uint32_t *foreground_rgb);
int dotcc_ghostty_snapshot(void *terminal, uint8_t *bytes, size_t capacity, size_t *written);
int dotcc_ghostty_out_of_space(void);
size_t dotcc_ghostty_pointer_size(void);
size_t dotcc_ghostty_style_size(void);

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
    CHECK(status == (expected[0] ? GHOSTTY_OUT_OF_SPACE : GHOSTTY_SUCCESS));
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

static void style_reserved_bits(void) {
    GhosttyStyle style;
    ghostty_style_default(&style);
    CHECK(ghostty_style_is_default(&style));
    CHECK(!ghostty_style_is_default(NULL));
    const uint16_t defined = GHOSTTY_STYLE_FLAG_BOLD | GHOSTTY_STYLE_FLAG_ITALIC
        | GHOSTTY_STYLE_FLAG_FAINT | GHOSTTY_STYLE_FLAG_BLINK | GHOSTTY_STYLE_FLAG_INVERSE
        | GHOSTTY_STYLE_FLAG_INVISIBLE | GHOSTTY_STYLE_FLAG_STRIKETHROUGH
        | GHOSTTY_STYLE_FLAG_OVERLINE | GHOSTTY_STYLE_UNDERLINE_MASK;
    /* Exhaust every flag combination, including reserved bits; reserved storage
       must never turn an otherwise default style into a nondefault style. */
    for (uint32_t flags = 0; flags <= UINT16_MAX; flags++) {
        style.flags = (GhosttyStyleFlags)flags;
        style.reserved = UINT16_MAX;
        CHECK(ghostty_style_is_default(&style) == ((flags & defined) == 0));
    }
}

int main(void) {
    style_reserved_bits();
    CHECK(dotcc_ghostty_pointer_size() == sizeof(void *));
    CHECK(dotcc_ghostty_style_size() == sizeof(GhosttyStyle));
    CHECK(dotcc_ghostty_out_of_space() == GHOSTTY_OUT_OF_SPACE);
    printf("PASS boundary-abi pointer=%zu style=%zu allocator=%zu vtable=%zu\n",
           sizeof(void *), sizeof(GhosttyStyle),
           sizeof(GhosttyAllocator), sizeof(GhosttyAllocatorVtable));
    for (int n = 0; n < 2; n++) {
        void *t = NULL, *other = NULL;
        OK(dotcc_ghostty_new(&t, 10, 4)); CHECK(t != NULL);
        OK(dotcc_ghostty_new(&other, 10, 4)); CHECK(other != NULL && other != t);
        snapshot(t, "");
        feed(t, "hello\r\n\xe4\xb8\xad");
        snapshot(t, "hello\n\xe4\xb8\xad");
        uint16_t cols, rows, x, y;
        OK(dotcc_ghostty_position(t, &cols, &rows, &x, &y));
        CHECK(cols == 10 && rows == 4 && x == 2 && y == 1);
        int enabled;
        feed(t, "\x1b[?25l"); OK(dotcc_ghostty_mode(t, 25, 0, &enabled)); CHECK(enabled == 0);
        feed(t, "\x1b[?25h"); OK(dotcc_ghostty_mode(t, 25, 0, &enabled)); CHECK(enabled == 1);
        uint32_t cp, rgb; int flags, underline, palette;
        OK(dotcc_ghostty_cell(t, 0, 1, &cp, &flags, &underline, &palette, &rgb));
        CHECK(cp == 0x4e2d && flags == 0 && palette == -1 && rgb == UINT32_MAX);
        feed(t, "\x1b[1;31mR\x1b[0m");
        OK(dotcc_ghostty_cell(t, 2, 1, &cp, &flags, &underline, &palette, &rgb));
        CHECK(cp == 'R' && flags == 1 && palette == 1 && rgb == UINT32_MAX);
        /* Keep the subsequent snapshot expectations unchanged. */
        feed(t, "\x1b[2;3H\x1b[K");
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
        snapshot(t, "");
        feed(t, "\x1b[1;2;3;4:3;5;7;8;9;53;38;2;12;34;56mS\x1b[0mN");
        OK(dotcc_ghostty_cell(t, 0, 0, &cp, &flags, &underline, &palette, &rgb));
        CHECK(cp == 'S' && flags == 255 && underline == GHOSTTY_SGR_UNDERLINE_CURLY
              && palette == -1 && rgb == 0x0c2238);
        OK(dotcc_ghostty_cell(t, 1, 0, &cp, &flags, &underline, &palette, &rgb));
        CHECK(cp == 'N' && flags == 0 && underline == GHOSTTY_SGR_UNDERLINE_NONE
              && palette == -1 && rgb == UINT32_MAX);
        dotcc_ghostty_reset(t);
        feed(t, "abcdefghijklmn"); snapshot(t, "abcdefghijklmn");
        dotcc_ghostty_reset(t);
        feed(t, "done"); snapshot(t, "done");
        dotcc_ghostty_free(t); snapshot(other, "second"); dotcc_ghostty_free(other);
    }
    void *invalid = NULL;
    CHECK(dotcc_ghostty_new(&invalid, 0, 4) == GHOSTTY_INVALID_VALUE);
    dotcc_ghostty_free(NULL);
    puts("PASS boundary-workflow cycles=2 utf8=wide snapshot=empty,sized,unwrapped style=all-flags,curly,rgb alternate=restored reset=preserves-resize independent=1");
    puts("RESULT boundary-cases=2 passed=2 failed=0");
    return 0;
}
