#!/usr/bin/env bash
set -euo pipefail
campaign=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)
archive="$campaign/ref/musl-1.2.5.tar.gz"
mkdir -p "$campaign/ref" "$campaign/build/musl" "$campaign/build/guest" "$campaign/artifacts/guest"
if [[ ! -f "$archive" ]]; then
  curl --fail --location --retry 2 --max-time 120 https://musl.libc.org/releases/musl-1.2.5.tar.gz -o "$archive.part"
  mv "$archive.part" "$archive"
fi
printf '%s  %s\n' a9a118bbe84d8764da0ea0d28b3ab3fae8477fc7e4085d90102b8596fc7c75e4 "$archive" | sha256sum --check --status
# Rebuild from the verified archive, never from stale objects or an edited ref.
rm -rf "$campaign/build/musl" "$campaign/build/musl-install" "$campaign/build/musl-source"
mkdir -p "$campaign/build/musl" "$campaign/build/musl-source"
tar -xzf "$archive" -C "$campaign/build/musl-source" --strip-components=1
python3 - "$campaign" <<'PY'
import hashlib, json, pathlib, sys
p = pathlib.Path(sys.argv[1]); lock = json.loads((p / 'tests/ServiceFixture/inputs.json').read_text())
for name, expected in lock['sources'].items():
    actual = hashlib.sha256((p / 'tests/ServiceFixture' / name).read_bytes()).hexdigest()
    if actual != expected: raise SystemExit('guest source hash mismatch: ' + name)
PY
cd "$campaign/build/musl"
timeout 60 env -u CC -u CFLAGS -u CPPFLAGS -u LDFLAGS "$campaign/build/musl-source/configure" --prefix="$campaign/build/musl-install" --disable-shared > "$campaign/artifacts/guest/musl-configure.log" 2>&1
timeout 300 make -j"${JOBS:-4}" > "$campaign/artifacts/guest/musl-build.log" 2>&1
timeout 60 make install > "$campaign/artifacts/guest/musl-install.log" 2>&1
flags=(-std=c11 -D_POSIX_C_SOURCE=200809L -O2 -static -fno-pie -no-pie -Wl,--build-id=none -Wall -Wextra -Werror)
timeout 30 "$campaign/build/musl-install/bin/musl-gcc" "${flags[@]}" "$campaign/tests/ServiceFixture/service.c" -o "$campaign/build/guest/service"
readelf -h -l "$campaign/build/guest/service" > "$campaign/artifacts/guest/elf.txt"
python3 - "$campaign" "${flags[@]}" <<'PY'
import hashlib, json, pathlib, shutil, struct, subprocess, sys
p = pathlib.Path(sys.argv[1]); exe = p / 'build/guest/service'; data = exe.read_bytes()
assert data[:6] == b'\x7fELF\x02\x01' and struct.unpack_from('<HH', data, 16) == (2, 62), 'expected Linux x64 ELF64 ET_EXEC'
phoff = struct.unpack_from('<Q', data, 32)[0]
phsize, phnum = struct.unpack_from('<HH', data, 54)
assert all(struct.unpack_from('<I', data, phoff+i*phsize)[0] != 3 for i in range(phnum)), 'dynamic interpreter forbidden'
digest = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
tools = {}
for name in ('cc', 'ld', 'as', 'ar', 'make'):
    path = pathlib.Path(shutil.which(name)).resolve()
    tools[name] = {'path': str(path), 'sha256': digest(path), 'version': subprocess.check_output([str(path), '--version'], text=True).splitlines()[0]}
cc1 = pathlib.Path(subprocess.check_output(['cc', '-print-prog-name=cc1'], text=True).strip()).resolve()
tools['cc1'] = {'path': str(cc1), 'sha256': digest(cc1)}
receipt = {'fixture': 'static-musl-http-v1', 'elf': 'ELF64 little-endian x86-64 ET_EXEC, no PT_INTERP', 'executable_sha256': digest(exe), 'source_sha256': digest(p/'tests/ServiceFixture/service.c'), 'musl_archive_sha256': digest(p/'ref/musl-1.2.5.tar.gz'), 'musl_libc_sha256': digest(p/'build/musl-install/lib/libc.a'), 'flags': sys.argv[2:], 'tools': tools}
(p/'artifacts/guest/build.json').write_text(json.dumps(receipt, indent=2)+'\n')
lock = json.loads((p/'tests/ServiceFixture/inputs.json').read_text())
if receipt['executable_sha256'] != lock['qualified_executable_sha256']:
    raise SystemExit('guest executable differs from qualified pin; inspect build.json and explicitly qualify this toolchain before updating inputs.json')
print(json.dumps({'executable': str(exe), 'sha256': receipt['executable_sha256']}))
PY
