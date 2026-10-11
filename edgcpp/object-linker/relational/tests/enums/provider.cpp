#include "program.h"

Status next_status(Status value) {
    if (value == Status::idle) return Status::ready;
    return Status::idle;
}

int score(Packet *packet) {
    int result = packet->code + static_cast<int>(packet->status);
    packet->status = next_status(packet->status);
    return result;
}

Wide wide_value() { return Wide::high; }
