# Remaining required lwIP work audit

Reviewed 2026-09-29 by the resumed coordinator's read-only scope worker. This
records implementation gaps, not new verification evidence. The complete
[plan](PLAN.md), pinned upstream and [verification policy](../config/verification-policy.json)
remain authoritative. Custom corruption tests are excluded.

The audit inspected profiles, feature and symbol manifests, contrib inventories,
the recipe/feature audit/test dispatcher, original pinned headers/implementations,
and product active-feature reports `123615-d0fe9319`, `123913-71c8944e`,
`124411-8570ce3e`. Source selection, emitted implementation, exercised behavior
and consumer-owned lifetime are separate claims. The current symbol audit proves
only its named definitions; it does not prove every implemented option family.
Coarse feature rows and stale `unrun` fields require reconciliation with receipts.

## Ordered implementation batches

1. Finish owning LOWPAN/ZEP/BLE output/lifecycle and its applicable matrices.
   Introduce option-level feature rows with effective macro, implementation guard,
   designated product profile, behavior suite and lifetime case.
2. Complete HTTP client and proxy CONNECT, then HTTP streaming variants:
   `LWIP_HTTPD_DYNAMIC_FILE_READ`, `LWIP_HTTPD_FILE_STATE`,
   `LWIP_HTTPD_FS_ASYNC_READ`, `LWIP_HTTPD_POST_MANUAL_WND`,
   `LWIP_HTTPD_CGI_SSI`, `LWIP_HTTPD_SSI_RAW`, `HTTPD_USE_MEM_POOL`.
   Use compatible streaming options together and a separate raw-SSI signature
   variant. Original contrib custom/generated-file callbacks need separate
   example products because their `fs_*` symbols overlap. Current host files are
   ready memory blocks, not asynchronous streaming or manual POST credit.
3. Add actual application workflows and owning boundaries: TFTP read/write and
   bounded storage; SNTP time notification; NetBIOS queries; mDNS service lifecycle;
   MQTT connection/pub/sub/QoS/keepalive; SMTP completion/refusal; lwiperf transfer
   and abort; SNMP v1/v2c get/set/walk/trap and v3 noAuthNoPriv including the
   existing netconn/threadsync variant. Original unit tests do not replace these
   product workflows. SNTP currently uses the default no-op system-time macro;
   supply owner-bound clock notifications and timestamp callbacks. Designate
   `SNTP_SERVER_DNS`, `SNTP_COMP_ROUNDTRIP` and implemented response check level 2;
   levels 3/4 are upstream TODOs. Add `LWIP_NETBIOS_RESPOND_NAME_QUERY` NBSTAT response
   coverage; ordinary NB address responses are already enabled without it. Stop/drain each protocol's real callback/PCB/workers before release.
4. Add a compatible enhanced threaded profile for `LWIP_TCP_SACK_OUT`,
   `LWIP_TCP_TIMESTAMPS`, `TCP_LISTEN_BACKLOG`, `LWIP_WND_SCALE`,
   `LWIP_SO_LINGER`, `LWIP_NETBUF_RECVINFO`, `SO_REUSE_RXTOALL`,
   `LWIP_FIONREAD_LINUXMODE`, `LWIP_TCPIP_TIMEOUT` and
   `LWIP_NETIF_REMOVE_CALLBACK`. Negotiate/use each option with coherent
   pool/window sizing. Diagnostic window-scale/receive-info coverage alone is
   not product coverage.
5. Add interface/addressing variants: `ETHARP_SUPPORT_VLAN`, `LWIP_VLAN_PCP`,
   `ETHARP_SUPPORT_STATIC_ENTRIES`, `DNS_LOCAL_HOSTLIST`, dynamic local hosts,
   `LWIP_DNS_SUPPORT_MDNS_QUERIES`, nonzero `LWIP_ND6_RDNSS_MAX_DNS_SERVERS`,
   `LWIP_DHCP_BOOTP_FILE`, `LWIP_MIB2_CALLBACKS`. Cover SLIP ISR-style queued and
   single-pending valid-frame admission through NO_SYS host entry, without
   claiming hardware interrupt delivery. Complete owning SLIP/PPP examples,
   IPv6 data, reconnect and callback shutdown.
6. Add distinct allocator configurations rather than a Cartesian product:
   libc heap/allocated memp (`MEM_LIBC_MALLOC=1`, `MEMP_MEM_MALLOC=1`), and
   custom pools (`MEM_USE_POOLS=1`, `MEMP_USE_CUSTOM_POOLS=1`,
   `MEMP_MEM_MALLOC=0`) with an authored pool list. Reconcile MPU, custom
   timers/allocators, socket header/naming aliases, offload/CRC/padding as explicit
   alternative host contracts. The implemented event API is incompatible with
   callback API and ALTCP; represent it as a small separate raw-event product
   with explicit compatibility scope. It remains a dependency-free implemented
   API requirement; incompatibility with the baseline is not an exclusion.
7. Complete contrib closure: inventory marks 32 C files required; selected list
   contains 15 (11 apps and four addons), leaving 17 example C files unselected.
   Deliver original callback examples in compatible separate products and run
   netconn/socket/raw app workflows. Qualify DHCP option hooks, real static-route
   packet selection and valid TCP MD5 signed exchanges. Existing addon ABI/ISN
   controls do not close these behaviors.
8. Refresh ownership examples, option/import/adaptation inventories and final
   all-profile matrices; rooted AOT, clean/local-input regeneration and Windows
   orchestration remain required acceptance work.

## Source anchors and compatibility

HTTP option contracts: original `src/include/lwip/apps/httpd_opts.h`,
`src/apps/http/{fs.c,httpd.c}` and `contrib/examples/httpd/`. Core/API options:
`src/include/lwip/opt.h`, `src/core/{tcp.c,tcp_in.c,tcp_out.c,netif.c}` and
`src/api/{sockets.c,api_msg.c,tcpip.c}`. Address/link options:
`src/netif/{ethernet.c,slipif.c}`, `src/core/{dns.c,netif.c}`,
`src/core/ipv4/{etharp.c,dhcp.c}`, `src/core/ipv6/nd6.c` and
`src/include/netif/slipif.h`. Allocator/event incompatibilities are asserted in
`src/core/init.c`; do not suppress those assertions to combine options.

Retain external TLS/mbedTLS, external crypto/EAP SRP, external SNMP auth/privacy,
Bonjour resolver, miniz/zlib, OS/device product ports and PPP multilink's unported
TDB dependency exclusions. Preserve upstream-unimplemented CBCP/ECP/demand,
PPPoE server, stateful DHCPv6 and unsupported payload compressors as such.
Bundled PPP crypto remains required. No excluded case is an acceptance pass.

Suggested verification uses coherent original or independently encoded valid
messages, ordinary API/negotiation outcomes, bounded loss/reordering and lifecycle
controls. No custom corruption exception is proposed by this audit.
