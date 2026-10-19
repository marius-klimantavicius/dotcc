#ifndef DOTCC_CPYTHON_HOST_H
#define DOTCC_CPYTHON_HOST_H
#include <stddef.h>
#include <stdint.h>

/* Opaque handles are real CPython objects. All entries except request_cancel
 * require the owning thread/GIL as documented. No native backend is shipped.
 * Callback: 0 = success, -1 = error; error is caller-owned NUL-terminated UTF-8.
 * The callback must contain all managed exceptions and return a fresh result. */
typedef int (*dcp_int64_callback)(int64_t token, int64_t argument,
                                int64_t *result, char *error, size_t capacity);
void *dcp_create(void);
void dcp_destroy(void *context);
int dcp_initialize(void *context, const char *stdlib, const char *application);
/* Explicit isolated startup, before imports/discovery. Boolean inputs are 0/1.
 * dcp_initialize retains defaults write_bytecode=0 and utf8_mode=1. */
int dcp_initialize_configured(void *context, const char *stdlib, const char *application,
                             int write_bytecode, int utf8_mode);
int dcp_failure_clean(void *context);
int dcp_status_kind(void *context);
int dcp_status_exitcode(void *context);
const char *dcp_status_function(void *context);
const char *dcp_status_message(void *context);
void dcp_restore(void *context);
void dcp_save(void *context);
int dcp_execute(void *context, const char *source, const char *filename);
void *dcp_evaluate(void *context, const char *source, const char *filename);
const char *dcp_output(void *context, int stream, size_t *length);
int dcp_capture_output(void *context);
void dcp_take_error(void *context);
const char *dcp_error_type(void *context);
const char *dcp_error_message(void *context);
const char *dcp_error_traceback(void *context);
size_t dcp_error_length(void *context, int part);
void dcp_register_callback(void *context, const char *module, const char *name,
                           int64_t token, dcp_int64_callback callback);
void dcp_join_workers(void *context);
int dcp_finalize(void *context);
/* Operation brackets require GIL; request is atomic, requires NO GIL, and is
 * valid only until its caller has drained/joined before operation_end. */
void dcp_operation_begin(void *context, int can_cancel);
int dcp_operation_poll(void *context);
void dcp_request_cancel(void *context);
void dcp_operation_end(void *context);

/* Ordinary object/numeric/Unicode/reference APIs are consumed directly from
 * generated Python.h declarations. Link with --export-inline Py_INCREF,
 * --export-inline Py_DECREF, --export-enum PyUnicode_4BYTE_KIND. */
#endif
