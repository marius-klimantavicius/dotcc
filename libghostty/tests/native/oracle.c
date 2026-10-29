/* Assertion oracle for the pinned, unmodified libghostty-vt C API.
 * This is a test consumer, never a product implementation. */
#include <ghostty/vt.h>
#include <inttypes.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

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
static size_t cases;

static void passed(const char *name, const char *observation) {
    printf("PASS %s %s\n", name, observation);
    cases++;
}

static GhosttyTerminal terminal_new(uint16_t cols, uint16_t rows) {
    GhosttyTerminal t = NULL;
    OK(ghostty_terminal_new(&allocator, &t, cols, rows));
    CHECK(t != NULL);
    return t;
}

static void feed(GhosttyTerminal t, const char *s) {
    ghostty_terminal_vt_write(t, (const uint8_t *)s, strlen(s));
    bool failed = true;
    OK(ghostty_terminal_get(t, GHOSTTY_TERMINAL_DATA_VT_PROCESSING_ERROR, &failed));
    CHECK(!failed);
}

static uint16_t query16(GhosttyTerminal t, GhosttyTerminalData key) {
    uint16_t v = UINT16_MAX;
    OK(ghostty_terminal_get(t, key, &v));
    return v;
}

static void cursor(GhosttyTerminal t, uint16_t x, uint16_t y) {
    CHECK(query16(t, GHOSTTY_TERMINAL_DATA_CURSOR_X) == x);
    CHECK(query16(t, GHOSTTY_TERMINAL_DATA_CURSOR_Y) == y);
}

static GhosttyFormatter formatter_new(GhosttyTerminal t) {
    GhosttyFormatter f = NULL;
    GhosttyFormatterTerminalOptions opts = GHOSTTY_INIT_SIZED(GhosttyFormatterTerminalOptions);
    opts.emit = GHOSTTY_FORMATTER_FORMAT_PLAIN;
    opts.unwrap = true; opts.trim = true;
    OK(ghostty_formatter_terminal_new(&allocator, &f, t, opts));
    return f;
}

static void text(GhosttyTerminal t, const char *expected) {
    uint8_t bytes[4096]; size_t written = SIZE_MAX;
    GhosttyFormatter f = formatter_new(t);
    OK(ghostty_formatter_format_buf(f, bytes, sizeof(bytes), &written));
    if (written != strlen(expected) || memcmp(bytes, expected, written) != 0) {
        fprintf(stderr, "text mismatch expected=[%s], actual=[%.*s] len=%zu\n",
                expected, (int)written, bytes, written);
        exit(1);
    }
    ghostty_formatter_free(f);
}

typedef struct {
    GhosttyRenderState state;
    GhosttyRenderStateRowIterator rows;
    GhosttyRenderStateRowCells cells;
} View;

static View view_new(void) {
    View v = {0};
    OK(ghostty_render_state_new(&allocator, &v.state));
    OK(ghostty_render_state_row_iterator_new(&allocator, &v.rows));
    OK(ghostty_render_state_row_cells_new(&allocator, &v.cells));
    return v;
}

static void view_free(View *v) {
    ghostty_render_state_row_cells_free(v->cells);
    ghostty_render_state_row_iterator_free(v->rows);
    ghostty_render_state_free(v->state);
    memset(v, 0, sizeof(*v));
}

static void cell_select(View *v, uint16_t x, uint16_t y) {
    OK(ghostty_render_state_get(v->state, GHOSTTY_RENDER_STATE_DATA_ROW_ITERATOR, &v->rows));
    for (uint16_t n = 0; n <= y; n++) CHECK(ghostty_render_state_row_iterator_next(v->rows));
    OK(ghostty_render_state_row_get(v->rows, GHOSTTY_RENDER_STATE_ROW_DATA_CELLS, &v->cells));
    OK(ghostty_render_state_row_cells_select(v->cells, x));
}

static uint32_t codepoint(View *v, uint16_t x, uint16_t y) {
    GhosttyCell raw = 0; uint32_t cp = UINT32_MAX;
    cell_select(v, x, y);
    OK(ghostty_render_state_row_cells_get(v->cells, GHOSTTY_RENDER_STATE_ROW_CELLS_DATA_RAW, &raw));
    OK(ghostty_cell_get(raw, GHOSTTY_CELL_DATA_CODEPOINT, &cp));
    return cp;
}

