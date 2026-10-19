#include "shared.h"
static int read_first(struct State *state) { return state->value; }
static State state = {7, 0};
static Codec codec = {read_first, &state};
int first_size(void) { return sizeof(state); }
int first_value(void) { state.codec=&codec; return state.codec->read(state.codec->state); }
