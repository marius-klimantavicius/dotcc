#include "c.h"
#include "port/pg_bitutils.h"
#include "port/pg_crc32c.h"

/* Real upstream software functions must agree with independent small vectors. */
int dpg_portable_codegen_check(void)
{
    const unsigned char vector[] = "123456789";
    unsigned char bytes[259];
    pg_crc32c crc;
    INIT_CRC32C(crc);
    COMP_CRC32C(crc, vector, 9);
    FIN_CRC32C(crc);
    if (crc != UINT32_C(0xe3069283)) return 1;
    if (pg_popcount32(UINT32_C(0xf0000101)) != 6) return 2;
    if (pg_popcount64(UINT64_C(0x80000001f0000101)) != 8) return 3;
    for (unsigned int i = 0; i < sizeof(bytes); ++i) bytes[i] = (unsigned char)i;
    for (unsigned int offset = 0; offset < 3; ++offset)
    {
        for (unsigned int length = 0; length <= 256; ++length)
        {
            unsigned int count = 0, masked = 0;
            uint32 slow_crc = UINT32_C(0xffffffff);
            for (unsigned int i = 0; i < length; ++i)
            {
                unsigned char value = bytes[offset + i];
                for (int bit = 0; bit < 8; ++bit)
                {
                    count += (value >> bit) & 1;
                    masked += ((value & 0xa5) >> bit) & 1;
                }
                slow_crc ^= value;
                for (int bit = 0; bit < 8; ++bit)
                    slow_crc = (slow_crc >> 1) ^ ((slow_crc & 1) ? UINT32_C(0x82f63b78) : 0);
            }
            if (pg_popcount((const char *)bytes + offset, length) != count) return 4;
            if (pg_popcount_masked((const char *)bytes + offset, length, 0xa5) != masked) return 5;
            INIT_CRC32C(crc);
            COMP_CRC32C(crc, bytes + offset, length);
            FIN_CRC32C(crc);
            if (crc != (slow_crc ^ UINT32_C(0xffffffff))) return 6;
        }
    }
    return 0;
}

#ifdef PORTABLE_CODEGEN_NATIVE
int main(void) { return dpg_portable_codegen_check(); }
#endif
