/* Compiles the actual staged browser algorithms and app selection/cleanup.
 * Core/window/GL services below are controlled test doubles, not rendering tests. */
#include <stddef.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "platform/cui_platform.h"
#ifdef PLATFORM_ISOLATED
#include "platform-files-selected.c"
#else
#include "cui_files.c"
#endif

static int test_windows;
int cui_host_is_windows(void) { return test_windows; }
#include "emoji-fixture.c"
#define CHECK(x) do { if (!(x)) return __LINE__; } while (0)

int platform_algorithms(void) {
    char path[1024];
    test_windows = 0;
    cui__path_join(path, sizeof path, "folder", "žą😀.txt");
    CHECK(!strcmp(path, "folder/žą😀.txt"));
    test_windows = 1;
    cui__path_join(path, sizeof path, "folder", "žą😀.txt");
    CHECK(!strcmp(path, "folder\\žą😀.txt"));
    cui__path_join(path, sizeof path, "C:/folder", "file");
    CHECK(!strcmp(path, "C:/folder/file"));
    CHECK(cui__path_up(path) && !strcmp(path, "C:/folder"));
    CHECK(cui__path_up(path) && !strcmp(path, "C:/"));
    CHECK(!cui__path_up(path));
    CHECK(cui__glob("*.PNG;*.jpg", "žą😀.png"));
    CHECK(!cui__glob("*.PNG;*.jpg", "žą😀.txt"));
    cui_file_entry entries[3] = {{"file10", false, 100, 20}, {"File2", false, 10, 30}, {"z-dir", true, 0, 0}};
    cui__fb_state state; memset(&state, 0, sizeof state);
    state.ent = entries; cui__fb_sorting = &state;
    int a = 0, b = 1, d = 2;
    CHECK(cui__fb_cmp(&a, &b) > 0);
    CHECK(cui__fb_cmp(&d, &a) < 0);
    state.desc = true;
    CHECK(cui__fb_cmp(&a, &b) < 0);
    CHECK(cui__fb_cmp(&d, &a) < 0);
    state.desc = false; state.sort = 1;
    CHECK(cui__fb_cmp(&a, &b) < 0);
    state.sort = 2;
    CHECK(cui__fb_cmp(&a, &b) > 0);
    cui__fb_sorting = NULL;
    return 0;
}

int platform_places(int windows, char *home, char *root, char *desktop) {
    char paths[5][CUI__FB_PATH]; const char *labels[5]; int n;
    test_windows = windows;
    cui__fb_default_places(paths, labels, &n);
    strcpy(root, paths[n - 1]);
    strcpy(home, n == 5 ? paths[0] : "");
    strcpy(desktop, n == 5 ? paths[1] : "");
    return n;
}

/* Keep the actual allocator used by the staged app and count owned blocks. */
static int app_blocks, contexts, inits, shutdowns, gls, software_calls;
static int init_ok = 1, gl_context_ok = 1, gl_renderer_ok = 1;
static void *test_calloc(size_t n, size_t size) { void *p = calloc(n, size); if (p) app_blocks++; return p; }
static void test_free(void *p) { if (p) app_blocks--; free(p); }
#define calloc test_calloc
#define free test_free
#ifdef PLATFORM_ISOLATED
#include "platform-app-selected.c"
#else
#include "platform/cui_app.c"
#endif
#undef calloc
#undef free

