/* Test-only embedding of the original QuickJS engine. */
#include "quickjs.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdint.h>
#include <math.h>
#include <stddef.h>

int quickjs_abi_probe(JSContext *ctx);
int quickjs_atomics_threads(const char *script);
static int callbacks, modules, rejected;
static const char *module_root;
static char *read_file(const char *path, size_t *length) {
    FILE *f = fopen(path, "rb");
    if (!f) return NULL;
    if (fseek(f, 0, SEEK_END)) { fclose(f); return NULL; }
    long size = ftell(f);
    if (size < 0 || fseek(f, 0, SEEK_SET)) { fclose(f); return NULL; }
    char *text = malloc((size_t)size + 1);
    if (!text) { fclose(f); return NULL; }
    *length = fread(text, 1, size, f);
    fclose(f);
    if (*length != (size_t)size) { free(text); return NULL; }
    text[size] = 0;
    return text;
}
static int exception(JSContext *ctx) {
    JSValue e = JS_GetException(ctx);
    const char *s = JS_ToCString(ctx, e);
    fprintf(stderr, "EXCEPTION %s\n", s ? s : "<conversion failed>");
    JS_FreeCString(ctx, s);
    JSValue stack = JS_GetPropertyStr(ctx, e, "stack");
    s = JS_ToCString(ctx, stack);
    if (s) fprintf(stderr, "%s\n", s);
    JS_FreeCString(ctx, s);
    JS_FreeValue(ctx, stack);
    JS_FreeValue(ctx, e);
    return 1;
}
static JSValue host_double(JSContext *ctx, JSValueConst self, int argc, JSValueConst *argv) {
    int32_t n;
    (void)self;
    if (argc != 1) return JS_ThrowTypeError(ctx, "hostDouble requires one argument");
    if (JS_ToInt32(ctx, &n, argv[0])) return JS_EXCEPTION;
    callbacks++;
    return JS_NewInt32(ctx, n * 2);
}
static JSValue host_gc(JSContext *ctx, JSValueConst self, int argc, JSValueConst *argv) {
    (void)self; (void)argc; (void)argv;
    JS_RunGC(JS_GetRuntime(ctx));
    return JS_UNDEFINED;
}
static JSValue host_print(JSContext *ctx, JSValueConst self, int argc, JSValueConst *argv) {
    (void)self;
    for (int i = 0; i < argc; i++) {
        const char *s = JS_ToCString(ctx, argv[i]);
        if (!s) return JS_EXCEPTION;
        printf("%s%s", i ? " " : "", s);
        JS_FreeCString(ctx, s);
    }
    putchar('\n');
    return JS_UNDEFINED;
}
static JSModuleDef *loader(JSContext *ctx, const char *name, void *opaque) {
    (void)opaque;
    const char *sum = "export function sum(xs) { return xs.reduce((a,b) => a+b, 0); }";
    char *owned = NULL;
    size_t len;
    const char *text;
    if (!strcmp(name, "sum")) { text = sum; len = strlen(sum); modules++; }
    else if (module_root) {
        char path[4096];
        snprintf(path, sizeof(path), "%s/%s", module_root, name);
        owned = read_file(path, &len);
        if (!owned) { JS_ThrowReferenceError(ctx, "unknown module: %s", name); return NULL; }
        text = owned;
    } else { JS_ThrowReferenceError(ctx, "unknown module: %s", name); return NULL; }
    JSValue v = JS_Eval(ctx, text, len, name, JS_EVAL_TYPE_MODULE | JS_EVAL_FLAG_COMPILE_ONLY);
    free(owned);
    if (JS_IsException(v)) return NULL;
    JSModuleDef *m = JS_VALUE_GET_PTR(v);
    JS_FreeValue(ctx, v);
    return m;
}
static void rejection(JSContext *ctx, JSValueConst promise, JSValueConst reason, JS_BOOL handled, void *opaque) {
    (void)ctx; (void)promise; (void)reason; (void)opaque;
    rejected += handled ? -1 : 1;
}
static int drain(JSRuntime *rt) {
    JSContext *ctx = NULL;
    for (int i = 0; i < 10000; i++) {
        int r = JS_ExecutePendingJob(rt, &ctx);
        if (r < 0) return exception(ctx);
        if (!r) return rejected ? (fprintf(stderr, "unhandled rejection(s): %d\n", rejected), 1) : 0;
    }
    fprintf(stderr, "job drain budget exceeded\n"); return 1;
}
static int evaluate(JSContext *ctx, const char *text, const char *name, int flags) {
    JSValue v = JS_Eval(ctx, text, strlen(text), name, flags);
    int failed = JS_IsException(v);
    if (failed) exception(ctx);
    JS_FreeValue(ctx, v);
    return failed;
}
static int setup(JSContext *ctx) {
    JSValue global = JS_GetGlobalObject(ctx);
    JS_SetPropertyStr(ctx, global, "hostDouble", JS_NewCFunction(ctx, host_double, "hostDouble", 1));
    JS_SetPropertyStr(ctx, global, "print", JS_NewCFunction(ctx, host_print, "print", 1));
    JSValue std = JS_NewObject(ctx);
    JS_SetPropertyStr(ctx, std, "gc", JS_NewCFunction(ctx, host_gc, "gc", 0));
    JS_SetPropertyStr(ctx, global, "std", std);
    JSValue console = JS_NewObject(ctx);
    JS_SetPropertyStr(ctx, console, "log", JS_NewCFunction(ctx, host_print, "log", 1));
    JS_SetPropertyStr(ctx, global, "console", console);
    JS_FreeValue(ctx, global);
    return evaluate(ctx, "function __case(id, fn) { fn(); print('CASE PASS ' + id); }", "<harness>", JS_EVAL_TYPE_GLOBAL);
}
static int workflow(JSContext *ctx) {
    const char *script = "import {sum} from 'sum';"
        "const values = JSON.parse('{\"items\":[2,3,5]}').items.map(x=>hostDouble(x));"
        "globalThis.result = null; Promise.resolve().then(()=>globalThis.result = JSON.stringify({values,sum:sum(values)}));";
    if (evaluate(ctx, script, "workflow.mjs", JS_EVAL_TYPE_MODULE) || drain(JS_GetRuntime(ctx))) return 1;
    JSValue g = JS_GetGlobalObject(ctx), result = JS_GetPropertyStr(ctx, g, "result");
    const char *s = JS_ToCString(ctx, result);
    int failed = !s || strcmp(s, "{\"values\":[4,6,10],\"sum\":20}") || callbacks != 3 || modules != 1;
    printf("WORKFLOW %s callbacks=%d modules=%d\n", s ? s : "<missing>", callbacks, modules);
    JS_FreeCString(ctx, s); JS_FreeValue(ctx, result); JS_FreeValue(ctx, g);
    return failed;
}
int main(int argc, char **argv) {
    if (argc < 2) return 2;
    if (!strcmp(argv[1], "--atomics-threads") && argc == 3) {
        size_t length;
        char *script = read_file(argv[2], &length);
        if (!script) return 1;
        int failed = quickjs_atomics_threads(script);
        free(script);
        return failed;
    }
    int failed = 0;
    int repetitions = !strcmp(argv[1], "--workflow") ? 2 : 1;
    for (int i = 0; i < repetitions && !failed; i++) {
        callbacks = modules = rejected = 0;
        JSRuntime *rt = JS_NewRuntime();
        if (!rt) return 1;
        JSContext *ctx = JS_NewContext(rt);
        if (!ctx) { JS_FreeRuntime(rt); return 1; }
        JS_SetModuleLoaderFunc(rt, NULL, loader, NULL);
        JS_SetHostPromiseRejectionTracker(rt, rejection, NULL);
        if (argc >= 4 && !strcmp(argv[3], "--can-block")) JS_SetCanBlock(rt, 1);
        failed = setup(ctx);
        if (!failed && !strcmp(argv[1], "--workflow")) failed = workflow(ctx);
        else if (!failed && !strcmp(argv[1], "--fail")) failed = evaluate(ctx, "__case('deliberate', () => { throw Error('deliberate assertion failure'); });", "<failure-control>", JS_EVAL_TYPE_GLOBAL);
        else if (!failed && !strcmp(argv[1], "--abi")) failed = quickjs_abi_probe(ctx);
        else if (!failed && !strcmp(argv[1], "--file") && argc >= 3) {
            size_t len;
            char *text = read_file(argv[2], &len);
            if (!text) failed = 1;
            else {
                int is_module = argc >= 4 && !strcmp(argv[3], "--module");
                module_root = argc >= 5 ? argv[4] : NULL;
                const char *name = strrchr(argv[2], '/');
                failed = evaluate(ctx, text, name ? name + 1 : argv[2], is_module ? JS_EVAL_TYPE_MODULE : JS_EVAL_TYPE_GLOBAL);
                free(text);
                if (!failed) failed = drain(rt);
            }
        } else if (!failed) failed = 2;
        JS_RunGC(rt);
        JS_FreeContext(ctx);
        JS_FreeRuntime(rt);
    }
    return failed;
}
