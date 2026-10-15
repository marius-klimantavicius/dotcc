/* Native ABI fixture adapter only. Record the supplied int/double test values
   and echo the fixed marker; no translated application body lives here. */
#include "fixture-va.h"
#include "outbound.h"

static int captured_integers[10];
static double captured_doubles[10];

int native_capture_variadic(int marker, ...) {
    fixture_va_list cursor;
    fixture_va_start(cursor, marker);
    for (int index = 0; index < 10; ++index) {
        captured_integers[index] = fixture_va_arg(cursor, int);
        captured_doubles[index] = fixture_va_arg(cursor, double);
    }
    fixture_va_end(cursor);
    return marker;
}

fixture_outbound_callback native_outbound_provider(void) {
    return native_capture_variadic;
}

fixture_outbound_callback native_outbound_echo(fixture_outbound_callback callback) {
    return callback;
}

int native_captured_integer(int index) { return captured_integers[index]; }
double native_captured_double(int index) { return captured_doubles[index]; }
