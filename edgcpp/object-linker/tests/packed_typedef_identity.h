#pragma once
namespace Public {
typedef struct { unsigned char tag; unsigned value; } __attribute__((packed)) Packet;
}
namespace Other {
typedef struct { unsigned char tag; unsigned value; } __attribute__((packed)) Packet;
}
namespace {
typedef struct { unsigned char tag; unsigned value; } __attribute__((packed)) PrivatePacket;
static PrivatePacket private_packet = { 2, 17 };
}
extern Public::Packet packet;
extern Other::Packet other_packet;
Public::Packet *get_packet();
Other::Packet *get_other_packet();
void *get_private_packet();
int read_private_packet();
