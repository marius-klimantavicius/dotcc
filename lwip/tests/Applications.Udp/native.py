#!/usr/bin/env python3
"""Native original-source UDP application scenarios and guarded product boundaries.

Builds its own reviewed prerequisites;
it never invokes another oracle's test workload as an implicit prerequisite.
"""
import argparse
import concurrent.futures
import hashlib
import importlib.util
import json
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[2]
HERE = Path(__file__).resolve().parent
CASES = ("tftp-server", "tftp-get", "tftp-put", "sntp", "netbios", "mdns", "owned-tftp")


def module(name, path):
    spec = importlib.util.spec_from_file_location(name, path)
    result = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(result)
    return result


def run(profile, case, output, product_boundaries=False):
    if profile not in ("threaded", "nosys", "threaded-locking"):
        raise ValueError("Declare native applicability before selecting another profile")
    if case not in CASES:
        raise ValueError("Unknown application case: " + case)
    output = Path(output).resolve()
    output.mkdir(parents=True, exist_ok=False)
    work = output / "build"
    work.mkdir(exist_ok=True)
    pin = json.loads((ROOT / "config/sources.json").read_text())
    source = ROOT / "ref" / pin["directory"]
    contract = module("udp_application_contract", ROOT / "scripts/udp-app-contract.py").check_contract(ROOT, source) if product_boundaries else None
    policy_path = ROOT / "config/verification-policy.json"
    policy = json.loads(policy_path.read_text())["udp_application_draft_snapshot"]
    verified = []
    for section, prefix in (("paths", ROOT), ("native_dependency_paths", ROOT), ("original_units", source)):
        for item in policy[section]:
            path = prefix / item["path"]
            actual = hashlib.sha256(path.read_bytes()).hexdigest()
            if actual != item["sha256"]:
                raise RuntimeError("Reviewed input changed before execution: " + str(path))
            verified.append(path)
    module("udp_profiles", ROOT / "scripts/profiles.py").write_header(ROOT, source, profile, work)
    stage = module("udp_stage", ROOT / "scripts/stage.py")
    staged, adaptations = stage.product_units(ROOT, source, work / "source")
    hop_flags = module("udp_hop", ROOT / "scripts/ip6-hop.py").native_flags(ROOT, source)
    groups = json.loads((ROOT / "config/source-groups.json").read_text())
    required = {"lwipcore_SRCS", "lwipcore4_SRCS", "lwipcore6_SRCS", "lwipapi_SRCS", "lwipppp_SRCS",
                "lwipsntp_SRCS", "lwiptftp_SRCS", "lwipnetbios_SRCS", "lwipmdns_SRCS"}
    upstream = [name for group, names in groups.items() if group in required for name in names]
    upstream.append("src/netif/ethernet.c")
    paths = [staged.get(name, source / name) for name in upstream]
    if product_boundaries:
        paths = [path for path in paths if path.name != "tftp.c"]
        paths.append(ROOT / "src/port/udp_app_services.c")
    paths += [ROOT / "src/Host/bridge.c", ROOT / "tests/NativeOracle/sys_arch.c",
              HERE / "native-main.c", HERE / "native-tftp.c", HERE / "native-services.c", HERE / "sntp-clock.c"]
    inputs = set(verified + paths + [policy_path, work / "profile.h"])
    inputs.update((source / "src/include").rglob("*.h"))
    inputs.update(path for path in (ROOT / "src/port").rglob("*") if path.is_file())
    inputs.update((work / "source").rglob("*.c"))
    frozen = [{"path": str(path), "sha256": hashlib.sha256(path.read_bytes()).hexdigest()} for path in sorted(inputs)]
    (output / "inputs.json").write_text(json.dumps({"profile": profile, "case": case, "inputs": frozen}, indent=2) + "\n")
    base = ["cc", "-std=c17", "-D_POSIX_C_SOURCE=200809L", "-O1", "-g", "-ffunction-sections", "-fdata-sections",
            "-pthread", "-include", "errno.h", "-I" + str(work), "-I" + str(HERE),
            "-I" + str(ROOT / "src/port"), "-I" + str(source / "src/include"), "-I" + str(source / "src")]
    commands = []
    for index, path in enumerate(paths):
        flags = ["-DDOTCC_TCPIP_WORKER=1"] if path.name == "tcpip.c" else []
        if path.name == "ip6.c":
            flags += hop_flags
        if path.name == "sntp.c":
            flags += ["-include", str(ROOT / "src/port/udp_app_services.h" if product_boundaries else HERE / "sntp-clock.h")]
        if product_boundaries and path.name == "native-tftp.c":
            flags += ["-include", str(HERE / "native-product-adapter.h")]
        if product_boundaries and path.name == "sys_arch.c":
            flags += ["-Dsys_now=fixture_real_sys_now"]
        if product_boundaries and path.name == "native-main.c":
            flags += ["-DDOTCC_UDP_VIRTUAL_CLOCK=1"]
        commands.append(base + flags + ["-c", str(path), "-o", str(work / f"{index}.o")])
    (output / "compile-commands.json").write_text(json.dumps(commands, indent=2) + "\n")
    report = {"case": case, "profile": profile, "status": "failed", "source": str(source),
              "scope": "Original native application with authored UDP payload peer; not product owning-service acceptance",
              "adaptations": [entry for entry in adaptations if entry["path"] in upstream],
              "clock_boundary": "test-only NTP notification observer; no operating-system clock change"}
    report["product_boundaries"] = product_boundaries
    report["product_contract"] = contract
    try:
        def compile_one(command):
            result = subprocess.run(command, capture_output=True, text=True, timeout=120)
            if result.returncode:
                raise RuntimeError(result.stdout + result.stderr)
        with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:
            list(pool.map(compile_one, commands))
        executable = work / "applications-udp"
        link = ["cc", "-pthread", "-Wl,--gc-sections", *[str(work / f"{i}.o") for i in range(len(paths))], "-o", str(executable)]
        (output / "link-command.json").write_text(json.dumps(link, indent=2) + "\n")
        linked = subprocess.run(link, capture_output=True, text=True, timeout=60)
        (output / "link.log").write_text(linked.stdout + linked.stderr)
        linked.check_returncode()
        control = subprocess.run([str(executable), "--assert-control"], capture_output=True, text=True, timeout=10)
        (output / "assert-control.log").write_text(control.stdout + control.stderr)
        if control.returncode != 92 or "FAIL" not in control.stderr:
            raise AssertionError("Native assertion control did not fail as expected")
        result = subprocess.run([str(executable), case], capture_output=True, text=True, timeout=40)
        (output / "stdout.jsonl").write_text(result.stdout)
        (output / "stderr.log").write_text(result.stderr)
        if result.returncode:
            raise RuntimeError(f"Native {case} exit {result.returncode}: {result.stderr}")
        observation = json.loads(result.stdout)
        if observation.get("case") != case or observation.get("status") != "passed":
            raise AssertionError("Missing completed native application observation")
        report.update(observation)
        report.update(assert_control_exit=control.returncode, compiler=subprocess.check_output(["cc", "--version"], text=True).splitlines()[0])
    except BaseException as error:
        report["error"] = str(error)
        raise
    finally:
        changed = [item["path"] for item in frozen if hashlib.sha256(Path(item["path"]).read_bytes()).hexdigest() != item["sha256"]]
        if changed:
            report.update(status="failed", changed_inputs=changed)
        (output / "report.json").write_text(json.dumps(report, indent=2) + "\n")
        if changed:
            raise RuntimeError("Frozen inputs changed during native execution")
    print(json.dumps(report))
    return report


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--profile", default="threaded")
    parser.add_argument("--case", required=True, choices=CASES)
    parser.add_argument("--output", required=True)
    parser.add_argument("--product-boundaries", action="store_true")
    args = parser.parse_args()
    run(args.profile, args.case, args.output, args.product_boundaries)
