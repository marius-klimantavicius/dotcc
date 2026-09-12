#include "precomp.h"
#include "observe.h"
int main(void) {
    ABI_LAYOUT(QUIC_STREAM_FRAME_TYPE);
    ABI_LAYOUT(QUIC_DATAGRAM_FRAME_TYPE);
    ABI_LAYOUT(QUIC_STREAM_EX);
    ABI_OFFSET(QUIC_STREAM_EX, StreamID);
    ABI_OFFSET(QUIC_STREAM_EX, Data);
    ABI_LAYOUT(QUIC_HEADER_INVARIANT);
    ABI_OFFSET(QUIC_HEADER_INVARIANT, LONG_HDR.Version);
    ABI_OFFSET(QUIC_HEADER_INVARIANT, LONG_HDR.DestCidLength);
    ABI_LAYOUT(QUIC_LONG_HEADER_V1);
    ABI_OFFSET(QUIC_LONG_HEADER_V1, Version);
    ABI_OFFSET(QUIC_LONG_HEADER_V1, DestCidLength);
    ABI_LAYOUT(QUIC_SHORT_HEADER_V1);
    ABI_LAYOUT(CXPLAT_POOL_HEADER);
    QUIC_STREAM_FRAME_TYPE stream;
    memset(&stream, 0, sizeof(stream));
    stream.FIN = 1; stream.LEN = 1; stream.OFF = 1; stream.FrameType = 1;
    abi_bytes("QUIC_STREAM_FRAME_TYPE", &stream, sizeof(stream));
    QUIC_DATAGRAM_FRAME_TYPE datagram;
    memset(&datagram, 0, sizeof(datagram));
    datagram.LEN = 1; datagram.FrameType = 24;
    abi_bytes("QUIC_DATAGRAM_FRAME_TYPE", &datagram, sizeof(datagram));
    QUIC_LONG_HEADER_V1 header;
    memset(&header, 0, sizeof(header));
    header.PnLength = 2; header.Type = 1; header.FixedBit = 1; header.IsLongHeader = 1;
    header.Version = 0x01020304; header.DestCidLength = 0x12;
    abi_bytes("QUIC_LONG_HEADER_V1", &header, sizeof(header));
    QUIC_SHORT_HEADER_V1 short_header;
    memset(&short_header, 0, sizeof(short_header));
    short_header.PnLength = 3; short_header.KeyPhase = 1; short_header.SpinBit = 1; short_header.FixedBit = 1;
    abi_bytes("QUIC_SHORT_HEADER_V1", &short_header, sizeof(short_header));
    return 0;
}
