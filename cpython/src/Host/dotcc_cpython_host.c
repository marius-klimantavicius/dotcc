/* Authored embedding policy only. CPython retains its parser/evaluator/GC/GIL.
 * Compile as an ordinary translated unit with the selected CPython headers.
 * Native linking is exclusively a test control against the exact-pin archive. */
#define PY_SSIZE_T_CLEAN
#include <Python.h>
#include <stdatomic.h>
#include <stdlib.h>
#include <string.h>
#include "dotcc_cpython_host.h"

/* Exact-pin declaration from Include/internal/pycore_ceval.h. The public void
 * PyEval_SetTrace reports audit/monitor failures as unraisable exceptions.
 * The owning boundary needs the real int-returning API to propagate them. */
extern int _PyEval_SetTrace(PyThreadState *, Py_tracefunc, PyObject *);

typedef struct {
    PyThreadState *saved;
    PyObject *globals, *streams[2];
    char *output[2];
    size_t output_length[2];
    char *status_function, *status_message;
    char *error_type, *error_message, *error_traceback;
    size_t error_length[3];
    int status_kind, status_exitcode, live, clean;
    int active_waits, stopping; /* Accessed only while owning GIL. */
    _Atomic int cancelled;
    Py_tracefunc previous_trace;
    PyObject *previous_trace_object;
    int operation, tracing;
} dcp_context;

typedef struct {
    PyMethodDef method; /* method/name storage must outlive PyCFunction */
    char *name;
    int64_t token;
    dcp_int64_callback callback;
} dcp_callback;

static char *dcp_copy(const char *source, size_t length)
{
    char *copy = malloc(length + 1);
    if (copy != NULL) { memcpy(copy, source, length); copy[length] = 0; }
    return copy;
}

static char *dcp_copy_text(const char *text)
{ return text == NULL ? NULL : dcp_copy(text, strlen(text)); }

static int dcp_status(dcp_context *ctx, PyStatus status)
{
    ctx->status_kind = PyStatus_IsExit(status) ? 2 : (PyStatus_IsError(status) ? 1 : 0);
    ctx->status_exitcode = status.exitcode;
    free(ctx->status_function); free(ctx->status_message);
    ctx->status_function = dcp_copy_text(status.func);
    ctx->status_message = dcp_copy_text(status.err_msg);
    return ctx->status_kind;
}

static int dcp_add_path(PyConfig *config, const char *path, int standard_library,
                        dcp_context *ctx)
{
    wchar_t *wide = NULL;
    PyStatus status = PyConfig_SetBytesString(config, &wide, path);
    /* The frozen importer needs the selected stdlib root as well as sys.path.
     * SetString owns a separate copy, cleared with config on every exit. */
    if (!PyStatus_Exception(status) && standard_library)
        status = PyConfig_SetString(config, &config->stdlib_dir, wide);
    if (!PyStatus_Exception(status)) status = PyWideStringList_Append(&config->module_search_paths, wide);
    PyMem_RawFree(wide);
    return dcp_status(ctx, status);
}

void *dcp_create(void)
{
    dcp_context *ctx = calloc(1, sizeof(dcp_context));
    if (ctx != NULL) { ctx->clean = 1; atomic_init(&ctx->cancelled, 0); }
    return ctx;
}

void dcp_destroy(void *context)
{
    dcp_context *ctx = context;
    if (ctx == NULL || ctx->live || !ctx->clean) return;
    free(ctx->status_function); free(ctx->status_message);
    free(ctx->error_type); free(ctx->error_message); free(ctx->error_traceback);
    free(ctx->output[0]); free(ctx->output[1]); free(ctx);
}

/* Save error strings with a visible escaping policy for non-UTF8 surrogates.
 * Failures in __str__/traceback formatting are retained as formatting failures. */
static char *dcp_unicode_copy(PyObject *text, size_t *length)
{
    if (text == NULL) return NULL;
    PyObject *bytes = PyUnicode_AsEncodedString(text, "utf-8", "backslashreplace");
    if (bytes == NULL) return NULL;
    *length = (size_t)PyBytes_GET_SIZE(bytes);
    char *copy = dcp_copy(PyBytes_AS_STRING(bytes), *length);
    Py_DECREF(bytes);
    if (copy == NULL) PyErr_NoMemory();
    return copy;
}

