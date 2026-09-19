FROM ubuntu@sha256:008173c23f95b170204355c12626cb5a965d779a7e1283b09e9cffbb1bf33ca3
ARG SAMBA_VERSION=2:4.19.5+dfsg-4ubuntu9.7
RUN apt-get update && DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
    samba=${SAMBA_VERSION} samba-common-bin=${SAMBA_VERSION} samba-libs=${SAMBA_VERSION} \
    && rm -rf /var/lib/apt/lists/* \
    && useradd --uid 12345 --no-create-home --shell /usr/sbin/nologin smbprobe
COPY smb.conf /etc/samba/smb.conf
COPY entrypoint.sh /entrypoint.sh
ENTRYPOINT ["/bin/sh", "/entrypoint.sh"]
