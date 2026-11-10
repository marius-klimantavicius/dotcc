/* Independent native OS-input control for the same upstream gallery. */
#include <cui/cui_app.h>
#include "cui_internal.h"
#include "gallery_ui.h"
#define GLFW_EXPOSE_NATIVE_X11
#include <GLFW/glfw3.h>
#include <GLFW/glfw3native.h>
#include <math.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/wait.h>
#include <unistd.h>

void cui__app_render(cui_app *app);
static cui_app *app;
static cui_ctx *ui;
static gallery g;
static char window_id[32];
static int assertions;

static void require(int condition, const char *message) {
    assertions++;
    if (!condition) {
        fprintf(stderr, "FAIL native gallery input: %s; page=%d\n", message, g.page);
        if (app) cui_app_destroy(app);
        exit(11);
    }
}

static void frames(int count) {
    for (int i = 0; i < count; i++) {
        require(cui_app_frame(app), "gallery stays open");
        gallery_frame(ui, &g);
        cui__app_render(app);
        usleep(5000);
    }
}

static void command(const char *const *args) {
    pid_t child = fork();
    require(child >= 0, "xdotool fork");
    if (!child) { execvp(args[0], (char *const *)args); _exit(127); }
    int status = 0;
    require(waitpid(child, &status, 0) == child && WIFEXITED(status) && WEXITSTATUS(status) == 0, "xdotool exit");
}

static cui_rect rect(const char *id) {
    cui_id value = cui_make_id(ui, id);
    for (int i = 0; i < ui->node_count; i++) if (ui->nodes[i].id == value) return ui->nodes[i].rect;
    require(0, id);
    return (cui_rect){0};
}

static float scroll_y(const char *id) {
    cui_id value = cui_make_id(ui, id);
    for (int i = 0; i < ui->node_count; i++) if (ui->nodes[i].id == value) return ui->nodes[i].scroll_y;
    require(0, id);
    return 0;
}

static int blocker(void) {
    for (int i = 0; i < ui->node_count; i++) if (ui->nodes[i].flags & CUI_NF_BLOCKER) return 1;
    return 0;
}

static cui_rect text(const char *label, int sidebar) {
    for (int i = 0; i < ui->node_count; i++) {
        cui_node *n = &ui->nodes[i];
        if (!n->text || (n->rect.x < 240) != sidebar) continue;
        if ((int)strlen(label) == n->text_len && !memcmp(label, n->text, n->text_len)) return n->rect;
    }
    require(0, label);
    return (cui_rect){0};
}

static void move(cui_rect r, float x, float y) {
    char px[32], py[32];
    snprintf(px, sizeof px, "%d", (int)lroundf(r.x + r.w * x));
    snprintf(py, sizeof py, "%d", (int)lroundf(r.y + r.h * y));
    command((const char *const[]){"xdotool", "mousemove", "--window", window_id, px, py, NULL});
    frames(2);
}

static void click(cui_rect r, float x, float y) {
    require(r.w > 0 && r.h > 0 && r.y >= 0 && r.y + r.h <= ui->vh, "click rectangle visible");
    move(r, x, y);
    command((const char *const[]){"xdotool", "mousedown", "1", NULL}); frames(2);
    command((const char *const[]){"xdotool", "mouseup", "1", NULL}); frames(3);
}

static void key(const char *name) {
    command((const char *const[]){"xdotool", "keydown", name, NULL}); frames(2);
    command((const char *const[]){"xdotool", "keyup", name, NULL}); frames(3);
}

static void wheel(int sidebar, int down, int count) {
    move(rect(sidebar ? "nav" : "main"), .7f, .5f);
    for (int i = 0; i < count; i++) {
        command((const char *const[]){"xdotool", "click", down ? "5" : "4", NULL}); frames(2);
    }
    frames(12);
}

