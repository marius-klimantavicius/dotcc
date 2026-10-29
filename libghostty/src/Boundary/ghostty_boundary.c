/* Authored ABI adapter. This file is translated together with the upstream
 * engine. It contains no terminal parsing, state, layout, or formatting logic. */
#define _POSIX_C_SOURCE 200112L
#include <stdlib.h>
#include <ghostty/vt/terminal.h>
#include <ghostty/vt/formatter.h>
#include <ghostty/vt/grid_ref.h>

/* Adapter observation flags, not an upstream packed-cell layout. */
enum DotccGhosttyTextStyle {
    DOTCC_STYLE_BOLD = 1, DOTCC_STYLE_ITALIC = 2,
    DOTCC_STYLE_FAINT = 4, DOTCC_STYLE_BLINK = 8,
    DOTCC_STYLE_INVERSE = 16, DOTCC_STYLE_INVISIBLE = 32,
    DOTCC_STYLE_STRIKETHROUGH = 64, DOTCC_STYLE_OVERLINE = 128
};

/* Freestanding upstream builds deliberately have no default allocator. These
 * callbacks provide only memory services through the public extension point.
 * posix_memalign/free map to the shared DotCC allocator in managed delivery. */
static void *boundary_alloc(void *ctx, size_t len, uint8_t alignment, uintptr_t ra) {
    (void)ctx; (void)ra;
    if (alignment >= sizeof(size_t) * 8) return NULL;
    size_t bytes = (size_t)1 << alignment;
    if (bytes < sizeof(void *)) bytes = sizeof(void *);
    void *result = NULL;
    if (posix_memalign(&result, bytes, len) != 0) return NULL;
    return result;
}

static bool boundary_resize(void *ctx, void *memory, size_t len,
                            uint8_t alignment, size_t next, uintptr_t ra) {
    (void)ctx; (void)memory; (void)len; (void)alignment; (void)next; (void)ra;
    return false; /* Let the upstream allocator perform relocation and copying. */
}

static void *boundary_remap(void *ctx, void *memory, size_t len,
                            uint8_t alignment, size_t next, uintptr_t ra) {
    (void)ctx; (void)memory; (void)len; (void)alignment; (void)next; (void)ra;
    return NULL; /* This allocator does not support remapping. */
}

static void boundary_free(void *ctx, void *memory, size_t len,
                           uint8_t alignment, uintptr_t ra) {
    (void)ctx; (void)len; (void)alignment; (void)ra;
    free(memory);
}

static int allocator_context;
static const GhosttyAllocatorVtable boundary_vtable = {
    .alloc = boundary_alloc, .resize = boundary_resize,
    .remap = boundary_remap, .free = boundary_free
};
static const GhosttyAllocator boundary_allocator = {
    .ctx = &allocator_context, .vtable = &boundary_vtable
};

int dotcc_ghostty_new(void **out, uint16_t columns, uint16_t rows) {
    return ghostty_terminal_new(&boundary_allocator, (GhosttyTerminal *)out, columns, rows);
}

void dotcc_ghostty_free(void *terminal) {
    ghostty_terminal_free((GhosttyTerminal)terminal);
}

void dotcc_ghostty_write(void *terminal, const uint8_t *bytes, size_t count) {
    ghostty_terminal_vt_write((GhosttyTerminal)terminal, bytes, count);
}

void dotcc_ghostty_reset(void *terminal) {
    ghostty_terminal_reset((GhosttyTerminal)terminal);
}

int dotcc_ghostty_resize(void *terminal, uint16_t columns, uint16_t rows) {
    return ghostty_terminal_resize((GhosttyTerminal)terminal, columns, rows, 0, 0);
}

int dotcc_ghostty_position(void *terminal, uint16_t *columns, uint16_t *rows,
                          uint16_t *x, uint16_t *y) {
    GhosttyResult result;
    result = ghostty_terminal_get(terminal, GHOSTTY_TERMINAL_DATA_COLS, columns);
    if (result != GHOSTTY_SUCCESS) return result;
    result = ghostty_terminal_get(terminal, GHOSTTY_TERMINAL_DATA_ROWS, rows);
    if (result != GHOSTTY_SUCCESS) return result;
    result = ghostty_terminal_get(terminal, GHOSTTY_TERMINAL_DATA_CURSOR_X, x);
    if (result != GHOSTTY_SUCCESS) return result;
    return ghostty_terminal_get(terminal, GHOSTTY_TERMINAL_DATA_CURSOR_Y, y);
}

