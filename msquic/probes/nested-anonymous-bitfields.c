/* C11 anonymous union + anonymous bitfield struct from QUIC_STREAM_FRAME_TYPE. */
#include <stdint.h>
typedef struct FrameType {
    union {
        struct { uint8_t FIN : 1; uint8_t LEN : 1; uint8_t OFF : 1; uint8_t FrameType : 5; };
        uint8_t Type;
    };
} FrameType;
int main(void) {
    FrameType frame = { .Type = 0x0a };
    return frame.LEN != 1;
}
