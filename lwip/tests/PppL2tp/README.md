# Independent L2TP peer

`ppp-l2tp` independently encodes RFC2661 control AVPs, validates mutual MD5 tunnel
challenges, rejects a wrong challenge, and negotiates real LCP/IPCP/IPv6CP. It
sends and decodes inner IPv4 UDP in both directions, rejects wrong tunnel/session
IDs, and observes TermAck followed by StopCCN. Heap, pbuf and timeout counters
return to their initial values. Native and translated consumers use the same
wire contract; BCL MD5 is a test-peer dependency only, while translated L2TP uses
bundled upstream crypto. This does not qualify every timeout/reconnect case or
an owning public PPP session API.
