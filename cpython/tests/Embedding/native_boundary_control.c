#define _POSIX_C_SOURCE 200809L
#define PY_SSIZE_T_CLEAN
#include <Python.h>
#include <assert.h>
#include <pthread.h>
#include <stdatomic.h>
#include <stdio.h>
#include <string.h>
#include <time.h>
#include "dotcc_cpython_host.h"

/* Native-only CONTROL of the authored C boundary, never a product backend or
 * evidence of a managed callback. Linked only to A's existing exact-pin archive. */
static _Atomic int callbacks, workers;
static unsigned long owner;
static double elapsed_cancellations[4];
static int cancellation_count;

static int callback(int64_t token, int64_t argument, int64_t *result, char *error, size_t capacity)
{
    assert(PyGILState_Check());
    atomic_fetch_add(&callbacks, 1);
    if (PyThread_get_thread_ident() != owner) atomic_fetch_add(&workers, 1);
    if (token == 2) { snprintf(error, capacity, "native control callback failure"); return -1; }
    assert(token == 1);
    *result = argument * 2;
    return 0;
}

static void error_is(void *ctx, const char *type, const char *message)
{
    assert(PyErr_Occurred());
    dcp_take_error(ctx);
    assert(!PyErr_Occurred());
    assert(strcmp(dcp_error_type(ctx), type) == 0);
    assert(strstr(dcp_error_message(ctx), message));
}

static void execute(void *ctx, const char *source)
{
    Py_tracefunc previous = PyThreadState_Get()->c_tracefunc;
    PyObject *previous_object = PyThreadState_Get()->c_traceobj;
    dcp_operation_begin(ctx, 0);
    assert(PyThreadState_Get()->c_tracefunc == previous && PyThreadState_Get()->c_traceobj == previous_object);
    assert(dcp_execute(ctx, source, "boundary_control.py") == 0);
    dcp_operation_end(ctx);
    assert(!PyErr_Occurred());
}

static double monotonic(void)
{
    struct timespec value;
    assert(clock_gettime(CLOCK_MONOTONIC, &value) == 0);
    return value.tv_sec + value.tv_nsec / 1e9;
}

static void *cancel(void *ctx)
{
    struct timespec delay = {0, 50000000};
    assert(nanosleep(&delay, NULL) == 0);
    dcp_request_cancel(ctx);
    return NULL;
}

static void cancellation(void *ctx, const char *source)
{
    pthread_t requester;
    dcp_operation_begin(ctx, 1);
    double start = monotonic();
    assert(pthread_create(&requester, NULL, cancel, ctx) == 0);
    assert(dcp_execute(ctx, source, "cancel_control.py") == -1);
    assert(pthread_join(requester, NULL) == 0); /* Drain before end/reset/free. */
    dcp_operation_end(ctx);
    double elapsed = monotonic() - start;
    elapsed_cancellations[cancellation_count++] = elapsed;
    assert(elapsed >= .025 && elapsed < 2.0);
    error_is(ctx, "KeyboardInterrupt", "Managed operation cancelled");
    if (!strstr(dcp_error_traceback(ctx), "cancel_control.py"))
        fprintf(stderr, "Cancellation traceback: %s\n", dcp_error_traceback(ctx));
    assert(strstr(dcp_error_traceback(ctx), "cancel_control.py"));
    execute(ctx, "assert 6 * 7 == 42\n"); /* Same interpreter remains usable. */
}