static void select_page(int page) {
    for (int i = 0; i < 30; i++) {
        cui_rect item = text(gallery_page_name(page), 1), nav = rect("nav");
        if (item.y >= nav.y + 2 && item.y + item.h <= nav.y + nav.h - 2) {
            click(item, .5f, .5f);
            require(g.page == page, "OS sidebar changes upstream page");
            for (int settle = 0; settle < 120 && scroll_y("main") >= 1; settle++) frames(1);
            require(scroll_y("main") < 1, "sidebar navigation scrolls main to top");
            printf("PASS native OS page %d: %s\n", page, gallery_page_name(page));
            return;
        }
        wheel(1, item.y + item.h > nav.y + nav.h - 2, 1);
    }
    require(0, "reveal sidebar page");
}

static void reveal(const char *label) {
    for (int i = 0; i < 30; i++) {
        cui_rect r = text(label, 0);
        if (r.y >= 12 && r.y + r.h < ui->vh - 20) return;
        wheel(0, r.y + r.h >= ui->vh - 20, 1);
    }
    require(0, label);
}

/* Read-only prefixes of private renderer payloads in pinned cui_textedit.c
 * and cui_emoji.c. memcpy avoids aliasing native objects as unrelated types.
 * The managed test uses the corresponding emitted structs directly. */
typedef struct { int start, count, kind; } observed_section;
typedef struct {
    float cell, size, header, pad;
    int cols, nsec;
    observed_section sec[11];
} observed_geometry;
typedef struct {
    void *emoji, *state;
    observed_geometry g;
    int *items;
    char (*recent)[32];
    int hover, cur;
    bool focus, soft;
    float top;
} observed_grid;

static cui_id smart_id(const char *name) {
    cui_push_id(ui, "Smart inputs");
    cui_id id = cui_make_id(ui, name);
    cui_pop_id(ui);
    return id;
}

static cui_node *node(cui_id id) {
    for (int i = 0; i < ui->node_count; i++) if (ui->nodes[i].id == id) return &ui->nodes[i];
    require(0, "original scoped node exists");
    return NULL;
}

static void emoji_message(const char *expected) {
    cui_node *n = node(smart_id("message") ^ 0xED17u);
    const char *buf = NULL;
    int len = 0;
    require(n->paint && n->paint_data, "message edit renderer exists");
    memcpy(&buf, n->paint_data, sizeof buf);
    memcpy(&len, (char *)n->paint_data + sizeof buf, sizeof len);
    require(buf && len >= 0 && len < 160, "message edit payload is valid");
    printf("EMOJI_MESSAGE\t%d\t", len);
    for (int i = 0; i < len; i++) printf("%02X", (unsigned char)buf[i]);
    printf("\t%.*s\n", len, buf);
    require(len == (int)strlen(expected) && !memcmp(buf, expected, len), "emoji message contains exact UTF-8 bytes");
}

static cui_node *emoji_grid(void) {
    cui_node *found = NULL;
    for (int i = 0; i < ui->node_count; i++) {
        cui_node *n = &ui->nodes[i];
        if (!n->paint || !n->paint_data || !n->id || !(n->flags & CUI_NF_FOCUSABLE) || n->rect.h != 260) continue;
        require(!found, "exactly one original emoji grid");
        found = n;
    }
    require(found != NULL, "original 260px emoji grid is rendered");
    return found;
}

static observed_grid emoji_paint(void) {
    observed_grid p;
    memcpy(&p, emoji_grid()->paint_data, sizeof p);
    require(p.emoji && p.state && p.items && p.recent && p.g.cell > 0 && p.g.cols >= 4 &&
        p.g.nsec > 0 && p.g.nsec <= 11, "emoji font and original picker paint data available");
    return p;
}

static void emoji_popup(int open) {
    cui_id id = smart_id("emoji-pop");
    int found = 0;
    for (int i = 0; i < ui->popup_count; i++) {
        cui_popup *p = &ui->popups[i];
        if (p->id == id) {
            found = 1;
            printf("EMOJI_POPUP\t%u\t%d\t%g\n", id, p->open, p->s.x);
            require(p->open == (bool)open, "original scoped emoji popup open state");
        }
        else require(!p->open, "no other popup is open");
    }
    require(found, "original scoped emoji popup exists");
}