int dotcc_ghostty_screen(void *terminal, int *screen) {
    GhosttyTerminalScreen value;
    GhosttyResult result = ghostty_terminal_get(
        terminal, GHOSTTY_TERMINAL_DATA_ACTIVE_SCREEN, &value);
    if (result == GHOSTTY_SUCCESS) *screen = (int)value;
    return result;
}

int dotcc_ghostty_cell(void *terminal, uint16_t x, uint16_t y,
                       uint32_t *codepoint, int *flags, int *underline,
                       int *foreground_palette, uint32_t *foreground_rgb) {
    GhosttyPoint point = {0};
    point.tag = GHOSTTY_POINT_TAG_ACTIVE;
    point.value.coordinate.x = x;
    point.value.coordinate.y = y;
    GhosttyGridRef ref = {0};
    ref.size = sizeof(ref);
    GhosttyResult result = ghostty_terminal_grid_ref(terminal, point, &ref);
    if (result != GHOSTTY_SUCCESS) return result;
    GhosttyCell cell;
    result = ghostty_grid_ref_cell(&ref, &cell);
    if (result != GHOSTTY_SUCCESS) return result;
    result = ghostty_cell_get(cell, GHOSTTY_CELL_DATA_CODEPOINT, codepoint);
    if (result != GHOSTTY_SUCCESS) return result;
    GhosttyStyle style = {0};
    style.size = sizeof(style);
    result = ghostty_grid_ref_style(&ref, &style);
    if (result != GHOSTTY_SUCCESS) return result;
    *flags = (style.bold ? DOTCC_STYLE_BOLD : 0)
           | (style.italic ? DOTCC_STYLE_ITALIC : 0)
           | (style.faint ? DOTCC_STYLE_FAINT : 0)
           | (style.blink ? DOTCC_STYLE_BLINK : 0)
           | (style.inverse ? DOTCC_STYLE_INVERSE : 0)
           | (style.invisible ? DOTCC_STYLE_INVISIBLE : 0)
           | (style.strikethrough ? DOTCC_STYLE_STRIKETHROUGH : 0)
           | (style.overline ? DOTCC_STYLE_OVERLINE : 0);
    *underline = style.underline;
    *foreground_palette = style.fg_color.tag == GHOSTTY_STYLE_COLOR_PALETTE
                        ? style.fg_color.value.palette : -1;
    *foreground_rgb = style.fg_color.tag == GHOSTTY_STYLE_COLOR_RGB
        ? ((uint32_t)style.fg_color.value.rgb.r << 16)
        | ((uint32_t)style.fg_color.value.rgb.g << 8)
        | style.fg_color.value.rgb.b
        : UINT32_MAX;
    return GHOSTTY_SUCCESS;
}

int dotcc_ghostty_snapshot(void *terminal, uint8_t *bytes, size_t capacity,
                         size_t *written) {
    GhosttyFormatter formatter = NULL;
    GhosttyFormatterTerminalOptions options = {0};
    options.size = sizeof(options);
    options.emit = GHOSTTY_FORMATTER_FORMAT_PLAIN;
    options.unwrap = true;
    options.trim = true;
    options.extra.size = sizeof(options.extra);
    options.extra.screen.size = sizeof(options.extra.screen);
    GhosttyResult result = ghostty_formatter_terminal_new(
        &boundary_allocator, &formatter, terminal, options);
    if (result != GHOSTTY_SUCCESS) return result;
    result = ghostty_formatter_format_buf(formatter, bytes, capacity, written);
    ghostty_formatter_free(formatter);
    return result;
}

/* Values originate in the pinned upstream headers rather than managed mirrors. */
int dotcc_ghostty_out_of_space(void) { return GHOSTTY_OUT_OF_SPACE; }
size_t dotcc_ghostty_pointer_size(void) { return sizeof(void *); }
size_t dotcc_ghostty_formatter_options_size(void) {
    return sizeof(GhosttyFormatterTerminalOptions);
}
