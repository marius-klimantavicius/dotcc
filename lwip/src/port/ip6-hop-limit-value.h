#ifndef DOTCC_LWIP_HOP_LIMIT_VALUE_H
#define DOTCC_LWIP_HOP_LIMIT_VALUE_H
/* Only ip6.c's integer decrement can be negative; its ordinary output input is u8_t. */
static inline unsigned char lwip_port_hop_limit(int value)
{
  return value < 0 ? 0 : (unsigned char)value;
}
#endif