static void open_emoji(void) {
    cui_rect message = node(smart_id("message"))->rect, trigger = {0};
    int count = 0;
    for (int i = 0; i < ui->node_count; i++) {
        cui_node *n = &ui->nodes[i];
        if (n->rect.x > message.x + message.w && fabsf(n->rect.y - message.y) < 10 &&
            (n->flags & CUI_NF_INTERACTIVE)) { trigger = n->rect; count++; }
    }
    require(count == 1, "original smile icon trigger beside message");
    click(trigger, .5f, .5f);
    frames(100);
    emoji_popup(1);
    emoji_grid();
    puts("PASS native OS smile icon opens original emoji popup");
}

static void emoji_recent(int count, const char *first, const char *second) {
    observed_grid p = emoji_paint();
    require(p.g.sec[0].kind == (count ? 0 : 1), "recent section precedes smileys when populated");
    if (count) {
        require(p.g.sec[0].count == count, "recent list deduplicates selected emoji");
        require(p.items[0] == -1 && !strcmp(p.recent[0], first), "recent first emoji persists");
        if (count > 1) require(p.items[1] == -2 && !strcmp(p.recent[1], second), "recent second emoji persists");
    }
    printf("EMOJI_RECENT\t%s%s%s\n", first ? first : "", second ? " " : "", second ? second : "");
}

static unsigned char *emoji_pixels(const char *name, cui_rect r, int color, size_t *length) {
    int width = 1240, height = 820;
    unsigned char *frame = malloc((size_t)width * height * 4);
    require(frame && cui_app_screenshot(app, frame, width, height), "emoji GL readback");
    int x = (int)ceilf(r.x), y = (int)ceilf(r.y), w = (int)floorf(r.w) - 1, h = (int)floorf(r.h) - 1;
    require(x >= 0 && y >= 0 && w > 0 && h > 0 && x + w <= width && y + h <= height, "emoji GL crop visible");
    *length = (size_t)w * h * 4;
    unsigned char *pixels = malloc(*length);
    require(pixels != NULL, "emoji GL crop allocation");
    int colored = 0, ink = 0;
    for (int row = 0; row < h; row++) {
        memcpy(pixels + (size_t)row * w * 4, frame + ((size_t)(y + row) * width + x) * 4, (size_t)w * 4);
        for (int col = 0; col < w; col++) {
            unsigned char *v = pixels + ((size_t)row * w + col) * 4;
            int high = v[0] > v[1] ? v[0] : v[1], low = v[0] < v[1] ? v[0] : v[1];
            if (v[2] > high) high = v[2];
            if (v[2] < low) low = v[2];
            if (high - low > 32) colored++;
            if (row > 3 && row < h - 3 && col > 8 && col < w - 8 && high < 128) ink++;
        }
    }
    free(frame);
    const char *directory = getenv("CUI_GALLERY_INPUT_CAPTURE");
    if (directory && *directory) {
        char path[4096];
        snprintf(path, sizeof path, "%s/%s.rgba", directory, name);
        FILE *file = fopen(path, "wb");
        require(file != NULL, "open emoji GL capture");
        require(fwrite(pixels, 1, *length, file) == *length, "write emoji GL capture");
        fclose(file);
    }
    printf("EMOJI_PIXELS\t%s\t%d\t%d\t%d\n", name, w, h, colored);
    if (color) require(colored >= 50, "picked emoji renders real colored GL pixels");
    else require(ink >= 50, "original message GL crop contains rendered text");
    return pixels;
}

