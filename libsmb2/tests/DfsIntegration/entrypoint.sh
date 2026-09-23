#!/bin/sh
set -eu
mkdir -p /run/samba /var/lib/samba/private /var/cache/samba /var/log/samba /srv/dfs
password=$(cat /oracle-password)
printf '%s\n%s\n' "$password" "$password" | smbpasswd -s -a smbprobe >/dev/null
unset password
server=$(hostname -i)
for share in alpha beta; do
    mkdir -p "/srv/$share/資料"
    printf '%s' "$share" > "/srv/$share/資料/marker.txt"
done
ln -s "msdfs:$server\\alpha" /srv/dfs/link-a
ln -s "msdfs:$server\\beta" /srv/dfs/link-b
exec smbd --foreground --no-process-group --debug-stdout
