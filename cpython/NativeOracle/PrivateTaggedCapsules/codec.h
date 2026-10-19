struct State;
typedef struct Codec {
    int (*read)(struct State *);
    struct State *state;
    struct Codec *self;
    struct State *slots[2];
} Codec;
