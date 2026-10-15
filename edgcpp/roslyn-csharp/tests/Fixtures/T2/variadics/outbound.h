#ifndef ROSLYN_CSHARP_T2_OUTBOUND_H
#define ROSLYN_CSHARP_T2_OUTBOUND_H
typedef int (*fixture_outbound_callback)(int marker, ...);
int native_capture_variadic(int marker, ...);
fixture_outbound_callback native_outbound_provider(void);
fixture_outbound_callback native_outbound_echo(fixture_outbound_callback callback);
int native_captured_integer(int index);
double native_captured_double(int index);
int fixture(void);
#endif
