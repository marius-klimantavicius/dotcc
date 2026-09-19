#!/bin/sh
set -eu
mkdir -p /run/samba /var/lib/samba/private /var/cache/samba /var/log/samba /srv/share
password=$(cat /oracle-password)
printf '%s\n%s\n' "$password" "$password" | smbpasswd -s -a smbprobe >/dev/null
unset password
chown smbprobe:smbprobe /srv/share
exec smbd --foreground --no-process-group --debug-stdout
