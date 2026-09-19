#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <fcntl.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <unistd.h>
#include "smb2.h"
#include "libsmb2.h"

/* Isolated-share oracle; the password is inherited through the environment. */
int main(int argc, char **argv)
{
    struct smb2_context *ctx = NULL;
    struct smb2fh *file = NULL;
    struct smb2dir *dir = NULL;
    struct smb2_stat_64 stat;
    uint8_t sent[65537], received[65537];
    const char *name = "dotcc-oracle-roundtrip.bin";
    const char *renamed = "dotcc-oracle-renamed.bin";
    const char *password = getenv("LIBSMB2_TEST_PASSWORD");
    int connected = 0, created = 0, moved = 0, rc = 1;
    uint32_t offset;
    int count, entries = 0;
    if (argc != 6 || password == NULL) return 2;
    ctx = smb2_init_context();
    if (!ctx) return 3;
    smb2_set_timeout(ctx, 10);
    smb2_set_user(ctx, "smbprobe");
    smb2_set_password(ctx, password);
    smb2_set_domain(ctx, "WORKGROUP");
    smb2_set_authentication(ctx, SMB2_SEC_NTLMSSP);
    smb2_set_version(ctx, (enum smb2_negotiate_version)strtoul(argv[3], NULL, 16));
    smb2_set_security_mode(ctx, SMB2_NEGOTIATE_SIGNING_ENABLED);
    smb2_set_sign(ctx, 1);
    if (!strcmp(argv[4], "encrypt")) smb2_set_seal(ctx, 1);
    count = smb2_connect_share(ctx, argv[1], argv[2], "smbprobe");
    if (!strcmp(argv[5], "reject")) {
        if (count >= 0) { connected = 1; goto cleanup; }
        printf("rejected:%d\n", count);
        rc = 0;
        goto cleanup;
    }
    if (count < 0) goto cleanup;
    connected = 1;
    if (smb2_get_dialect(ctx) != strtoul(argv[3], NULL, 16)) goto cleanup;
    for (offset = 0; offset < sizeof(sent); offset++) sent[offset] = (uint8_t)(offset * 31 + 17);
    file = smb2_open(ctx, name, O_CREAT | O_EXCL | O_RDWR);
    if (!file) goto cleanup;
    created = 1;
    offset = 0;
    while (offset < sizeof(sent)) {
        count = smb2_pwrite(ctx, file, sent + offset, (uint32_t)sizeof(sent) - offset, offset);
        if (count <= 0) goto cleanup;
        offset += count;
    }
    if (smb2_fsync(ctx, file) < 0 || smb2_fstat(ctx, file, &stat) < 0 || stat.smb2_size != sizeof(sent)) goto cleanup;
    memset(received, 0, sizeof(received));
    offset = 0;
    while (offset < sizeof(received)) {
        count = smb2_pread(ctx, file, received + offset, (uint32_t)sizeof(received) - offset, offset);
        if (count <= 0) goto cleanup;
        offset += count;
    }
    if (memcmp(sent, received, sizeof(sent)) || smb2_pread(ctx, file, received, 1, sizeof(sent)) != 0) goto cleanup;
    if (smb2_ftruncate(ctx, file, 17) < 0 || smb2_fstat(ctx, file, &stat) < 0 || stat.smb2_size != 17) goto cleanup;
    count = smb2_close(ctx, file);
    file = NULL;
    if (count < 0 || smb2_rename(ctx, name, renamed) < 0) goto cleanup;
    moved = 1;
    dir = smb2_opendir(ctx, "");
    if (!dir) goto cleanup;
    struct smb2dirent *entry;
    while ((entry = smb2_readdir(ctx, dir)) != NULL) if (!strcmp(entry->name, renamed)) entries++;
    smb2_closedir(ctx, dir);
    dir = NULL;
    if (entries != 1 || smb2_unlink(ctx, renamed) < 0) goto cleanup;
    created = 0;
    printf("passed:dialect=%04x,protection=%s,bytes=%zu\n", smb2_get_dialect(ctx), argv[4], sizeof(sent));
    rc = 0;
cleanup:
    if (rc) fprintf(stderr, "client failed: %s\n", smb2_get_error(ctx));
    if (dir) smb2_closedir(ctx, dir);
    if (file) smb2_close(ctx, file);
    if (created) smb2_unlink(ctx, moved ? renamed : name);
    if (connected && smb2_disconnect_share(ctx) < 0) rc = 1;
    smb2_destroy_context(ctx);
    return rc;
}
