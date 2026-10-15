/* Narrow native ABI test adapters only. This file has no source application body.
   The provider is deliberately an external identity callback. A future generated
   run may load this adapter library; it must never link callbacks.c natively. */
#include "callbacks.h"

static int events[64];
static int event_count;

void native_reset_events(void) { event_count = 0; }

void native_mark_event(int event) {
    if (event_count >= 64) __builtin_trap();
    events[event_count++] = event;
}

int native_event_count(void) { return event_count; }

int native_event_at(int index) {
    return index >= 0 && index < event_count ? events[index] : -1;
}

fixture_callback native_echo_callback(fixture_callback callback) { return callback; }

int native_invoke_callback(fixture_callback callback, int value) {
    native_mark_event(21);
    return callback(value);
}

static int native_identity_callback(int value) {
    native_mark_event(41);
    return value;
}

fixture_callback native_callback_provider(void) { return native_identity_callback; }