int main(int argc, char **argv)
{
    assert(argc == 3 || argc == 4);
    assert(strncmp(Py_GetVersion(), "3.13.15 ", 8) == 0);
    if (argc == 4 && strcmp(argv[3], "--bad-startup") == 0) {
        void *ctx = dcp_create(); assert(ctx);
        assert(dcp_initialize(ctx, "/dotcc/definitely/missing/stdlib", argv[2]) == 1);
        assert(dcp_status_kind(ctx) == 1);
        assert(dcp_status_message(ctx) && *dcp_status_message(ctx));
        assert(!dcp_failure_clean(ctx)); /* Partial init is never claimed safe. */
        printf("{\"scope\":\"native C boundary control\",\"nonfatal_status\":true,\"unsafe_reuse_rejected\":true}\n");
        return 0; /* Disposable control process; no recovery claim for partial init. */
    }
    for (int cycle = 0; cycle < 2; ++cycle) {
        atomic_store(&callbacks, 0); atomic_store(&workers, 0);
        void *ctx = dcp_create(); assert(ctx);
        assert(dcp_initialize(ctx, argv[1], argv[2]) == 0);
        assert(dcp_status_kind(ctx) == 0);
        dcp_restore(ctx); owner = PyThread_get_thread_ident();
        dcp_register_callback(ctx, "host", "double", 1, callback);
        dcp_register_callback(ctx, "host", "fail", 2, callback);
        assert(!PyErr_Occurred());
        execute(ctx,
            "import host, json, sys, threading, time\n"
            "assert sys.stdin.read() == '' and sys.__stdin__ is sys.stdin\n"
            "assert sys.__stdout__ is sys.stdout and sys.__stderr__ is sys.stderr\n"
            "values = [host.double(n) for n in (2, 3, 5)]\n"
            "assert json.dumps({'sum':sum(values), 'values':values}, separators=(',', ':'), sort_keys=True) == '{\"sum\":20,\"values\":[4,6,10]}'\n"
            "print('stream\\x00🐍', end='')\n"
            "sys.stderr.write('error stream\\n')\n");
        size_t length = 0;
        const char expected[] = "stream\0🐍";
        const char *output = dcp_output(ctx, 0, &length);
        assert(length == sizeof(expected) - 1 && memcmp(output, expected, length) == 0);
        output = dcp_output(ctx, 1, &length);
        assert(length == strlen("error stream\n") && memcmp(output, "error stream\n", length) == 0);
        assert(atomic_load(&callbacks) == 3);
        execute(ctx, "assert sys.gettrace() is None\n");
        execute(ctx, "sys.stdout.write('\\ud800')\n");
        output = dcp_output(ctx, 0, &length);
        assert(length == strlen("\\ud800") && memcmp(output, "\\ud800", length) == 0);

        PyObject *value = PyLong_FromLongLong(1234567890123LL); assert(value);
        Py_ssize_t references = Py_REFCNT(value);
        Py_INCREF(value); assert(Py_REFCNT(value) == references + 1);
        Py_DECREF(value); assert(Py_REFCNT(value) == references);
        assert(PyLong_AsLongLong(value) == 1234567890123LL); Py_DECREF(value);
        const Py_UCS4 points[] = {0x41, 0, 0x1f40d, 0xd800};
        value = PyUnicode_FromKindAndData(PyUnicode_4BYTE_KIND, points, 4); assert(value);
        Py_UCS4 *copy = PyUnicode_AsUCS4Copy(value); assert(copy);
        assert(PyUnicode_GetLength(value) == 4 && memcmp(copy, points, sizeof(points)) == 0);
        PyMem_Free(copy); Py_DECREF(value);
        execute(ctx, "assert host.double(21) == 42\n");
        assert(dcp_execute(ctx, "host.fail(1)", "callback_control.py") == -1);
        error_is(ctx, "RuntimeError", "native control callback failure");
        assert(strstr(dcp_error_traceback(ctx), "callback_control.py"));
        int count = atomic_load(&callbacks);
        assert(dcp_execute(ctx, "host.double('wrong')", "type_control.py") == -1);
        error_is(ctx, "TypeError", "integer"); assert(atomic_load(&callbacks) == count);
        assert(dcp_execute(ctx, "host.double(1 << 100)", "overflow_control.py") == -1);
        error_is(ctx, "OverflowError", "too big"); assert(atomic_load(&callbacks) == count);
        assert(dcp_execute(ctx, "def broken():\n    raise ValueError('precise λ')\nbroken()\n", "trace_control.py") == -1);
        error_is(ctx, "ValueError", "precise λ");
        assert(strstr(dcp_error_traceback(ctx), "trace_control.py") && strstr(dcp_error_traceback(ctx), "in broken"));
        assert(dcp_execute(ctx, "print('before error')\nraise ValueError('a\\x00b')", "nul_error.py") == -1);
        dcp_take_error(ctx);
        const char nul_message[] = "a\0b";
        assert(dcp_error_length(ctx, 1) == sizeof(nul_message) - 1);
        assert(memcmp(dcp_error_message(ctx), nul_message, sizeof(nul_message) - 1) == 0);
        output = dcp_output(ctx, 0, &length);
        assert(length == strlen("before error\n") && memcmp(output, "before error\n", length) == 0);
        assert(dcp_execute(ctx, "raise SystemExit(7)", "exit_control.py") == -1);
        error_is(ctx, "SystemExit", "7"); /* Never exits embedding process. */
        assert(dcp_execute(ctx, "class BadError(Exception):\n    def __str__(self): raise RuntimeError('format failure')\nraise BadError()", "format_control.py") == -1);
        dcp_take_error(ctx);
        assert(strstr(dcp_error_message(ctx), "formatting failed"));
        assert(strstr(dcp_error_traceback(ctx), "formatting failed") && !PyErr_Occurred());
        assert(strstr(dcp_error_traceback(ctx), "format_control.py"));
        execute(ctx, "assert 42 == 42\n");

        /* Actual pre-existing Python trace is restored after cancellation. */
        assert(dcp_execute(ctx, "_trace_events = []\ndef previous_trace(frame, event, arg):\n    _trace_events.append(event)\n    return previous_trace\nsys.settrace(previous_trace)\n", "previous_trace.py") == 0);
        execute(ctx, "_trace_events.clear()\nassert sys.gettrace() is previous_trace\nassert _trace_events\n");
        Py_tracefunc previous = PyThreadState_Get()->c_tracefunc;
        PyObject *previous_object = PyThreadState_Get()->c_traceobj;
        assert(previous && previous_object);
        cancellation(ctx, "while True:\n    pass\n");
        assert(PyThreadState_Get()->c_tracefunc == previous && PyThreadState_Get()->c_traceobj == previous_object);
        cancellation(ctx, "host.wait_ms(30000)\n");
        assert(PyThreadState_Get()->c_tracefunc == previous && PyThreadState_Get()->c_traceobj == previous_object);
        assert(dcp_execute(ctx, "sys.settrace(None)", "clear_trace.py") == 0);
        dcp_operation_begin(ctx, 1); dcp_request_cancel(ctx);
        assert(dcp_execute(ctx, "raise AssertionError('must not execute')", "pre_cancel.py") == -1);
        dcp_operation_end(ctx); error_is(ctx, "KeyboardInterrupt", "cancelled");
        execute(ctx, "host.wait_ms(20)\n");

        /* Leave real workers alive across owner save/restore; shutdown joins
         * while workers acquire GIL and invoke the rooted native control. */
        execute(ctx,
            "worker_values = []\n"
            "def worker(n):\n    time.sleep(.02)\n    worker_values.append(host.double(n))\n"
            "daemon_entered = threading.Event()\n"
            "def daemon_wait():\n    daemon_entered.set()\n    host.wait_ms(200)\n"
            "daemon = threading.Thread(target=daemon_wait, daemon=True)\n"
            "daemon.start()\n"
            "assert daemon_entered.wait(2)\n"
            "threads = [threading.Thread(target=worker, args=(n,)) for n in (10, 11)]\n"
            "for thread in threads: thread.start()\n");
        dcp_save(ctx); dcp_restore(ctx);
        double shutdown_start = monotonic();
        dcp_join_workers(ctx); assert(!PyErr_Occurred());
        assert(monotonic() - shutdown_start >= .10); /* Drains daemon C wait. */
        assert(atomic_load(&workers) == 2);
        execute(ctx, "daemon.join(1)\nassert not daemon.is_alive()\n");
        assert(dcp_execute(ctx, "host.wait_ms(0)", "stopping_control.py") == -1);
        error_is(ctx, "RuntimeError", "embedding is stopping");
        value = dcp_evaluate(ctx, "sorted(worker_values) == [20, 22]", "workers.py");
        assert(value && PyObject_IsTrue(value) == 1); Py_DECREF(value);
        if (cycle == 1) execute(ctx,
            "class BadFlush:\n"
            "    def write(self, text): return len(text)\n"
            "    def flush(self): raise OSError('flush control failure')\n"
            "sys.stdout = BadFlush()\n");
        assert(dcp_finalize(ctx) == (cycle == 1 ? -1 : 0));
        assert(dcp_failure_clean(ctx) && !Py_IsInitialized());
        dcp_destroy(ctx);
    }
    puts("{\"scope\":\"native C boundary control; no managed/translated pass\",\"cycles\":2,\"streams\":true,\"errors\":true,\"refcounts\":true,\"worker_callbacks_per_cycle\":2,\"loop_cancellation\":true,\"wait_cancellation\":true,\"trace_restored\":true,\"reusable_after_cancel\":true,\"finalize_flush_error\":true}");
    printf("{\"cancellation_seconds\":[%.6f,%.6f,%.6f,%.6f],\"required_max_seconds\":2}\n",
        elapsed_cancellations[0], elapsed_cancellations[1], elapsed_cancellations[2], elapsed_cancellations[3]);
    return 0;
}
