#include "codec.h"
typedef struct State { int value; Codec *codec; } State;
static int read_first(State *p) { return p->value + (p->codec->state==p ? 0:100); }
static State first_state = {7, 0};
static Codec first_codec = {read_first, &first_state, &first_codec, {&first_state, &first_state}};
void *first(void) { first_state.codec=&first_codec; return &first_codec; }
int first_size(void) { return sizeof(State); }
