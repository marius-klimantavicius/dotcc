#include "program.h"

int main() {
    Packet packet;
    packet.status = Status::idle;
    packet.code = negative;
    if (score(&packet) != -1) return 1;
    if (packet.status != Status::ready) return 2;
    if (score(&packet) != 510) return 3;
    if (static_cast<unsigned long long>(wide_value()) != 18446744073709551615ULL) return 4;
    Legacy code = positive;
    if (code != 6) return 5;
    try {
        throw Status::ready;
    } catch (unsigned short &wrong_type) {
        return 6;
    } catch (Status &status) {
        if (status != Status::ready) return 7;
    }
    return 0;
}
