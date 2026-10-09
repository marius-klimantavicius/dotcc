#include "property_names.h"
struct Packet packet = { 7 };
struct Packet event = { 11 };
int other = 13;
int main(void) {
  if (get_packet() != &packet || get_packet__2() != &packet) return 1;
  if (get_event() != &event || get_event()->value != 11) return 2;
  if (get_other() != other) return 3;
  get_packet()->value += 5;
  return packet.value != 12;
}