void dcp_take_error(void *context)
{
    dcp_context *ctx = context;
    PyObject *error = PyErr_GetRaisedException();
    free(ctx->error_type); free(ctx->error_message); free(ctx->error_traceback);
    ctx->error_type = NULL; ctx->error_message = NULL; ctx->error_traceback = NULL;
    memset(ctx->error_length, 0, sizeof(ctx->error_length));
    if (error == NULL) return;
    int failed = 0;
    PyObject *type = PyObject_GetAttrString((PyObject *)Py_TYPE(error), "__name__");
    ctx->error_type = dcp_unicode_copy(type, &ctx->error_length[0]);
    if (PyErr_Occurred() || ctx->error_type == NULL) { failed = 1; PyErr_Clear(); }
    Py_XDECREF(type);
    PyObject *message = PyObject_Str(error);
    ctx->error_message = dcp_unicode_copy(message, &ctx->error_length[1]);
    if (PyErr_Occurred() || ctx->error_message == NULL) { failed = 1; PyErr_Clear(); }
    Py_XDECREF(message);
    /* Clear only formatting errors; the original exception remains owned. */
    PyErr_Clear();
    PyObject *traceback = PyImport_ImportModule("traceback");
    PyObject *lines = traceback == NULL ? NULL : PyObject_CallMethod(traceback, "format_exception", "O", error);
    PyObject *separator = PyUnicode_FromString("");
    PyObject *formatted = lines == NULL || separator == NULL ? NULL : PyUnicode_Join(separator, lines);
    ctx->error_traceback = dcp_unicode_copy(formatted, &ctx->error_length[2]);
    if (PyErr_Occurred() || ctx->error_traceback == NULL) failed = 1;
    Py_XDECREF(formatted); Py_XDECREF(separator); Py_XDECREF(lines); Py_XDECREF(traceback);
    Py_DECREF(error);
    PyErr_Clear();
    if (ctx->error_type == NULL) {
        ctx->error_type = dcp_copy_text("<exception type formatting failed>");
        ctx->error_length[0] = ctx->error_type == NULL ? 0 : strlen(ctx->error_type);
    }
    if (ctx->error_message == NULL) {
        ctx->error_message = dcp_copy_text("<exception message formatting failed>");
        ctx->error_length[1] = ctx->error_message == NULL ? 0 : strlen(ctx->error_message);
    }
    if (ctx->error_traceback == NULL) {
        ctx->error_traceback = dcp_copy_text("<traceback/exception formatting failed; original error type/message retained above>");
        ctx->error_length[2] = ctx->error_traceback == NULL ? 0 : strlen(ctx->error_traceback);
    } else if (failed) {
        /* Keep every frame that traceback.format_exception did produce. */
        const char note[] = "\n<exception type/message formatting failed>\n";
        size_t length = ctx->error_length[2];
        char *annotated = malloc(length + sizeof(note));
        if (annotated != NULL) {
            memcpy(annotated, ctx->error_traceback, length);
            memcpy(annotated + length, note, sizeof(note));
            free(ctx->error_traceback); ctx->error_traceback = annotated;
            ctx->error_length[2] += sizeof(note) - 1;
        }
    }
}

