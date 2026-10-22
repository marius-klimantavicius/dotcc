/* Execution control only. All allocation/reset/stemming algorithms are the
 * original api.c, utilities.c and generated English/Lithuanian source files.
 * Use the original libstemmer/header.h, without PostgreSQL's palloc remapping.
 * Full-module objects are checked separately with the PostgreSQL header. */
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "header.h"
#include "stem_UTF_8_english.h"
#include "stem_UTF_8_lithuanian.h"

struct dsb_case
{
    int language;
    const symbol *input;
    int input_size;
    const symbol *expected;
    int expected_size;
};
#include "cases.h"

#define CHECK(condition) do { if (!(condition)) { result = __LINE__; goto done; } } while (0)

static int owner_sequence;

int dsb_owner_sequence(void)
{
    return ++owner_sequence;
}

static void print_hex(const symbol *bytes, int length)
{
    int i;
    for (i = 0; i < length; i++) printf("%02x", (unsigned int)bytes[i]);
}

int dsb_run(void)
{
    struct SN_env *generic = NULL;
    struct SN_env *english = NULL;
    struct SN_env *lithuanian = NULL;
    symbol *long_word = NULL;
    symbol copy[128];
    int result = 0;
    int i, round, cycle;

    /* SN_create_env allocates its p/S/I buffers and zeroes all I slots. */
    generic = SN_create_env(3, 5);
    CHECK(generic != NULL);
    CHECK(generic->p != NULL && generic->S != NULL && generic->I != NULL);
    CHECK(generic->l == 0 && generic->c == 0 && SIZE(generic->p) == 0);
    for (i = 0; i < 3; i++) CHECK(generic->S[i] != NULL && SIZE(generic->S[i]) == 0);
    for (i = 0; i < 5; i++) CHECK(generic->I[i] == 0);
    SN_close_env(generic, 3);
    generic = NULL;
    SN_close_env(NULL, 0);

    english = english_UTF_8_create_env();
    lithuanian = lithuanian_UTF_8_create_env();
    CHECK(english != NULL && lithuanian != NULL);
    CHECK(english != lithuanian && english->p != lithuanian->p);
    CHECK(english->I != NULL && lithuanian->I != NULL && english->I != lithuanian->I);

    /* Force the real replace_s/increase_size allocation path, then shrink,
     * reset the cursor, and stem many different words in these same owners. */
    long_word = (symbol *)malloc(8192);
    CHECK(long_word != NULL);
    for (i = 0; i < 8192; i++) long_word[i] = (symbol)('a' + i % 26);
    for (i = 2; i <= 8192; i *= 8)
    {
        CHECK(SN_set_current(english, i, long_word) == 0);
        CHECK(english->l == i && english->c == 0 && CAPACITY(english->p) >= i);
        CHECK(SIZE(english->p) == i && memcmp(english->p, long_word, i) == 0);
    }
    CHECK(SN_set_current(english, 8192, long_word) == 0);
    CHECK(english->l == 8192 && memcmp(english->p, long_word, 8192) == 0);

    for (round = 0; round < DSB_ROUNDS; round++)
    {
        for (i = 0; i < DSB_CASE_COUNT; i++)
        {
            const struct dsb_case *item = &dsb_cases[i];
            struct SN_env *environment = item->language == 0 ? english : lithuanian;
            struct SN_env *other = item->language == 0 ? lithuanian : english;
            int other_size = other->l;
            CHECK(item->input_size < (int)sizeof(copy));
            memcpy(copy, item->input, item->input_size);
            environment->c = environment->l;
            CHECK(SN_set_current(environment, item->input_size, copy) == 0);
            CHECK(environment->c == 0 && environment->l == item->input_size);
            CHECK(SIZE(environment->p) == item->input_size);
            CHECK(memcmp(environment->p, item->input, item->input_size) == 0);
            memset(copy, 0xff, sizeof(copy));
            CHECK(memcmp(environment->p, item->input, item->input_size) == 0);
            CHECK((item->language == 0 ? english_UTF_8_stem(environment)
                                       : lithuanian_UTF_8_stem(environment)) == 1);
            CHECK(environment->l == item->expected_size);
            CHECK(memcmp(environment->p, item->expected, item->expected_size) == 0);
            CHECK(other->l == other_size);
            if (round == 0)
            {
                printf("stem %d %d ", item->language, i);
                print_hex(item->input, item->input_size);
                printf(" ");
                print_hex(environment->p, environment->l);
                printf("\n");
            }
        }
    }

    /* Actual Unicode utility code counts/steps code points, not UTF-8 bytes. */
    {
        static const symbol unicode_word[] = {0xc4, 0x85, 0xc5, 0xbe, 'u', 'o', 'l', 'a', 's'};
        CHECK(SN_set_current(lithuanian, sizeof(unicode_word), unicode_word) == 0);
        CHECK(len_utf8(lithuanian->p) == 7);
        CHECK(skip_utf8(lithuanian->p, 0, lithuanian->l, 2) == 4);
        CHECK(skip_b_utf8(lithuanian->p, lithuanian->l, 0, 7) == 0);
        CHECK(skip_utf8(lithuanian->p, 0, lithuanian->l, 8) == -1);
        CHECK(skip_b_utf8(lithuanian->p, lithuanian->l, 0, 8) == -1);
    }

    /* Free and recreate real stemmer environments repeatedly. */
    for (cycle = 0; cycle < DSB_ALLOCATION_CYCLES; cycle++)
    {
        english_UTF_8_close_env(english);
        english = english_UTF_8_create_env();
        CHECK(english != NULL);
        CHECK(SN_set_current(english, 7, (const symbol *)"running") == 0);
        CHECK(english_UTF_8_stem(english) == 1);
        CHECK(english->l == 3 && memcmp(english->p, "run", 3) == 0);
        lithuanian_UTF_8_close_env(lithuanian);
        lithuanian = lithuanian_UTF_8_create_env();
        CHECK(lithuanian != NULL);
        CHECK(SN_set_current(lithuanian, 0, (const symbol *)"") == 0);
        CHECK(lithuanian_UTF_8_stem(lithuanian) == 1 && lithuanian->l == 0);
    }
    printf("snowball cases=%d rounds=%d allocation_cycles=%d\n",
           DSB_CASE_COUNT, DSB_ROUNDS, DSB_ALLOCATION_CYCLES);
done:
    if (generic != NULL) SN_close_env(generic, 3);
    if (english != NULL) english_UTF_8_close_env(english);
    if (lithuanian != NULL) lithuanian_UTF_8_close_env(lithuanian);
    free(long_word);
    if (result != 0) fprintf(stderr, "Snowball contract failed at probe.c:%d\n", result);
    return result;
}

#ifdef DSB_NATIVE_MAIN
int main(void)
{
    int result;
    if (dsb_owner_sequence() != 1) return 2;
    result = dsb_run();
    if (result != 0) return 1;
    return dsb_owner_sequence() == 2 ? 0 : 3;
}
#endif