static void pick_emoji(int selection, int section, int column, const char *expected) {
    cui_rect grid = emoji_grid()->rect;
    observed_grid p = emoji_paint();
    require(column >= 0 && column < p.g.cols && section < p.g.nsec && p.g.sec[section].count > column,
        "selected original emoji cell exists");
    float top = 0;
    for (int i = 0; i < section; i++) top += (p.g.sec[i].kind == 10 ? 0 : p.g.header) +
        ((p.g.sec[i].count + p.g.cols - 1) / p.g.cols) * p.g.cell;
    cui_rect cell = { grid.x + p.g.pad + column * p.g.cell, grid.y + top + p.g.header - p.top, p.g.cell, p.g.cell };
    char name[80]; size_t length;
    snprintf(name, sizeof name, "emoji-%d-cell", selection);
    free(emoji_pixels(name, cell, 1, &length));
    int popup_count = ui->popup_count;
    cui_id grid_id = emoji_grid()->id;
    click(cell, .5f, .5f);
    frames(10);
    emoji_message(expected); /* append checked independently before close */
    emoji_popup(0);
    require(ui->popup_count == popup_count, "picking creates no phantom popup in nested ID scope");
    frames(100);
    emoji_popup(0);
    for (int i = 0; i < ui->popup_count; i++)
        if (ui->popups[i].id == smart_id("emoji-pop")) require(ui->popups[i].s.x == 0, "original emoji popup exit spring finishes");
    for (int i = 0; i < ui->node_count; i++) require(ui->nodes[i].id != grid_id, "closed original emoji grid disappears from rendered nodes");
    emoji_message(expected);
    snprintf(name, sizeof name, "emoji-%d-message", selection);
    free(emoji_pixels(name, node(smart_id("message"))->rect, 0, &length));
    printf("PASS native OS emoji selection %d appends exact UTF-8 and closes original popup\n", selection);
}

static void emoji_workflow(void) {
    const char *first = "\xf0\x9f\x98\x80", *second = "\xf0\x9f\x98\x83";
    select_page(10); reveal("Emoji picker"); wheel(0, 1, 3);
    emoji_message("Release notes are up ");
    size_t before_len, after_len;
    unsigned char *before = emoji_pixels("emoji-0-message", node(smart_id("message"))->rect, 0, &before_len);
    open_emoji(); emoji_recent(0, NULL, NULL);
    pick_emoji(1, 0, 0, "Release notes are up \xf0\x9f\x98\x80");
    unsigned char *after = emoji_pixels("emoji-1-rendered-message", node(smart_id("message"))->rect, 0, &after_len);
    /* Native ordinary text fonts do not use the separate color emoji renderer. */
    printf("EMOJI_RENDER_CHANGE\t%d\n", before_len != after_len || memcmp(before, after, before_len) != 0);
    free(before); free(after);
    open_emoji(); emoji_recent(1, first, NULL);
    pick_emoji(2, 1, 1, "Release notes are up \xf0\x9f\x98\x80\xf0\x9f\x98\x83");
    select_page(1); select_page(10); reveal("Emoji picker"); wheel(0, 1, 3);
    emoji_message("Release notes are up \xf0\x9f\x98\x80\xf0\x9f\x98\x83");
    open_emoji(); emoji_recent(2, second, first);
    pick_emoji(3, 0, 1, "Release notes are up \xf0\x9f\x98\x80\xf0\x9f\x98\x83\xf0\x9f\x98\x80");
    open_emoji(); emoji_recent(2, first, second);
    key("Escape"); frames(100); emoji_popup(0);
    emoji_message("Release notes are up \xf0\x9f\x98\x80\xf0\x9f\x98\x83\xf0\x9f\x98\x80");
    puts("PASS native emoji message and recent history persist across popup reopen and page navigation");
}