static int dcp_capture(dcp_context *ctx)
{
    PyObject *error = PyErr_GetRaisedException();
    int result = 0;
    for (int stream = 0; stream < 2; ++stream) {
        PyObject *text = PyObject_CallMethod(ctx->streams[stream], "getvalue", NULL);
        size_t length = 0;
        /* Stream/error diagnostics use visible backslash escapes for lone
         * surrogates; value conversion separately preserves ordinal values. */
        char *copy = dcp_unicode_copy(text, &length);
        Py_XDECREF(text);
        if (copy == NULL) { if (!PyErr_Occurred()) PyErr_NoMemory(); result = -1; break; }
        free(ctx->output[stream]); ctx->output[stream] = copy; ctx->output_length[stream] = length;
        PyObject *seek = PyObject_CallMethod(ctx->streams[stream], "seek", "i", 0);
        Py_XDECREF(seek);
        if (PyErr_Occurred()) { result = -1; break; }
        PyObject *truncate = PyObject_CallMethod(ctx->streams[stream], "truncate", "i", 0);
        Py_XDECREF(truncate);
        if (PyErr_Occurred()) { result = -1; break; }
    }
    if (error != NULL) {
        /* Preserve the primary Python error if capture itself failed. */
        PyObject *capture_error = PyErr_GetRaisedException();
        if (capture_error != NULL) {
            PyObject *prior = PyException_GetContext(error);
            PyException_SetContext(capture_error, prior);
            PyException_SetContext(error, capture_error);
        }
        PyErr_SetRaisedException(error);
    }
    return result;
}
int dcp_capture_output(void *context) { return dcp_capture(context); }

static void dcp_capsule_destroy(PyObject *capsule)
{
    dcp_callback *entry = PyCapsule_GetPointer(capsule, "dotcc.callback");
    if (entry != NULL) { free(entry->name); free(entry); }
}

static PyObject *dcp_callback_invoke(PyObject *capsule, PyObject *argument)
{
    dcp_callback *entry = PyCapsule_GetPointer(capsule, "dotcc.callback");
    if (entry == NULL) return NULL;
    int64_t input = PyLong_AsLongLong(argument), output = 0;
    if (PyErr_Occurred()) return NULL;
    char error[1024] = {0};
    int status = entry->callback(entry->token, input, &output, error, sizeof(error));
    error[sizeof(error) - 1] = 0;
    if (status != 0) {
        PyErr_SetString(PyExc_RuntimeError, error[0] ? error : "Managed callback failed.");
        return NULL;
    }
    if (PyErr_Occurred()) return NULL;
    return PyLong_FromLongLong(output);
}

static int dcp_cancelled(dcp_context *ctx)
{
    if (!atomic_load_explicit(&ctx->cancelled, memory_order_acquire)) return 0;
    PyErr_SetString(PyExc_KeyboardInterrupt, "Managed operation cancelled.");
    return 1;
}

static int dcp_trace(PyObject *capsule, PyFrameObject *frame, int what, PyObject *arg)
{
    (void)what; (void)arg;
    dcp_context *ctx = PyCapsule_GetPointer(capsule, "dotcc.operation");
    if (ctx == NULL) return -1;
    if (dcp_cancelled(ctx)) {
        /* Trace-hook failures do not automatically retain the interrupted
         * Python frame. Ask CPython to attach the actual frame, not a mirror. */
        PyTraceBack_Here(frame);
        return -1;
    }
    return 0;
}

/* Explicit embedding wait contract: actual timed CPython lock waits with GIL
 * released, checked every <=10ms; no thread abort or evaluator replacement. */
static PyObject *dcp_wait(PyObject *capsule, PyObject *argument)
{
    dcp_context *ctx = PyCapsule_GetPointer(capsule, "dotcc.operation");
    if (ctx == NULL) return NULL;
    if (ctx->stopping) {
        PyErr_SetString(PyExc_RuntimeError, "CPython embedding is stopping"); return NULL;
    }
    long long milliseconds = PyLong_AsLongLong(argument);
    if (PyErr_Occurred()) return NULL;
    if (milliseconds < 0 || milliseconds > 86400000) {
        PyErr_SetString(PyExc_ValueError, "wait_ms requires 0..86400000 milliseconds"); return NULL;
    }
    PyThread_type_lock lock = PyThread_allocate_lock();
    if (lock == NULL) return PyErr_NoMemory();
    PyThread_acquire_lock(lock, WAIT_LOCK);
    PyTime_t start, now;
    if (PyTime_Monotonic(&start) < 0) { PyThread_release_lock(lock); PyThread_free_lock(lock); return NULL; }
    ctx->active_waits++;
    for (;;) {
        if (dcp_cancelled(ctx)) break;
        if (PyTime_Monotonic(&now) < 0) break;
        long long remaining_us = milliseconds * 1000 - (now - start) / 1000;
        if (remaining_us <= 0) break;
        long long quantum = remaining_us > 10000 ? 10000 : remaining_us;
        Py_BEGIN_ALLOW_THREADS
        PyThread_acquire_lock_timed(lock, quantum, 0);
        Py_END_ALLOW_THREADS
    }
    ctx->active_waits--;
    PyThread_release_lock(lock); PyThread_free_lock(lock);
    if (PyErr_Occurred()) return NULL;
    Py_RETURN_NONE;
}

