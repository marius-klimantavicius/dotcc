/* Explicit managed-consumer roots. These public header types and the inline
   loopback helper are used by the BCL host/facade, outside EDG's C call graph.
   Keep their native declarations as the single ABI source of truth. */
#include "precomp.h"
void EdgMsQuicRetainPublicTypes(QUIC_HANDSHAKE_INFO *info, CXPLAT_SEND_FLAGS flags)
{
    (void)info;
    (void)flags;
}
void (*const EdgMsQuicLoopbackAddress)(QUIC_ADDR *) = QuicAddrSetToLoopback;
