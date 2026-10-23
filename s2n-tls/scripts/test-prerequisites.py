"""Prepare isolated Linux test peers; no system installation or product backend."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys

sys.path.insert(0, str(Path(__file__).resolve().parents[2] / "Scripts"))
from campaigns.identity import HashPolicy
from campaigns.inputs import acquire
from campaigns.recipes import load

ROOT = Path(__file__).resolve().parents[1]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--fetch", choices=("missing", "never"), default="missing")
    args = parser.parse_args()
    metadata = json.loads((ROOT / "config/native-prerequisites.json").read_text())
    work = ROOT / "build/test-prerequisites"
    packages, target = work / "packages", work / "root"
    packages.mkdir(parents=True, exist_ok=True)
    target.mkdir(exist_ok=True)
    output = ROOT / "artifacts/test-prerequisites"
    output.mkdir(parents=True, exist_ok=True)
    report = {"status": "running", "commands": []}

    def run(argv, *, cwd=ROOT, environment=None):
        command = list(map(str, argv))
        log = output / (str(len(report["commands"])) + ".log")
        with log.open("w") as stream:
            result = subprocess.run(command, cwd=cwd, env=environment, stdout=stream, stderr=subprocess.STDOUT, timeout=1200)
        report["commands"].append({"argv": command, "cwd": str(cwd), "exit_code": result.returncode, "log": str(log)})
        if result.returncode:
            raise RuntimeError("Test prerequisite failed: " + str(log))

    try:
        sources = {source.name: source for source in load(ROOT.parent, "s2n-tls").sources(ROOT)}
        for name in ("s2n-peer",):
            acquire(sources[name], HashPolicy("warn"), args.fetch)
        backend = ROOT / "build/native/openssl-legacy"
        if not (backend / "libcrypto.a").is_file():
            raise RuntimeError("Build the pinned native oracle first; no system libcrypto fallback")
        old_build = ROOT / "build/native/s2n-peer"
        run(["cmake", "-S", sources["s2n-peer"].directory, "-B", old_build,
             "-DCMAKE_BUILD_TYPE=Release", "-DBUILD_SHARED_LIBS=OFF", "-DBUILD_TESTING=ON",
             "-DS2N_USE_CRYPTO_SHARED_LIBS=OFF", "-DCMAKE_PREFIX_PATH=" + str(backend),
             "-Dcrypto_INCLUDE_DIR=" + str(backend / "include"),
             "-Dcrypto_STATIC_LIBRARY=" + str(backend / "libcrypto.a"),
             "-DS2N_ENFORCE_PROPER_LIBCRYPTO_FEATURE_PROBE=ON"])
        run(["cmake", "--build", old_build, "-j2", "--target", "s2nc", "s2nd"])
        for package in metadata["packages"]:
            path = packages / package["file"]
            if not path.is_file():
                if args.fetch == "never":
                    raise RuntimeError("Missing test-only OS package: " + str(path))
                fields = dict(line.split(": ", 1) for line in package["metadata"].splitlines())
                run(["apt-get", "download", fields["Package"] + "=" + fields["Version"]], cwd=packages)
            if hashlib.sha256(path.read_bytes()).hexdigest() != package["sha256"]:
                raise RuntimeError("Test prerequisite package hash differs: " + str(path))
            run(["dpkg-deb", "-x", path, target])
        python = ROOT / "build/python-integration/bin/python"
        if not python.is_file():
            run(["uv", "venv", python.parent.parent, "--python", "python3.12"])
        sync = ["uv", "pip", "sync", "--python", python, "--require-hashes", ROOT / metadata["python_lock"]]
        if args.fetch == "never":
            sync.append("--offline")
        run(sync)
        java = work / "java"
        java.mkdir(exist_ok=True)
        run(["javac", "-d", java, ROOT / "ref/s2n-tls/tests/integrationv2/bin/SSLSocketClient.java"])
        apache = work / "apache"
        for name in ("conf", "sites-enabled", "run", "log", "lock"):
            (apache / name).mkdir(parents=True, exist_ok=True)
        upstream = ROOT / "ref/s2n-tls"
        shutil.copytree(upstream / "codebuild/bin/apache2/www", apache / "www", dirs_exist_ok=True)
        shutil.copyfile(upstream / "tests/integrationv2/apache2/nix/conf/ports.conf", apache / "conf/ports.conf")
        conf = (upstream / "tests/integrationv2/apache2/nix/conf/apache2.conf").read_text()
        # Use portable copies and the extracted distro module location, retaining
        # the original fixture's HTTP/renegotiation behavior and route assertions.
        conf = conf.replace("${APACHE_NIX_STORE}/modules/", str(target / "usr/lib/apache2/modules") + "/")
        conf = conf.replace("${APACHE_NIX_STORE}/conf/mime.types", str(upstream / "tests/integrationv2/apache2/nix/conf/mime.types"))
        for line in conf.splitlines():
            if line.startswith("LoadModule ") and not Path(line.split()[-1]).is_file():
                # These are built into the Debian executable, not absent features.
                module = line.split()[1]
                if module not in ("log_config_module", "unixd_module"):
                    raise RuntimeError("Required Apache module absent: " + line)
                conf = conf.replace(line + "\n", "")
        conf += "\nServerName localhost\n"
        (apache / "apache2.conf").write_text(conf)
        virtual = (upstream / "codebuild/bin/apache2/sites-enabled/renegotiate.conf").read_text()
        virtual = "\n".join(line + ":@SECLEVEL=0" if line.strip().startswith("SSLCipherSuite ") else line for line in virtual.splitlines()) + "\n"
        (apache / "sites-enabled/renegotiate.conf").write_text(virtual)
        env = {"LD_LIBRARY_PATH": str(target / "usr/lib/x86_64-linux-gnu"),
               "APACHE_SERVER_ROOT": str(apache), "APACHE_RUN_DIR": str(apache / "run"),
               "APACHE_PID_FILE": str(apache / "run/apache2.pid"), "APACHE_LOG_DIR": str(apache / "log"),
               "APACHE_RUN_USER": __import__("pwd").getpwuid(os.getuid()).pw_name,
               "APACHE_RUN_GROUP": __import__("grp").getgrgid(os.getgid()).gr_name,
               "APACHE_CERT_DIR": str(upstream / "tests/pems")}
        server = [str(target / "usr/sbin/apache2"), "-f", str(apache / "apache2.conf"), "-DFOREGROUND"]
        run([*server, "-t"], environment={**os.environ, **env})
        old = ROOT / "build/native/s2n-peer/bin"
        if not all((old / name).is_file() for name in ("s2nc", "s2nd")):
            raise RuntimeError("Build the independently pinned s2n-peer CLI before qualification")
        report["old_peer"] = {"source": sources["s2n-peer"].url, "binaries": {}}
        for name in ("s2nc", "s2nd"):
            binary = old / name
            dependencies = subprocess.check_output(["ldd", str(binary)], text=True)
            if any(name in dependencies for name in ("libcrypto", "libssl", "libs2n")):
                raise RuntimeError("Previous-release peer unexpectedly resolves a dynamic TLS/crypto backend")
            report["old_peer"]["binaries"][name] = {"sha256": hashlib.sha256(binary.read_bytes()).hexdigest(), "ldd": dependencies}
        commands = {"s2nc_head": [str(old / "s2nc")], "s2nd_head": [str(old / "s2nd")],
                    "openssl": [str(ROOT / "build/native/openssl-peer/apps/openssl")],
                    "SSLv3Provider:openssl": [str(ROOT / "build/native/openssl-peer-sslv3/apps/openssl")],
                    "gnutls-cli": [str(target / "usr/bin/gnutls-cli")], "gnutls-serv": [str(target / "usr/bin/gnutls-serv")]}
        for name, command in commands.items():
            if not Path(command[0]).is_file():
                raise RuntimeError("Missing declared independent peer: " + name)
        peers = {"commands": commands, "environment": env, "python": str(python),
                 "assets": [{"source": str(java / "SSLSocketClient.class"), "destination": "tests/integrationv2/bin/SSLSocketClient.class"}],
                 "services": [{"name": "apache-renegotiation", "command": server, "host": "127.0.0.1", "port": 7777}]}
        (work / "peers.json").write_text(json.dumps(peers, indent=2) + "\n")
        report.update(status="passed", peers=str(work / "peers.json"))
    finally:
        (output / "receipt.json").write_text(json.dumps(report, indent=2) + "\n")


if __name__ == "__main__":
    main()
