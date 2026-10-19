#ifndef PRIVATE_TAGGED_VARIANTS_H
#define PRIVATE_TAGGED_VARIANTS_H
struct State;
typedef struct Codec {
    int (*read)(struct State *);
    struct State *state;
} Codec;
#ifndef EXTRA_STATE
#define EXTRA_STATE
#endif
typedef struct State {
    int value;
    Codec *codec;
    EXTRA_STATE
} State;
#endif
