/* Native differential consumer: stdout is an exact observation stream. */
#include <ghostty/vt.h>
#include <inttypes.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define CHECK(c) do { if (!(c)) { fprintf(stderr, "FAIL line %d: %s\n", __LINE__, #c); exit(1); } } while (0)
#define OK(c) CHECK((c) == GHOSTTY_SUCCESS)

static size_t cases, snapshots, cells_seen, stops, input_bytes;
static uint8_t input[16384];
static size_t input_len;

static void number(uint64_t value) {
    for (size_t i = 0; i < 8; ++i) { CHECK(putchar((int)(value & 255)) != EOF); value >>= 8; }
}
static void bytes(const void *data, size_t len) {
    number(len); CHECK(fwrite(data, 1, len, stdout) == len);
}
static void append(const void *data, size_t len) {
    CHECK(len <= sizeof(input) - input_len);
    memcpy(input + input_len, data, len); input_len += len;
}
static void literal(const char *text) { append(text, strlen(text)); }
static void ascii(size_t len) {
    for (size_t i = 0; i < len; ++i) {
        const uint8_t value = (uint8_t)('A' + i % 26); append(&value, 1);
    }
}

typedef struct { uint8_t replies[8192]; size_t reply_len, bells, titles; } Effects;
static void write_pty(GhosttyTerminal terminal, void *ctx, const uint8_t *data, size_t len) {
    (void)terminal; Effects *e = ctx;
    CHECK(len <= sizeof(e->replies) - e->reply_len);
    memcpy(e->replies + e->reply_len, data, len); e->reply_len += len;
}
static void bell(GhosttyTerminal terminal, void *ctx) { (void)terminal; ((Effects *)ctx)->bells++; }
static void title(GhosttyTerminal terminal, void *ctx) { (void)terminal; ((Effects *)ctx)->titles++; }

static void observe(GhosttyTerminal t, Effects *effects) {
    bool failed = true, visible = false;
    uint16_t x = 0, y = 0, cols = 0, rows = 0;
    OK(ghostty_terminal_get(t, GHOSTTY_TERMINAL_DATA_VT_PROCESSING_ERROR, &failed)); CHECK(!failed);
    OK(ghostty_terminal_get(t, GHOSTTY_TERMINAL_DATA_CURSOR_X, &x));
    OK(ghostty_terminal_get(t, GHOSTTY_TERMINAL_DATA_CURSOR_Y, &y));
    OK(ghostty_terminal_get(t, GHOSTTY_TERMINAL_DATA_CURSOR_VISIBLE, &visible));
    OK(ghostty_terminal_get(t, GHOSTTY_TERMINAL_DATA_COLS, &cols));
    OK(ghostty_terminal_get(t, GHOSTTY_TERMINAL_DATA_ROWS, &rows));
    number(x); number(y); number(visible); number(cols); number(rows);
    number(effects->bells); number(effects->titles); bytes(effects->replies, effects->reply_len);
    GhosttyString current_title = {0};
    OK(ghostty_terminal_get(t, GHOSTTY_TERMINAL_DATA_TITLE, &current_title));
    bytes(current_title.ptr, current_title.len);

    GhosttyFormatter f = NULL; uint8_t text[65536]; size_t written = 0; bool unwrap = true;
    OK(ghostty_formatter_terminal_new(NULL, &f, t));
    OK(ghostty_formatter_set(f, GHOSTTY_FORMATTER_OPT_UNWRAP, &unwrap));
    OK(ghostty_formatter_format_buf(f, text, sizeof(text), &written));
    bytes(text, written); ghostty_formatter_free(f);

    GhosttyRenderState state = NULL;
    GhosttyRenderStateRowIterator row = NULL;
    GhosttyRenderStateRowCells cell = NULL;
    OK(ghostty_render_state_new(NULL, &state));
    OK(ghostty_render_state_row_iterator_new(NULL, &row));
    OK(ghostty_render_state_row_cells_new(NULL, &cell));
    OK(ghostty_render_state_update(state, t));
    OK(ghostty_render_state_get(state, GHOSTTY_RENDER_STATE_DATA_ROW_ITERATOR, &row));
    for (uint16_t yi = 0; yi < rows; ++yi) {
        CHECK(ghostty_render_state_row_iterator_next(row));
        OK(ghostty_render_state_row_get(row, GHOSTTY_RENDER_STATE_ROW_DATA_CELLS, &cell));
        for (uint16_t xi = 0; xi < cols; ++xi) {
            GhosttyCell raw = 0; uint32_t cp = 0; GhosttyCellWide wide;
            GhosttyStyle style = {0}; uint32_t graphemes[8] = {0};
            GhosttyCodepointBuffer buffer = { .ptr = graphemes, .cap = 8 };
            OK(ghostty_render_state_row_cells_select(cell, xi));
            OK(ghostty_render_state_row_cells_get(cell, GHOSTTY_RENDER_STATE_ROW_CELLS_DATA_RAW, &raw));
            OK(ghostty_cell_get(raw, GHOSTTY_CELL_DATA_CODEPOINT, &cp));
            OK(ghostty_cell_get(raw, GHOSTTY_CELL_DATA_WIDE, &wide));
            OK(ghostty_render_state_row_cells_get(cell, GHOSTTY_RENDER_STATE_ROW_CELLS_DATA_STYLE, &style));
            OK(ghostty_render_state_row_cells_get(cell, GHOSTTY_RENDER_STATE_ROW_CELLS_DATA_GRAPHEMES_BUF, &buffer));
            number(cp); number((unsigned)wide); bytes(&style, sizeof(style));
            number(buffer.len);
            for (size_t gi = 0; gi < buffer.len; ++gi) number(graphemes[gi]);
            cells_seen++;
        }
    }
    ghostty_render_state_row_cells_free(cell);
    ghostty_render_state_row_iterator_free(row);
    ghostty_render_state_free(state); snapshots++;
}

