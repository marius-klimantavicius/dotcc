#include <stdio.h>

static int effects;
static int effect(void) { return ++effects; }

/* One scalar-only table, 8 * 248 = 1984 bytes on the supported target.
   Keep every operation in this table so one full-size aggregate-data marker
   proves that none of the interesting initializers silently fell back. */
struct OperationRow {
    unsigned shifts[4];
    long long wide_shifts[2];
    int signed_divmod[4];
    unsigned unsigned_divmod[2];
    unsigned bits[4];
    unsigned wrapped[4];
    int comparisons[6];
    long long mixed_arithmetic[3];
    double float_steps[4];
    float float_values[2];
    double double_steps[4];
    int logical[4];
    int conditional[2];
    int unary[4];
};

static struct OperationRow operations[8] = {
    [0] = {
        .shifts = {1u << 31, 0x80000000u >> 31, (unsigned char)128 << 1, 1u << 3ULL},
        .wide_shifts = {1LL << 40, 0x4000000000000000LL >> 60},
        .signed_divmod = {-37 / 5, -37 % 5, 37 / -5, 37 % -5},
        .unsigned_divmod = {0xffffffffu / 3u, 0xffffffffu % 7u},
        .bits = {0xa5a55a5au & 0x0ff00ff0u, 0xa5a55a5au | 0x0ff00ff0u,
                 0xa5a55a5au ^ 0x0ff00ff0u, ~0xf0f00f0fu},
        .wrapped = {0xffffffffu + 2u, 0u - 3u, 0x80000001u * 2u, (unsigned short)65537},
        .comparisons = {-1 < 1u, -1 > 1u, -1LL <= 1u, 1u >= -1LL,
                        -1 == 0xffffffffu, -1LL != 0xffffffffu},
        .mixed_arithmetic = {-2LL + 1u, 7LL - 9u, -3LL * 5u},
        /* Each float operation must round before conversion to double. */
        .float_steps = {16777216.0f + 1.0f, (16777216.0f + 1.0f) - 16777216.0f,
                        (16777216.0f + 1.0f) * 0.5f, 1.0f / 10.0f},
        .float_values = {16777217.0f, -0.0f},
        .double_steps = {9007199254740992.0 + 1.0,
                         (9007199254740992.0 + 1.0) - 9007199254740992.0,
                         1.0 / 10.0, (1.5 + 0.25) * 2.0},
        /* C17 6.6 permits effects in subexpressions that are not evaluated. */
        .logical = {0 && effect(), 1 || ++effects, 1 && 7, 0 || -3},
        .conditional = {1 ? 41 : effect(), 0 ? ++effects : -43},
        .unary = {+17, -19, ~0, !42}
    },
    [1] = {
        .shifts = {0xffffffffu >> 4, 3u << 30, (unsigned short)32769 >> 1, 1u << 0ULL},
        .wide_shifts = {3LL << 32, (1LL << 48) >> 32},
        .signed_divmod = {-37 / -5, -37 % -5, 37 / 5, 37 % 5},
        .unsigned_divmod = {0x80000000u / 65536u, 0x80000001u % 65536u},
        .bits = {0xf0u & 0x0fu, 0xf0u | 0x0fu, 0xf0u ^ 0x0fu, ~0u},
        .wrapped = {0xfffffffeu + 3u, 1u - 2u, 0x40000000u * 4u, (unsigned char)258},
        .comparisons = {1u < -1, 1u > -1, 0u <= -1, -1LL >= 0u,
                        1u == -1LL, 0xffffffffu != -1},
        .mixed_arithmetic = {-1 + 1u, -2LL + 0xffffffffu, 0xffffffffu - 1LL},
        .float_steps = {(1.0f / 10.0f) + 1.0f, 1.0f - (1.0f / 10.0f),
                        (1.0f / 10.0f) * 10.0f, -1.0f / 10.0f},
        .float_values = {(16777216.0f + 3.0f) - 16777216.0f, -0.0f / 2.0f},
        .double_steps = {(9007199254740992.0 + 3.0) - 9007199254740992.0,
                         1.0 - 0.1, -1.0 / 10.0, (1.25 * 2.0) - 0.5},
        .logical = {0 && ++effects, 2 || effect(), 1 && 0, 0 || 0},
        .conditional = {0 ? effect() : 47, 1 ? -53 : (effects = 99)},
        .unary = {+0, -0, ~1, !0}
    },
    [7] = {.conditional = {1 ? 0x1234 : effect(), 0 ? (effects = 88) : -0x1234}}
};

