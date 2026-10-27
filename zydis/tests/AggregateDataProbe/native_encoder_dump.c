/* Exact original-C table storage, built separately from the managed product. */
#include <stdio.h>
#include <Zydis/Internal/EncoderData.h>

extern const ZydisEncodableInstruction encoder_instructions[7809];
_Static_assert(sizeof(ZydisEncodableInstruction) == 9, "encoder row layout changed");
_Static_assert(sizeof(encoder_instructions) == 70281, "encoder table size changed");

int main(int argc, char **argv)
{
    if (argc != 2) return 2;
    FILE *file = fopen(argv[1], "wb");
    if (!file) return 3;
    size_t written = fwrite(encoder_instructions, 1, sizeof(encoder_instructions), file);
    int closed = fclose(file);
    return written == sizeof(encoder_instructions) && closed == 0 ? 0 : 4;
}