typedef struct { char bytes[128]; size_t len, bells, titles; GhosttyTerminal owner; } Effects;

static void write_pty(GhosttyTerminal t, void *ctx, const uint8_t *bytes, size_t len) {
    Effects *e = ctx; CHECK(t == e->owner && len <= sizeof(e->bytes) - e->len);
    memcpy(e->bytes + e->len, bytes, len); e->len += len;
}
static void bell(GhosttyTerminal t, void *ctx) {
    Effects *e = ctx; CHECK(t == e->owner); e->bells++;
}
static void title_changed(GhosttyTerminal t, void *ctx) {
    Effects *e = ctx; CHECK(t == e->owner); e->titles++;
}

int main(void) {
    GhosttyTerminal t = terminal_new(10, 4);
    View v = view_new();
    CHECK(query16(t, GHOSTTY_TERMINAL_DATA_COLS) == 10);
    CHECK(query16(t, GHOSTTY_TERMINAL_DATA_ROWS) == 4);
    cursor(t, 0, 0); text(t, "");
    passed("creation", "grid=10x4 cursor=0,0 text=empty");

    feed(t, "hello\r\nworld");
    cursor(t, 5, 1); text(t, "hello\nworld");
    OK(ghostty_render_state_update(v.state, t));
    uint16_t cols = 0, rows = 0;
    OK(ghostty_render_state_get(v.state, GHOSTTY_RENDER_STATE_DATA_COLS, &cols));
    OK(ghostty_render_state_get(v.state, GHOSTTY_RENDER_STATE_DATA_ROWS, &rows));
    CHECK(cols == 10 && rows == 4);
    CHECK(codepoint(&v, 0, 0) == 'h' && codepoint(&v, 4, 1) == 'd');
    GhosttyRenderStateCursor rc = GHOSTTY_INIT_SIZED(GhosttyRenderStateCursor);
    OK(ghostty_render_state_get(v.state, GHOSTTY_RENDER_STATE_DATA_CURSOR, &rc));
    CHECK(rc.viewport_has_value && rc.viewport_x == 5 && rc.viewport_y == 1 && rc.visible);
    passed("ascii-render", "text=hello\\nworld cursor=5,1 cells=h,d");

    feed(t, "\x1b[2;4HZ"); cursor(t, 4, 1);
    OK(ghostty_render_state_update(v.state, t));
    CHECK(codepoint(&v, 3, 1) == 'Z');
    passed("cursor-cup", "cell(3,1)=U+005A cursor=4,1");

    feed(t, "\x1b[2J\x1b[H"); cursor(t, 0, 0); text(t, "");
    passed("erase-display", "text=empty cursor=0,0");

    feed(t, "\x1b[1;31mR\x1b[0mN");
    OK(ghostty_render_state_update(v.state, t));
    cell_select(&v, 0, 0);
    GhosttyStyle style = GHOSTTY_INIT_SIZED(GhosttyStyle);
    OK(ghostty_render_state_row_cells_get(v.cells, GHOSTTY_RENDER_STATE_ROW_CELLS_DATA_STYLE, &style));
    CHECK(style.bold && style.fg_color.tag == GHOSTTY_STYLE_COLOR_PALETTE && style.fg_color.value.palette == 1);
    cell_select(&v, 1, 0);
    style.size = sizeof(style);
    OK(ghostty_render_state_row_cells_get(v.cells, GHOSTTY_RENDER_STATE_ROW_CELLS_DATA_STYLE, &style));
    CHECK(!style.bold && style.fg_color.tag == GHOSTTY_STYLE_COLOR_NONE);
    passed("sgr", "R=bold,palette1 N=default");

    ghostty_terminal_reset(t);
    feed(t, "\xe2"); feed(t, "\x82"); feed(t, "\xac\xe4\xb8\xad" "e\xcc\x81");
    cursor(t, 4, 0); text(t, "\xe2\x82\xac\xe4\xb8\xad" "e\xcc\x81");
    OK(ghostty_render_state_update(v.state, t));
    CHECK(codepoint(&v, 0, 0) == 0x20ac && codepoint(&v, 1, 0) == 0x4e2d);
    cell_select(&v, 1, 0); GhosttyCell raw; GhosttyCellWide wide;
    OK(ghostty_render_state_row_cells_get(v.cells, GHOSTTY_RENDER_STATE_ROW_CELLS_DATA_RAW, &raw));
    OK(ghostty_cell_get(raw, GHOSTTY_CELL_DATA_WIDE, &wide)); CHECK(wide == GHOSTTY_CELL_WIDE_WIDE);
    cell_select(&v, 2, 0);
    OK(ghostty_render_state_row_cells_get(v.cells, GHOSTTY_RENDER_STATE_ROW_CELLS_DATA_RAW, &raw));
    OK(ghostty_cell_get(raw, GHOSTTY_CELL_DATA_WIDE, &wide)); CHECK(wide == GHOSTTY_CELL_WIDE_SPACER_TAIL);
    cell_select(&v, 3, 0); uint32_t count = 0, graphemes[8] = {0};
    OK(ghostty_render_state_row_cells_get(v.cells, GHOSTTY_RENDER_STATE_ROW_CELLS_DATA_GRAPHEMES_LEN, &count));
    CHECK(count == 2);
    OK(ghostty_render_state_row_cells_get(v.cells, GHOSTTY_RENDER_STATE_ROW_CELLS_DATA_GRAPHEMES_BUF, graphemes));
    CHECK(graphemes[0] == 'e' && graphemes[1] == 0x301);
    passed("utf8-graphemes", "U+20AC U+4E2D[wide,tail] U+0065+0301 cursor=4,0");

    feed(t, "\x1b[?25l"); bool visible = true;
    OK(ghostty_terminal_get(t, GHOSTTY_TERMINAL_DATA_CURSOR_VISIBLE, &visible)); CHECK(!visible);
    feed(t, "\x1b[?25h");
    OK(ghostty_terminal_get(t, GHOSTTY_TERMINAL_DATA_CURSOR_VISIBLE, &visible)); CHECK(visible);
    passed("cursor-visibility", "hidden=0 restored=1");

    ghostty_terminal_reset(t); feed(t, "PRIMARY"); feed(t, "\x1b[?1049h\x1b[HALT");
    GhosttyTerminalScreen screen;
    OK(ghostty_terminal_get(t, GHOSTTY_TERMINAL_DATA_ACTIVE_SCREEN, &screen));
    CHECK(screen == GHOSTTY_TERMINAL_SCREEN_ALTERNATE); text(t, "ALT");
    feed(t, "\x1b[?1049l");
    OK(ghostty_terminal_get(t, GHOSTTY_TERMINAL_DATA_ACTIVE_SCREEN, &screen));
    CHECK(screen == GHOSTTY_TERMINAL_SCREEN_PRIMARY); text(t, "PRIMARY"); cursor(t, 7, 0);
    passed("alternate-screen", "alternate=ALT primary=PRIMARY cursor=7,0");

    ghostty_terminal_reset(t); feed(t, "ABCDEFGHIJKL"); cursor(t, 2, 1);
    OK(ghostty_terminal_resize(t, 6, 5, 8, 16));
    CHECK(query16(t, GHOSTTY_TERMINAL_DATA_COLS) == 6 && query16(t, GHOSTTY_TERMINAL_DATA_ROWS) == 5);
    text(t, "ABCDEFGHIJKL");
    uint32_t px = 0;
    OK(ghostty_terminal_get(t, GHOSTTY_TERMINAL_DATA_WIDTH_PX, &px)); CHECK(px == 48);
    OK(ghostty_terminal_get(t, GHOSTTY_TERMINAL_DATA_HEIGHT_PX, &px)); CHECK(px == 80);
    OK(ghostty_render_state_update(v.state, t)); CHECK(codepoint(&v, 0, 1) == 'G');
    passed("resize-reflow", "grid=6x5 text=ABCDEFGHIJKL pixels=48x80 row1=GHIJKL");

    ghostty_terminal_reset(t); cursor(t, 0, 0); text(t, "");
    CHECK(query16(t, GHOSTTY_TERMINAL_DATA_COLS) == 6 && query16(t, GHOSTTY_TERMINAL_DATA_ROWS) == 5);
    passed("reset", "grid=6x5 cursor=0,0 text=empty");

    Effects effects = { .owner = t };
    OK(ghostty_terminal_set(t, GHOSTTY_TERMINAL_OPT_USERDATA, &effects));
    OK(ghostty_terminal_set(t, GHOSTTY_TERMINAL_OPT_WRITE_PTY, (void *)write_pty));
    OK(ghostty_terminal_set(t, GHOSTTY_TERMINAL_OPT_BELL, (void *)bell));
    OK(ghostty_terminal_set(t, GHOSTTY_TERMINAL_OPT_TITLE_CHANGED, (void *)title_changed));
    feed(t, "\x1b[2;3H\x1b[6n\a\x1b]2;DotCC oracle\a");
    CHECK(effects.len == 6 && memcmp(effects.bytes, "\x1b[2;3R", 6) == 0);
    CHECK(effects.bells == 1 && effects.titles == 1);
    GhosttyString title = {0};
    OK(ghostty_terminal_get(t, GHOSTTY_TERMINAL_DATA_TITLE, &title));
    CHECK(title.len == 12 && memcmp(title.ptr, "DotCC oracle", 12) == 0);
    passed("callbacks-title", "reply=ESC[2;3R bell=1 title=DotCC_oracle notifications=1");

    GhosttyFormatter f = formatter_new(t); size_t required = 0;
    feed(t, "X");
    CHECK(ghostty_formatter_format_buf(f, NULL, 0, &required) == GHOSTTY_OUT_OF_SPACE && required > 0);
    uint8_t one[1]; size_t written = 0;
    CHECK(ghostty_formatter_format_buf(f, one, 0, &written) == GHOSTTY_OUT_OF_SPACE && written == required);
    uint8_t *allocated = NULL; size_t len = 0;
    OK(ghostty_formatter_format_alloc(f, &allocator, &allocated, &len));
    CHECK(len == required && allocated != NULL); ghostty_free(&allocator, allocated, len);
    ghostty_formatter_free(f);
    passed("formatter-buffer", "sizing=OUT_OF_SPACE short=OUT_OF_SPACE allocated=freed");

    CHECK(ghostty_terminal_resize(t, 0, 5, 0, 0) == GHOSTTY_INVALID_VALUE);
    CHECK(ghostty_terminal_get(t, GHOSTTY_TERMINAL_DATA_INVALID, &cols) == GHOSTTY_INVALID_VALUE);
    CHECK(ghostty_terminal_get(NULL, GHOSTTY_TERMINAL_DATA_COLS, &cols) == GHOSTTY_INVALID_VALUE);
    GhosttyTerminal invalid = NULL;
    CHECK(ghostty_terminal_new(&allocator, &invalid, 0, 4) == GHOSTTY_INVALID_VALUE);
    ghostty_terminal_reset(NULL); ghostty_terminal_free(NULL);
    passed("ordinary-errors", "zero-dimension=NULL-query=invalid null-free/reset=accepted");

    heap.fail = true;
    CHECK(ghostty_terminal_new(&allocator, &invalid, 10, 4) == GHOSTTY_OUT_OF_MEMORY);
    heap.fail = false;
    passed("allocator-failure", "new=OUT_OF_MEMORY");

    GhosttyTerminal other = terminal_new(6, 5);
    feed(other, "SECOND");
    ghostty_terminal_reset(t); feed(t, "FIRST");
    text(t, "FIRST"); text(other, "SECOND");
    ghostty_terminal_free(other);
    text(t, "FIRST");
    passed("instance-independence", "first=FIRST second=SECOND first-survives-second-free");

    view_free(&v); ghostty_terminal_free(t);
    for (int n = 0; n < 32; n++) {
        GhosttyTerminal cycle = terminal_new(8, 3); feed(cycle, "cycle");
        text(cycle, "cycle"); ghostty_terminal_free(cycle);
    }
    CHECK(heap.live == 0 && heap.allocated == heap.released);
    passed("lifecycle", "cycles=32 allocator-pairs=balanced live=0");
    printf("RESULT cases=%zu passed=%zu failed=0\n", cases, cases);
    return 0;
}
