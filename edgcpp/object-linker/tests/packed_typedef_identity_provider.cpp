#include "packed_typedef_identity.h"
Public::Packet packet = { 3, 31 };
Other::Packet other_packet = { 5, 47 };
Public::Packet *get_packet() { return &packet; }
Other::Packet *get_other_packet() { return &other_packet; }
void *get_private_packet() { return &private_packet; }
int read_private_packet() { return private_packet.value; }
