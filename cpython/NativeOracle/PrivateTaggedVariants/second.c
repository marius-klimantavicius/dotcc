#define EXTRA_STATE long extension;
#include "shared.h"
static int read_second(struct State *state) { return state->value + (int)state->extension; }
static State state = {19, 0, 23};
static Codec codec = {read_second, &state};
int second_size(void) { return sizeof(state); }
int second_value(void) { state.codec=&codec; return state.codec->read(state.codec->state); }
