#ifndef DOTCC_LWIP_HOP_LIMIT_NATIVE_H
#define DOTCC_LWIP_HOP_LIMIT_NATIVE_H
#include "lwip/prot/ip6.h"
#include "ip6-hop-limit-value.h"
#undef IP6H_HOPLIM_SET
#define IP6H_HOPLIM_SET(hdr, hl) ((hdr)->_hoplim = lwip_port_hop_limit((hl)))
#endif
