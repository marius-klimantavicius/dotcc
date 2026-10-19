#include "codec.h"
typedef struct State { int value; Codec *codec; long extension; } State;
static int read_second(State *p) { return p->value + p->extension + (p->codec->state==p ? 0:100); }
static State second_state = {19, 0, 23};
static Codec second_codec = {read_second, &second_state, &second_codec, {&second_state, &second_state}};
void *second(void) { second_state.codec=&second_codec; return &second_codec; }
int second_size(void) { return sizeof(State); }
