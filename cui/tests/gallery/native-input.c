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
    printf("PASS native gallery OS input: all27 sidebar selections, checkbox/switch/disabled/radio/slider/input/buttons/tabs/stepper/modal/extended/theme/accent; %d assertions\n", assertions);
    return 0;
}