static void payload(size_t len, unsigned pattern) {
    input_len = 0; ascii(len);
    switch (pattern) {
    case 0: break;
    case 1: literal("\tA\bB\rC\nD\a"); ascii(len); break;
    case 2:
        literal("\x1b[1;38;2;12;34;56m"); ascii(len);
        literal("\x1b[0m\x1b[2;3H\x1b[6n"); ascii(len); break;
    case 3: {
        static const uint8_t upper[] = {0x80, 0xff, 0xc2, 0x80, 0xe2, 0x82, 0xac, 0xe4, 0xb8, 0xad, 'e', 0xcc, 0x81};
        append(upper, sizeof(upper)); ascii(len);
        literal("\r\n\x1b[31m"); ascii(len); literal("\x1b[0m"); break;
    }
    case 4:
        literal("\x1b]2;"); ascii(len); literal("\a"); ascii(len);
        literal("\x1bP$qm\x1b\\"); ascii(len);
        literal("\x1b_APCpayload\x1b\\"); ascii(len); break;
    case 5:
        input_len = 0; /* Standalone long u32 narrow, wide, and grapheme runs. */
        for (size_t i = 0; i < len; ++i) literal("\xe2\x82\xac");
        literal("\r\n");
        for (size_t i = 0; i < len; ++i) literal("\xe4\xb8\xad");
        literal("\r\n");
        for (size_t i = 0; i < len; ++i) literal("e\xcc\x81");
        break;
    case 6:
        literal("\x1b(0");
        for (size_t i = 0; i < len; ++i) literal("lqxj");
        literal("\x1b(B"); ascii(len); break;
    default: CHECK(0);
    }
}

static void execute(size_t len, unsigned pattern, uint16_t cols, unsigned mode) {
    payload(len, pattern); number(len); number(pattern); number(cols); number(mode); bytes(input, input_len);
    GhosttyTerminal t = NULL; Effects effects = {0};
    OK(ghostty_terminal_new(NULL, &t, cols, 9));
    OK(ghostty_terminal_set(t, GHOSTTY_TERMINAL_OPT_USERDATA, &effects));
    OK(ghostty_terminal_set(t, GHOSTTY_TERMINAL_OPT_WRITE_PTY, (void *)write_pty));
    OK(ghostty_terminal_set(t, GHOSTTY_TERMINAL_OPT_BELL, (void *)bell));
    OK(ghostty_terminal_set(t, GHOSTTY_TERMINAL_OPT_TITLE_CHANGED, (void *)title));
    size_t off = 0, step = 0;
    if (mode == 0) {
        ghostty_terminal_vt_write(t, input, input_len); off = input_len;
    } else if (mode == 1) {
        static const size_t chunks[] = {63,64,65,127,128,129,255,256,257,1};
        while (off < input_len) {
            size_t n = chunks[step++ % (sizeof(chunks) / sizeof(chunks[0]))];
            if (n > input_len - off) n = input_len - off;
            ghostty_terminal_vt_write(t, input + off, n); off += n;
        }
    } else {
        const uint32_t strings = GHOSTTY_TERMINAL_BOUNDARY_APC_START | GHOSTTY_TERMINAL_BOUNDARY_APC_END |
            GHOSTTY_TERMINAL_BOUNDARY_DCS_START | GHOSTTY_TERMINAL_BOUNDARY_DCS_END |
            GHOSTTY_TERMINAL_BOUNDARY_OSC_START | GHOSTTY_TERMINAL_BOUNDARY_OSC_END;
        uint32_t requested = strings;
        do {
            GhosttyTerminalWriteStop stop = GHOSTTY_INIT_SIZED(GhosttyTerminalWriteStop);
            GhosttyResult result = ghostty_terminal_vt_write_until(t, input + off, input_len - off, requested, &stop);
            CHECK(result == GHOSTTY_SUCCESS || result == GHOSTTY_NO_VALUE);
            CHECK(stop.consumed <= input_len - off);
            CHECK(stop.consumed > 0 || input_len == 0);
            number(off); number((unsigned)result); number(requested); number(stop.consumed);
            number(stop.reached); number(stop.kind); number(stop.introducer); number(stop.end); number(stop.byte);
            off += stop.consumed; stops++; observe(t, &effects);
            requested = strings;
            if (mode == 3 && (stop.reached & (GHOSTTY_TERMINAL_BOUNDARY_APC_START |
                GHOSTTY_TERMINAL_BOUNDARY_DCS_START | GHOSTTY_TERMINAL_BOUNDARY_OSC_START)))
                requested |= GHOSTTY_TERMINAL_BOUNDARY_GROUND;
        } while (off < input_len);
    }
    CHECK(off == input_len); observe(t, &effects);
    input_bytes += input_len; cases++; ghostty_terminal_free(t);
}

int main(void) {
    static const size_t lengths[] = {0,1,63,64,65,127,128,129,255,256,257,511,512,513,1023};
    static const uint16_t columns[] = {17,63,64,65,127,128,129};
    for (size_t li = 0; li < sizeof(lengths) / sizeof(lengths[0]); ++li)
        for (size_t ci = 0; ci < sizeof(columns) / sizeof(columns[0]); ++ci)
            for (unsigned pattern = 0; pattern < 7; ++pattern)
                for (unsigned mode = 0; mode < 4; ++mode) execute(lengths[li], pattern, columns[ci], mode);
    CHECK(fflush(stdout) == 0);
    fprintf(stderr, "cases=%zu snapshots=%zu cells=%zu stops=%zu input_bytes=%zu\n", cases, snapshots, cells_seen, stops, input_bytes);
    return 0;
}