static PyMethodDef dcp_wait_method = {"wait_ms", dcp_wait, METH_O,
    "Cooperatively interruptible embedding wait in milliseconds."};

int dcp_initialize(void *context, const char *stdlib, const char *application)
{
    return dcp_initialize_configured(context, stdlib, application, 0, 1);
}

int dcp_initialize_configured(void *context, const char *stdlib, const char *application,
                             int write_bytecode, int utf8_mode)
{
    dcp_context *ctx = context;
    /* Reject invalid boundary values without entering preinitialization or
     * poisoning a fresh context. Existing live/unclean state remains honest. */
    if ((write_bytecode != 0 && write_bytecode != 1) || (utf8_mode != 0 && utf8_mode != 1))
        return dcp_status(ctx, PyStatus_Error("Startup boolean options must be 0 or 1"));
    if (Py_IsInitialized()) {
        ctx->clean = 0;
        return dcp_status(ctx, PyStatus_Error("CPython already initialized"));
    }
    /* Failed core initialization can leave partially constructed global state.
     * Only an actual completed finalization makes this context reusable. */
    ctx->clean = 0;
    PyPreConfig pre;
    PyPreConfig_InitIsolatedConfig(&pre); pre.utf8_mode = utf8_mode;
    if (dcp_status(ctx, Py_PreInitialize(&pre))) return ctx->status_kind;
    PyConfig config;
    PyConfig_InitIsolatedConfig(&config);
    config.install_signal_handlers = 0; config.site_import = 0;
    config.parse_argv = 0; config.write_bytecode = write_bytecode;
    config.module_search_paths_set = 1;
    PyStatus status = PyConfig_SetBytesString(&config, &config.program_name, "dotcc-cpython");
    if (dcp_status(ctx, status) == 0 && dcp_add_path(&config, stdlib, 1, ctx) == 0 &&
        dcp_add_path(&config, application, 0, ctx) == 0) {
        status = Py_InitializeFromConfig(&config);
        dcp_status(ctx, status); /* Copy before PyConfig_Clear. Never exit(). */
    }
    PyConfig_Clear(&config);
    if (ctx->status_kind) return ctx->status_kind;
    ctx->live = 1; ctx->clean = 0;
    PyObject *main = PyImport_AddModule("__main__");
    if (main != NULL) ctx->globals = Py_NewRef(PyModule_GetDict(main));
    PyObject *io = PyImport_ImportModule("io");
    PyObject *factory = io == NULL ? NULL : PyObject_GetAttrString(io, "StringIO");
    for (int stream = 0; factory != NULL && stream < 2 && !PyErr_Occurred(); ++stream) {
        ctx->streams[stream] = PyObject_CallNoArgs(factory);
        if (ctx->streams[stream] != NULL) {
            PySys_SetObject(stream ? "stderr" : "stdout", ctx->streams[stream]);
            if (!PyErr_Occurred()) PySys_SetObject(stream ? "__stderr__" : "__stdout__", ctx->streams[stream]);
        }
    }
    if (factory != NULL && !PyErr_Occurred()) {
        /* Explicit host stdin is EOF until a future input API is provided. */
        PyObject *input = PyObject_CallNoArgs(factory);
        if (input != NULL) {
            PySys_SetObject("stdin", input);
            if (!PyErr_Occurred()) PySys_SetObject("__stdin__", input);
            Py_DECREF(input);
        }
    }
    Py_XDECREF(factory); Py_XDECREF(io);
    PyObject *host = PyImport_AddModule("host");
    PyObject *capsule = PyCapsule_New(ctx, "dotcc.operation", NULL);
    PyObject *wait = capsule == NULL ? NULL : PyCFunction_New(&dcp_wait_method, capsule);
    if (host != NULL && wait != NULL) PyObject_SetAttrString(host, "wait_ms", wait);
    Py_XDECREF(wait); Py_XDECREF(capsule);
    if (PyErr_Occurred()) {
        dcp_take_error(ctx);
        dcp_status(ctx, PyStatus_Error("Embedding stream/module initialization failed; see boundary error"));
        dcp_finalize(ctx);
        return ctx->status_kind;
    }
    ctx->saved = PyEval_SaveThread();
    return 0;
}

