#ifndef DOTCC_LWIP_CONTRIB_HOOKS_H
#define DOTCC_LWIP_CONTRIB_HOOKS_H
#include "addons/tcp_isn/tcp_isn.h"
#include "addons/tcp_md5/tcp_md5.h"
#include "addons/ipv6_static_routing/ip6_route_table.h"
#include "addons/dhcp_extra_opts/dhcp_extra_opts.h"
/* LWIP_HOOK_TCP_ISN is defined by profile.h: tcp_isn.c also guards its body
 * with that option before any LWIP_HOOK_FILENAME include. */
#define LWIP_HOOK_TCP_INPACKET_PCB(pcb, hdr, optlen, opt1len, opt2, p) tcp_md5_check_inpacket(pcb, hdr, optlen, opt1len, opt2, p)
#define LWIP_HOOK_TCP_OUT_TCPOPT_LENGTH(pcb, internal_len) tcp_md5_get_additional_option_length(pcb, internal_len)
#define LWIP_HOOK_TCP_OUT_ADD_TCPOPTS(p, hdr, pcb, opts) tcp_md5_add_tx_options(p, hdr, pcb, opts)
#define LWIP_HOOK_SOCKETS_SETSOCKOPT(s, sock, level, optname, optval, optlen, err) tcp_md5_setsockopt_hook(sock, level, optname, optval, optlen, err)
#define LWIP_HOOK_IP6_ROUTE(src, dest) ip6_static_route(src, dest)
#define LWIP_HOOK_ND6_GET_GW(netif, dest) ip6_get_gateway(netif, dest)
#define LWIP_HOOK_DHCP_PARSE_OPTION(netif, dhcp, state, msg, msg_type, option, len, pbuf, offset) dhcp_parse_extra_opts(dhcp, state, option, len, pbuf, offset)
#define LWIP_HOOK_DHCP_APPEND_OPTIONS(netif, dhcp, state, msg, msg_type, options_len_ptr) dhcp_append_extra_opts(netif, state, msg, options_len_ptr)
#endif
