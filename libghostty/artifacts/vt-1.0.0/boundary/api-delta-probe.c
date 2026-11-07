/* Assertion oracle for the pinned, unmodified libghostty-vt C API.
 * This is a test consumer, never a product implementation. */
#include <ghostty/vt.h>
#include <inttypes.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#ifdef DOTCC_GHOSTTY_INDIRECT_ABI
#include "dotcc_ghostty_abi.h"
#endif

#define CHECK(c) do { if (!(c)) { \
    fprintf(stderr, "FAIL %s:%d: %s\n", __FILE__, __LINE__, #c); exit(1); \
} } while (0)
#define OK(c) CHECK((c) == GHOSTTY_SUCCESS)

typedef struct { size_t live, allocated, released; bool fail; } Heap;
typedef struct { void *base; size_t len; uint8_t alignment; Heap *owner; } Header;

static void *heap_alloc(void *ctx, size_t len, uint8_t alignment, uintptr_t ra) {
    (void)ra;
    Heap *heap = ctx;
    if (heap->fail) return NULL;
    CHECK(alignment < sizeof(size_t) * 8 - 1);
    size_t align = (size_t)1 << alignment;
    if (align < _Alignof(Header)) align = _Alignof(Header);
    CHECK(len <= SIZE_MAX - align - sizeof(Header));
    void *base = malloc(len + align + sizeof(Header));
    if (!base) return NULL;
    uintptr_t address = ((uintptr_t)base + sizeof(Header) + align - 1) & ~(uintptr_t)(align - 1);
    Header *h = (Header *)address - 1;
    h->base = base; h->len = len; h->alignment = alignment; h->owner = heap;
    heap->live++; heap->allocated++;
    return (void *)address;
}

static bool heap_resize(void *ctx, void *mem, size_t len, uint8_t align, size_t next, uintptr_t ra) {
    (void)ctx; (void)mem; (void)len; (void)align; (void)next; (void)ra;
    return false; /* Ask Zig's allocator to allocate/copy/free. */
}

static void *heap_remap(void *ctx, void *mem, size_t len, uint8_t align, size_t next, uintptr_t ra) {
    (void)ctx; (void)mem; (void)len; (void)align; (void)next; (void)ra;
    return NULL;
}

static void heap_free(void *ctx, void *mem, size_t len, uint8_t align, uintptr_t ra) {
    (void)ra;
    Heap *heap = ctx;
    Header *h = (Header *)mem - 1;
    CHECK(h->owner == heap && h->len == len && h->alignment == align);
    CHECK(heap->live > 0);
    heap->live--; heap->released++;
    free(h->base);
}

static const GhosttyAllocatorVtable heap_vtable = {
    .alloc = heap_alloc, .resize = heap_resize, .remap = heap_remap, .free = heap_free
};
static Heap heap;
static const GhosttyAllocator allocator = { .ctx = &heap, .vtable = &heap_vtable };

int main(void) {
    uint8_t bytes[128]; size_t len = 0;
    OK(ghostty_color_scheme_report_encode(GHOSTTY_COLOR_SCHEME_DARK, bytes, sizeof(bytes), &len));
    CHECK(len == 9 && memcmp(bytes, "\x1b[?997;1n", len) == 0);
    puts("PASS color-scheme-report uint8-buffer=ESC[?997;1n");
    OK(ghostty_mode_report_encode(GHOSTTY_MODE_CURSOR_VISIBLE, GHOSTTY_MODE_REPORT_SET, bytes, sizeof(bytes), &len));
    CHECK(len == 9 && memcmp(bytes, "\x1b[?25;1$y", len) == 0);
    puts("PASS mode-report uint8-buffer=ESC[?25;1$y");
    GhosttySizeReportSize size = { .rows = 24, .columns = 80, .cell_width = 8, .cell_height = 16 };
    OK(ghostty_size_report_encode(GHOSTTY_SIZE_REPORT_CSI_18_T, &size, bytes, sizeof(bytes), &len));
    CHECK(len == 10 && memcmp(bytes, "\x1b[8;24;80t", len) == 0);
    CHECK(ghostty_size_report_encode(GHOSTTY_SIZE_REPORT_CSI_18_T, NULL, bytes, sizeof(bytes), &len) == GHOSTTY_INVALID_VALUE);
    puts("PASS size-report const-size-pointer=ESC[8;24;80t null=INVALID_VALUE");
    GhosttySgrParser sgr = NULL;
    OK(ghostty_sgr_new(&allocator, &sgr));
    uint16_t params[] = {1};
    OK(ghostty_sgr_set_params(sgr, params, NULL, 1));
    GhosttySgrAttribute attr;
    CHECK(ghostty_sgr_next(sgr, &attr));
    CHECK(ghostty_sgr_attribute_tag(&attr) == GHOSTTY_SGR_ATTR_BOLD);
    ghostty_sgr_free(sgr);
    puts("PASS sgr const-attribute-pointer=bold");
    GhosttyOscParser osc = NULL;
    OK(ghostty_osc_new(&allocator, &osc));
    const char input[] = "2;release title";
    for (size_t i = 0; i < sizeof(input) - 1; i++) ghostty_osc_next(osc, (uint8_t)input[i]);
    GhosttyOscCommand command = ghostty_osc_end(osc, 0x07);
    GhosttyString title = {0};
    OK(ghostty_osc_command_data(command, GHOSTTY_OSC_DATA_CHANGE_WINDOW_TITLE_STR, &title));
    CHECK(title.len == 13 && memcmp(title.ptr, "release title", 13) == 0);
    CHECK(ghostty_osc_command_data(command, GHOSTTY_OSC_DATA_UNKNOWN_CONTENT, &title) == GHOSTTY_NO_VALUE);
    CHECK(ghostty_osc_command_data(command, (GhosttyOscCommandData)9999, &title) == GHOSTTY_UNSUPPORTED);
    ghostty_osc_free(osc);
    puts("PASS osc data-result=SUCCESS,NO_VALUE,UNSUPPORTED title=GhosttyString");
    GhosttyTerminal terminal = NULL;
    OK(ghostty_terminal_new(&allocator, &terminal, 10, 3));
    GhosttyTerminalScrollViewport scroll = { .tag = GHOSTTY_TERMINAL_SCROLL_VIEWPORT_BOTTOM };
    OK(ghostty_terminal_scroll_viewport(terminal, scroll));
    puts("PASS viewport return=SUCCESS renamed-bottom-tag");
    GhosttyFormatter formatter = NULL;
    OK(ghostty_formatter_terminal_new(&allocator, &formatter, terminal));
    bool unwrap = true;
    OK(ghostty_formatter_set(formatter, GHOSTTY_FORMATTER_OPT_UNWRAP, &unwrap));
    const char wrapped[] = "abcdefghijklmn";
    ghostty_terminal_vt_write(terminal, (const uint8_t *)wrapped, sizeof(wrapped) - 1);
    OK(ghostty_formatter_format_buf(formatter, bytes, sizeof(bytes), &len));
    CHECK(len == sizeof(wrapped) - 1 && memcmp(bytes, wrapped, len) == 0);
    ghostty_formatter_free(formatter);
    ghostty_terminal_free(terminal);
    CHECK(heap.live == 0 && heap.allocated == heap.released);
    puts("PASS formatter constructor=3-args option-set=unwrapped allocator=balanced");
    puts("RESULT api-delta-cases=7 passed=7 failed=0");
    return 0;
}
