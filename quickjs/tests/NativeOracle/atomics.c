/* Test-only cross-runtime backing ownership and actual upstream waiter list. */
#include "quickjs.h"
#include <pthread.h>
#include <stdatomic.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>

static _Atomic int finalized_buffers;
static void shared_released(JSRuntime *rt, void *opaque, void *pointer) {
    (void)rt; (void)opaque; (void)pointer;
    atomic_fetch_add(&finalized_buffers, 1);
}
static int result_equals(JSContext *ctx, const char *script, const char *expected) {
    JSValue value = JS_Eval(ctx, script, strlen(script), "atomics-thread.js", JS_EVAL_TYPE_GLOBAL);
    int failed = JS_IsException(value);
    if (failed) {
        JSValue error = JS_GetException(ctx);
        const char *text = JS_ToCString(ctx, error);
        fprintf(stderr, "atomics: %s\n", text ? text : "exception");
        JS_FreeCString(ctx, text); JS_FreeValue(ctx, error);
    } else {
        const char *text = JS_ToCString(ctx, value);
        failed = !text || strcmp(text, expected);
        if (failed) fprintf(stderr, "atomics expected %s got %s\n", expected, text ? text : "null");
        JS_FreeCString(ctx, text);
    }
    JS_FreeValue(ctx, value);
    return failed;
}
static int install(JSContext *ctx, uint8_t *backing, int big) {
    JSValue buffer = JS_NewArrayBuffer(ctx, backing, 16, shared_released, NULL, 1);
    if (JS_IsException(buffer)) return 1;
    JSValue global = JS_GetGlobalObject(ctx);
    int failed = JS_SetPropertyStr(ctx, global, "shared", buffer) < 0;
    failed |= JS_SetPropertyStr(ctx, global, "waitBigInt", JS_NewBool(ctx, big)) < 0;
    JS_FreeValue(ctx, global);
    JS_SetCanBlock(JS_GetRuntime(ctx), 1);
    return failed;
}
typedef struct Worker { uint8_t *backing; int big, failed; const char *script; } Worker;
static void *work(void *opaque) {
    Worker *worker = opaque;
    JSRuntime *rt = JS_NewRuntime();
    JSContext *ctx = rt ? JS_NewContext(rt) : NULL;
    worker->failed = !ctx;
    if (ctx) {
        worker->failed = install(ctx, worker->backing, worker->big);
        if (!worker->failed) worker->failed = result_equals(ctx, worker->script, "ok");
        JS_FreeContext(ctx);
    }
    if (rt) JS_FreeRuntime(rt);
    return NULL;
}
int quickjs_atomics_threads(const char *script) {
    for (int big = 0; big < 2; big++) {
        uint8_t *backing = calloc(1, 16);
        if (!backing) return 1;
        atomic_store(&finalized_buffers, 0);
        JSRuntime *rt = JS_NewRuntime();
        JSContext *ctx = rt ? JS_NewContext(rt) : NULL;
        int failed = !ctx;
        Worker worker = {backing, big, 0, script};
        pthread_t thread;
        int started = 0;
        if (ctx) failed = install(ctx, backing, big);
        if (!failed) { failed = pthread_create(&thread, NULL, work, &worker) != 0; started = !failed; }
        if (!failed) failed = result_equals(ctx, "for(let i=0;i<2000;i++)Atomics.add(new Int32Array(shared),2,1);'done'", "done");
        int notified = 0;
        for (int attempt = 0; !failed && !notified && attempt < 10000; attempt++) {
            const char *notify = "Atomics.notify(waitBigInt?new BigInt64Array(shared):new Int32Array(shared),0,1)";
            JSValue value = JS_Eval(ctx, notify, strlen(notify), "notify.js", JS_EVAL_TYPE_GLOBAL);
            if (JS_IsException(value) || JS_ToInt32(ctx, &notified, value)) failed = 1;
            JS_FreeValue(ctx, value);
            if (!notified) { struct timespec delay = {0, 1000000}; nanosleep(&delay, NULL); }
        }
        if (started) { pthread_join(thread, NULL); failed |= worker.failed; }
        if (!failed) failed = notified != 1 || result_equals(ctx, "Atomics.load(new Int32Array(shared),2)", "4000") ||
            result_equals(ctx, "Atomics.notify(new Int32Array(shared),0)", "0");
        if (ctx) JS_FreeContext(ctx);
        if (rt) JS_FreeRuntime(rt);
        failed |= atomic_load(&finalized_buffers) != 2;
        free(backing);
        if (failed) return 1;
        printf("CASE PASS atomics-cross-runtime-%s\n", big ? "bigint64" : "int32");
    }
    return 0;
}
