# Pinned inputs

The campaign uses upstream libsmb2 commit
`99d5cffc85e4aa8d517649568ff8ec2008e35e90`, initially selected for the parser probe.
The [source manifest](../config/source.json) records the immutable codeload URL,
extraction directory, and SHA-256
`e67e8803969336a50b3c7063870376f944a0ab9638cd39629fc8fcd76bff9031`.
`scripts/fetch.sh` verifies the archive and every extracted file before reuse.
No upstream source changes are applied.

The [pinned library build list](https://github.com/sahlberg/libsmb2/blob/99d5cffc85e4aa8d517649568ff8ec2008e35e90/lib/CMakeLists.txt)
supplies the 53-unit [source closure](../config/sources.json). Its two share-enum
wrappers include unchanged C files from `libdcerpc/`; the full DCE/RPC library is
disabled. Example programs are not used as the managed product.

Upstream `COPYING` assigns LGPL-2.1-or-later to the SMB library and non-DCE/RPC
headers, BSD-2-Clause to DCE/RPC code/headers and examples. Preserve individual
source notices and the included `LICENCE-LGPL-2.1.txt` with redistributions. The
archive remains intact under ignored `ref/`.

The independent server is Samba `2:4.19.5+dfsg-4ubuntu9.7` from Ubuntu packages.
Its [container recipe](../tests/Samba/Dockerfile) pins the Ubuntu base image digest
and the Samba, samba-common-bin, and samba-libs package versions. The oracle
receipt records the resulting immutable image ID, recipe hashes, server version,
and complete installed package inventory. Transitive distribution dependencies
are recorded but are not all separately version-locked; retain the built image ID
for exact binary replay. Native oracle dependencies never enter the managed product.

The oracle creates a disposable account and tmpfs share inside its own container,
publishes port 445 on a dynamically allocated loopback-only host port, and removes
the container and password file on completion. It changes no host Samba account,
system service, or SMB share. Docker and Ubuntu's package repositories are needed
only for the independent-server test setup.