cui_ctx *cui_create(const cui_ctx_desc *d) { (void)d; contexts++; return calloc(1, sizeof(cui_ctx)); }
void cui_destroy(cui_ctx *ui) { contexts--; free(ui); }
void cui_set_platform(cui_ctx *ui, const cui_platform_iface *p) { ui->platform = *p; }
void cui_set_renderer(cui_ctx *ui, const cui_renderer_iface *p) { (void)ui; (void)p; }
void cui_end_frame(cui_ctx *ui) { (void)ui; }
bool cui__plat_init(cui_app *app) { (void)app; inits++; return init_ok; }
void cui__plat_shutdown(cui_app *app) { (void)app; shutdowns++; }
bool cui__plat_gl_create(cui_app *app, bool vsync) { (void)app; (void)vsync; return gl_context_ok; }
void *cui__plat_gl_proc(const char *name) { (void)name; return NULL; }
const char *cui__plat_name(void) { return "glfw"; }
int cui__plat_system_dark(void) { return -1; }
void cui__plat_show(cui_app *app) { (void)app; }
double cui__plat_now(void) { return 1; }
void cui__plat_gl_release(cui_app *app) { (void)app; }
cui_gl_renderer *cui_gl_create(cui_gl_loader proc) { (void)proc; if (!gl_renderer_ok) return NULL; gls++; return (cui_gl_renderer *)1; }
void cui_gl_destroy(cui_gl_renderer *gl) { (void)gl; gls--; }
bool cui_gl_is_software(const cui_gl_renderer *gl) { (void)gl; return true; }
cui_renderer_iface cui_gl_iface(cui_gl_renderer *gl) { (void)gl; cui_renderer_iface r = {0}; return r; }
cui_sw_renderer *cui_sw_create(void) { software_calls++; return NULL; }
void cui_sw_destroy(cui_sw_renderer *sw) { (void)sw; }
void cui_sw_set_incremental(cui_sw_renderer *sw, bool enabled) { (void)sw; (void)enabled; }
cui_renderer_iface cui_sw_iface(cui_sw_renderer *sw) { (void)sw; cui_renderer_iface r = {0}; return r; }

int platform_app_case(int kind) {
    app_blocks = contexts = inits = shutdowns = gls = software_calls = 0;
    init_ok = kind != 2; gl_context_ok = kind != 3; gl_renderer_ok = kind != 4;
    cui_app_desc d; memset(&d, 0, sizeof d);
    d.hidden = true;
    d.renderer = kind == 0 ? CUI_RENDERER_SOFTWARE : kind == 4 ? CUI_RENDERER_OPENGL : CUI_RENDERER_AUTO;
    cui_app *app = cui_app_create(&d);
    if (kind <= 4) CHECK(app == NULL);
    else { CHECK(app != NULL && !cui_app_is_software(app)); cui_app_destroy(app); }
    CHECK(app_blocks == 0 && contexts == 0 && gls == 0 && software_calls == 0);
    CHECK(cui_app_ui(NULL) == NULL);
    CHECK(inits == (kind < 2 ? 0 : 1));
    CHECK(shutdowns == (kind >= 3 ? 1 : 0));
    return 0;
}
#include "glfw-prefix-fixture.c"

/* These wrappers exercise the exact production override signatures. */
int platform_list(const char *path, cui_file_entry *entries, int cap, char *names, size_t ncap, bool hidden) {
    return cui_fs_list(NULL, path, entries, cap, names, ncap, hidden);
}
bool platform_dialog(const cui_file_dialog_opts *opts, char *out, size_t cap) { return cui__plat_file_dialog(NULL, opts, out, cap); }
bool platform_url(const char *url) { return cui__plat_open_url(url); }

#ifdef PLATFORM_TEST_MAIN
int main(void) {
    int result = platform_algorithms();
    if (result) { fprintf(stderr, "browser algorithms failed at %d\n", result); return 1; }
    for (int k = 0; k <= 5; k++) {
        if (k == 1) setenv("CUI_RENDERER", "software", 1); else unsetenv("CUI_RENDERER");
        result = platform_app_case(k);
        if (result) { fprintf(stderr, "app case %d failed at %d\n", k, result); return 1; }
    }
    CHECK(platform_emoji(0, -1) == 0 && platform_emoji_count() == 9 && platform_emoji_tried());
    CHECK(platform_emoji(1, 0) == 1 && platform_emoji_count() == 1);
    CHECK(platform_emoji_null() == 0);
    CHECK(platform_glfw_allocation_case(0) == 0);
    CHECK(platform_glfw_allocation_case(1) == 0);
    printf("{\"entry\":[%zu,%zu,%zu,%zu,%zu],\"dialog\":[%zu,%zu,%zu,%zu,%zu,%zu]}\n",
        sizeof(cui_file_entry), offsetof(cui_file_entry,name), offsetof(cui_file_entry,dir), offsetof(cui_file_entry,size), offsetof(cui_file_entry,modified),
        sizeof(cui_file_dialog_opts), offsetof(cui_file_dialog_opts,kind), offsetof(cui_file_dialog_opts,title), offsetof(cui_file_dialog_opts,filter_name), offsetof(cui_file_dialog_opts,filter), offsetof(cui_file_dialog_opts,default_name));
    return 0;
}
#endif
