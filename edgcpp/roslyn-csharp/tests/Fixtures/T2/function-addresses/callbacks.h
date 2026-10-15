#ifndef ROSLYN_CSHARP_T2_CALLBACKS_H
#define ROSLYN_CSHARP_T2_CALLBACKS_H

typedef int (*fixture_callback)(int);
typedef void (*fixture_erased_callback)(void);

/* Narrow native test adapters; no translated application implementation. */
void native_reset_events(void);
void native_mark_event(int event);
int native_event_count(void);
int native_event_at(int index);
fixture_callback native_echo_callback(fixture_callback callback);
int native_invoke_callback(fixture_callback callback, int value);
fixture_callback native_callback_provider(void);

int fixture(void);

#endif