int main(void) {
    unsigned char *bytes = (unsigned char *)operations;
    int row_size = sizeof(operations[0]);
    printf("rows=8 stride=%d bytes=%d\n", row_size, (int)sizeof(operations));
    /* Compare all storage, including zero rows and the last designated row. */
    for (int row = 0; row < 8; ++row) {
        printf("%d:", row);
        for (int i = 0; i < row_size; ++i)
            printf("%02x", (unsigned)bytes[row * row_size + i]);
        printf("\n");
    }
    for (int row = 0; row < 2; ++row) {
        struct OperationRow *p = &operations[row];
        printf("values%d shifts=%u,%u,%u,%u wide=%lld,%lld\n", row,
            p->shifts[0], p->shifts[1], p->shifts[2], p->shifts[3],
            p->wide_shifts[0], p->wide_shifts[1]);
        printf("divmod%d signed=%d,%d,%d,%d unsigned=%u,%u\n", row,
            p->signed_divmod[0], p->signed_divmod[1], p->signed_divmod[2], p->signed_divmod[3],
            p->unsigned_divmod[0], p->unsigned_divmod[1]);
        printf("bits%d=%08x,%08x,%08x,%08x wrap=%u,%u,%u,%u\n", row,
            p->bits[0], p->bits[1], p->bits[2], p->bits[3],
            p->wrapped[0], p->wrapped[1], p->wrapped[2], p->wrapped[3]);
        printf("mixed%d compare=%d,%d,%d,%d,%d,%d arithmetic=%lld,%lld,%lld\n", row,
            p->comparisons[0], p->comparisons[1], p->comparisons[2], p->comparisons[3],
            p->comparisons[4], p->comparisons[5],
            p->mixed_arithmetic[0], p->mixed_arithmetic[1], p->mixed_arithmetic[2]);
        printf("logic%d=%d,%d,%d,%d conditional=%d,%d unary=%d,%d,%d,%d\n", row,
            p->logical[0], p->logical[1], p->logical[2], p->logical[3],
            p->conditional[0], p->conditional[1],
            p->unary[0], p->unary[1], p->unary[2], p->unary[3]);
    }
    int float_ok = operations[0].float_steps[0] == 16777216.0
        && operations[0].float_steps[1] == 0.0 && operations[0].float_steps[2] == 8388608.0
        && operations[0].float_steps[3] == 0.100000001490116119384765625
        && operations[0].float_values[0] == 16777216.0f
        && operations[1].float_steps[0] == 1.10000002384185791015625
        && operations[1].float_steps[1] == 0.89999997615814208984375
        && operations[1].float_steps[2] == 1.0
        && operations[1].float_steps[3] == -0.100000001490116119384765625
        && operations[1].float_values[0] == 4.0f;
    int double_ok = operations[0].double_steps[0] == 9007199254740992.0
        && operations[0].double_steps[1] == 0.0 && operations[0].double_steps[2] == 0.1
        && operations[0].double_steps[3] == 3.5 && operations[1].double_steps[0] == 4.0
        && operations[1].double_steps[1] == 0.9 && operations[1].double_steps[2] == -0.1
        && operations[1].double_steps[3] == 2.0;
    int zero_ok = 1;
    for (int i = 2 * row_size; i < 7 * row_size; ++i)
        if (bytes[i] != 0) zero_ok = 0;
    int tail_ok = operations[7].conditional[0] == 0x1234 && operations[7].conditional[1] == -0x1234;
    printf("float32=%d double=%d zero=%d tail=%d effects=%d\n",
        float_ok, double_ok, zero_ok, tail_ok, effects);
    return float_ok && double_ok && zero_ok && tail_ok && effects == 0 ? 0 : 1;
}