int dcp_failure_clean(void *context) { return ((dcp_context *)context)->clean; }
int dcp_status_kind(void *context) { return ((dcp_context *)context)->status_kind; }
int dcp_status_exitcode(void *context) { return ((dcp_context *)context)->status_exitcode; }
const char *dcp_status_function(void *context) { return ((dcp_context *)context)->status_function; }
const char *dcp_status_message(void *context) { return ((dcp_context *)context)->status_message; }
const char *dcp_error_type(void *context) { return ((dcp_context *)context)->error_type; }
const char *dcp_error_message(void *context) { return ((dcp_context *)context)->error_message; }
const char *dcp_error_traceback(void *context) { return ((dcp_context *)context)->error_traceback; }
size_t dcp_error_length(void *context, int part) { return ((dcp_context *)context)->error_length[part]; }

void dcp_restore(void *context)
{
    dcp_context *ctx = context;
    PyEval_RestoreThread(ctx->saved); ctx->saved = NULL;
}
void dcp_save(void *context) { ((dcp_context *)context)->saved = PyEval_SaveThread(); }

void dcp_operation_begin(void *context, int can_cancel)
{
    dcp_context *ctx = context;
    if (ctx->operation) { PyErr_SetString(PyExc_RuntimeError, "Operation already active"); return; }
    atomic_store_explicit(&ctx->cancelled, 0, memory_order_release);
    ctx->operation = 1;
    /* Ordinary execution must preserve Python's trace/monitoring behavior.
     * Only an explicitly cancellable owning operation takes the trace hook. */
    if (!can_cancel) return;
    PyThreadState *thread = PyThreadState_Get();
    ctx->previous_trace = thread->c_tracefunc;
    ctx->previous_trace_object = Py_XNewRef(thread->c_traceobj);
    PyObject *capsule = PyCapsule_New(ctx, "dotcc.operation", NULL);
    if (capsule != NULL) {
        ctx->tracing = 1; /* Restore even if monitor setup fails after swapping. */
        _PyEval_SetTrace(thread, dcp_trace, capsule);
        Py_DECREF(capsule);
    }
}
void dcp_request_cancel(void *context)
{ atomic_store_explicit(&((dcp_context *)context)->cancelled, 1, memory_order_release); }
int dcp_operation_poll(void *context)
{ return PyErr_Occurred() || dcp_cancelled(context) ? -1 : 0; }
void dcp_operation_end(void *context)
{
    dcp_context *ctx = context;
    PyObject *error = PyErr_GetRaisedException();
    if (ctx->tracing) _PyEval_SetTrace(PyThreadState_Get(), ctx->previous_trace, ctx->previous_trace_object);
    Py_CLEAR(ctx->previous_trace_object); ctx->previous_trace = NULL; ctx->operation = 0; ctx->tracing = 0;
    atomic_store_explicit(&ctx->cancelled, 0, memory_order_release);
    if (error != NULL) {
        /* Retain the original Python exception; failed restoration is chained. */
        PyObject *restore_error = PyErr_GetRaisedException();
        if (restore_error != NULL) {
            PyObject *prior = PyException_GetContext(error);
            PyException_SetContext(restore_error, prior);
            PyException_SetContext(error, restore_error);
        }
        PyErr_SetRaisedException(error);
    }
}