int native_input(void) {
    app = cui_app_create(&(cui_app_desc){.title="Native CUI gallery input", .width=1240,
        .height=820, .continuous=true, .renderer=CUI_RENDERER_OPENGL,
        .theme=CUI_THEME_LIGHT, .accent=CUI_ACCENT_NEUTRAL});
    require(app != NULL, "create native app");
    ui = cui_app_ui(app);
    gallery_init(&g);
    frames(4);
    snprintf(window_id, sizeof window_id, "%lu", glfwGetX11Window(glfwGetCurrentContext()));
    command((const char *const[]){"xdotool", "windowraise", window_id, "windowfocus", "--sync", window_id, NULL});
    select_page(1);
    for (int i = 0; i < 27; i++) select_page(i);
    select_page(3);
    click(text("Email notifications", 0), .5f, .5f); require(!g.newsletter, "checkbox state");
    click(g.bluetooth_rect, .5f, .5f); require(g.bluetooth, "switch state");
    click(text("Disabled option", 0), .5f, .5f); require(!g.terms, "disabled state");
    click(text("Only when I save", 0), .5f, .5f); require(g.plan == 2, "radio state");
    wheel(0, 1, 5);
    reveal("Volume");
    click(rect("Volume"), .85f, .8f); require(g.volume > 75 && g.volume <= 100, "slider state");
    select_page(2);
    click(rect("name"), .5f, .5f);
    command((const char *const[]){"xdotool", "keydown", "Control_L", NULL}); frames(2);
    key("a"); command((const char *const[]){"xdotool", "keyup", "Control_L", NULL}); frames(2);
    command((const char *const[]){"xdotool", "type", "--clearmodifiers", "Gallery test", NULL}); frames(4);
    if (strcmp(g.name, "Gallery test")) fprintf(stderr, "Native input name='%s'; focus=%u, name-id=%u, rect=%.0f,%.0f,%.0f,%.0f\n", g.name, ui->focus, cui_make_id(ui, "name"), rect("name").x, rect("name").y, rect("name").w, rect("name").h);
    require(!strcmp(g.name, "Gallery test"), "input buffer");
    select_page(1);
    reveal("Save changes");
    click(text("Save changes", 0), .5f, .5f); require(g.loading, "button loading");
    click(text("Reset", 0), .5f, .5f); require(!g.loading, "reset loading");
    select_page(4);
    click(text("Analytics", 0), .5f, .5f); require(g.tab == 1, "tab state");
    click(text("Next", 0), .5f, .5f); require(g.step == 2, "step forward");
    click(text("Back", 0), .5f, .5f); require(g.step == 1, "step back");
    select_page(5);
    click(text("Edit profile", 0), .5f, .5f); require(g.dialog_open, "modal opened");
    key("Escape"); require(!g.dialog_open, "modal closed");
    for (int settle = 0; settle < 120 && (ui->modal_layer || ui->prev_modal_layer || blocker()); settle++) frames(1);
    require(!ui->modal_layer && !ui->prev_modal_layer && !blocker(), "modal input layer and exit scrim released");
    emoji_workflow();
    select_page(9);
    click(text("Save", 0), .5f, .5f); require(text("Saved", 0).w > 0, "extended saved result");
    select_page(1);
    click(text("Dark", 1), .5f, .5f); require(g.dark && cui_get_theme_mode(ui) == CUI_THEME_DARK, "theme state");
    for (int settle = 0; settle < 120 && ui->theme_t < 1; settle++) frames(1);
    require(ui->theme_t == 1, "dark palette settled");
    click(rect("accent-3"), .5f, .5f); require(g.accent == 3 && cui_get_accent(ui) == CUI_ACCENT_GREEN, "accent state");
    for (int settle = 0; settle < 120 && ui->theme_t < 1; settle++) frames(1);
    require(ui->theme_t == 1, "green palette settled");
    cui_app_quit(app);
    require(!cui_app_frame(app), "quit ends loop");
    cui_app_destroy(app);
    app = NULL;
    printf("PASS native gallery OS input: all27 sidebar selections, checkbox/switch/disabled/radio/slider/input/buttons/tabs/stepper/modal/emoji/extended/theme/accent; %d assertions\n", assertions);
    return 0;
}
