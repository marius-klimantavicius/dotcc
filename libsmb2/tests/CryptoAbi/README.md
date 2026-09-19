# Portable crypto and ABI checks

`../../scripts/crypto-abi.sh` compiles a native oracle from the pinned, unchanged
upstream portable crypto sources, verifies it against `expected.txt`, then builds
and runs this consumer against `generated/TranslatedLibsmb2`. Add `--aot` to
publish and run the same checks with NativeAOT. `--native-only` tests only the
native oracle and makes no claim about generated code. `--generated-project`
allows the same consumer to test an explicitly selected raw/staging project.
Use `--label raw` with that override to keep its receipts separate; the default
label is `final`.
The ABI baseline is intentionally Linux x64; other targets need their own baseline.

The 45 checks cover:

- SHA-256/SHA-512 `abc` and a 257-byte, incrementally supplied message.
- The RSA Data Security, Inc. MD4 Message-Digest Algorithm and MD5 `abc` vectors.
- HMAC-MD5/HMAC-SHA-256 `Hi There`, and a SHA-256 HMAC key larger than its block.
- AES-128 ECB, CMAC empty/block messages, and CCM ciphertext/tag, decryption,
  and rejection of a modified tag.
- Native sizes and critical offsets of digest contexts, `smb2_iovec`,
  `smb2_stat_64`, `smb2_statvfs`, `smb2_header`, and `smb2_pdu`.
- A callback invoked through the actual `smb2_pdu.cb` field with an opaque pointer,
  a 64-bit cookie, and high-bit NTSTATUS; access-denied status-to-errno mapping.

SHA, MD5, and HMAC baselines were independently checked with Python `hashlib` and
`hmac`. AES baselines were independently checked with Python `cryptography`
(AES ECB, CMAC, and AESCCM); that package is not required to run the campaign.
The MD4 value is the standard `abc` known answer. ABI expectations were captured
from the native Linux x64 oracle. Tests fail on missing, extra, or differing keys;
they never regenerate the committed expectations automatically.

The native executable links only the tested upstream crypto/error compilation
units with section garbage collection; it does not replace the campaign's full
53-unit translation profile. The managed consumer references the complete
translated product and calls its implementations directly. Both stages record
commands, input hashes, binary hashes, and transcripts under
`artifacts/crypto-abi/<label>/`. These checks do not establish SMB interoperability,
cryptographic certification, timing behavior, or production security readiness.