static PyObject *dcp_run(dcp_context *ctx, const char *source, const char *filename, int mode)
{
    if (dcp_operation_poll(ctx) < 0) return NULL;
    PyObject *code = Py_CompileString(source, filename, mode);
    if (code == NULL) return NULL;
    PyObject *value = PyEval_EvalCode(code, ctx->globals, ctx->globals);
    Py_DECREF(code);
    return value;
}
int dcp_execute(void *context, const char *source, const char *filename)
{
    dcp_context *ctx = context;
    PyObject *result = dcp_run(ctx, source, filename, Py_file_input);
    int failed = result == NULL;
    Py_XDECREF(result);
    if (dcp_capture(ctx) < 0) failed = 1;
    return failed ? -1 : 0;
}
void *dcp_evaluate(void *context, const char *source, const char *filename)
{
    dcp_context *ctx = context;
    PyObject *result = dcp_run(ctx, source, filename, Py_eval_input);
    if (dcp_capture(ctx) < 0) { Py_XDECREF(result); return NULL; }
    return result;
}
const char *dcp_output(void *context, int stream, size_t *length)
{
    dcp_context *ctx = context;
    *length = ctx->output_length[stream]; return ctx->output[stream];
}

void dcp_register_callback(void *context, const char *module, const char *name,
                           int64_t token, dcp_int64_callback callback)
{
    (void)context;
    dcp_callback *entry = calloc(1, sizeof(dcp_callback));
    if (entry == NULL) { PyErr_NoMemory(); return; }
    entry->name = dcp_copy_text(name); entry->token = token; entry->callback = callback;
    if (entry->name == NULL) { free(entry); PyErr_NoMemory(); return; }
    entry->method.ml_name = entry->name; entry->method.ml_meth = dcp_callback_invoke;
    entry->method.ml_flags = METH_O;
    PyObject *capsule = PyCapsule_New(entry, "dotcc.callback", dcp_capsule_destroy);
    if (capsule == NULL) { free(entry->name); free(entry); return; }
    PyObject *callable = PyCFunction_New(&entry->method, capsule);
    PyObject *owner = callable == NULL ? NULL : PyImport_AddModule(module);
    if (owner != NULL) PyObject_SetAttrString(owner, name, callable);
    Py_XDECREF(callable); Py_DECREF(capsule);
}

void dcp_join_workers(void *context)
{
    dcp_context *ctx = context;
    /* This is CPython's own shutdown hook (also called by Py_FinalizeEx),
     * including registered threading atexit work and joins with GIL released. */
    PyObject *threading = PyImport_ImportModule("threading");
    PyObject *result = threading == NULL ? NULL : PyObject_CallMethod(threading, "_shutdown", NULL);
    Py_XDECREF(result); Py_XDECREF(threading);
    if (PyErr_Occurred()) return;
    ctx->stopping = 1;
    /* CPython intentionally does not join daemon threads. A daemon may still
     * be in our GIL-releasing C wait, so retain the context until every such
     * call exits. New host waits are rejected once ordinary shutdown finishes.
     * No managed monitor or worker-dependent lock is held during this drain. */
    if (ctx->active_waits == 0) return;
    PyThread_type_lock drain = PyThread_allocate_lock();
    if (drain == NULL) { PyErr_NoMemory(); return; }
    PyThread_acquire_lock(drain, WAIT_LOCK);
    while (ctx->active_waits > 0) {
        Py_BEGIN_ALLOW_THREADS
        PyThread_acquire_lock_timed(drain, 10000, 0);
        Py_END_ALLOW_THREADS
    }
    PyThread_release_lock(drain); PyThread_free_lock(drain);
}
int dcp_finalize(void *context)
{
    dcp_context *ctx = context;
    Py_CLEAR(ctx->globals); Py_CLEAR(ctx->streams[0]); Py_CLEAR(ctx->streams[1]);
    int result = Py_FinalizeEx();
    ctx->live = 0; ctx->clean = 1; ctx->saved = NULL;
    return result;
}
