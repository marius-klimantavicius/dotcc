#include "packed_typedef_identity.h"
int main() {
  if (get_packet() != &packet || get_other_packet() != &other_packet) return 1;
  if (get_packet()->value != 31 || get_other_packet()->value != 47) return 2;
  get_packet()->value = 101;
  if (packet.value != 101 || other_packet.value != 47) return 3;
  if (get_private_packet() == &private_packet) return 4;
  private_packet.value = 53;
  if (read_private_packet() != 17) return 5;
  return 0;
}
