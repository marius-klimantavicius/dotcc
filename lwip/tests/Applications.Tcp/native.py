#!/usr/bin/env python3
"""Finite native TCP application controls; exact reviewed policy is required."""
import argparse
import concurrent.futures
import hashlib
import importlib.util
import json
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[2]
HERE = Path(__file__).resolve().parent
CASES = ("mqtt", "smtp", "lwiperf", "http-client", "proxy-connect")


def module(name, path):
    spec = importlib.util.spec_from_file_location(name, path)
    result = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(result)
    return result


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def run(profile, case, output, original_smtp=False):
    if profile not in ("nosys", "threaded", "threaded-locking"):
        raise ValueError("Declare additional profile applicability before execution")
    if case not in CASES:
        raise ValueError("Unknown TCP application case")
    output = Path(output).resolve()
    output.mkdir(parents=True, exist_ok=False)
    work = output / "build"
    work.mkdir()
    pin = json.loads((ROOT / "config/sources.json").read_text())
    source = ROOT / "ref" / pin["directory"]
    policy_path = ROOT / "config/verification-policy.json"
    policy = json.loads(policy_path.read_text()).get("tcp_application_draft_snapshot")
    if policy is None:
        raise RuntimeError("TCP drafts require source review and a tcp_application_draft_snapshot policy inventory before any build")
    verified = []
    for section, prefix in (("paths", ROOT), ("native_dependency_paths", ROOT), ("original_units", source)):
        for item in policy[section]:
            path = prefix / item["path"]
            if sha(path) != item["sha256"]:
                raise RuntimeError("Reviewed input changed: " + str(path))
            verified.append(path)
    module("tcp_profiles", ROOT / "scripts/profiles.py").write_header(ROOT, source, profile, work)
    staged, adaptations = module("tcp_stage", ROOT / "scripts/stage.py").product_units(ROOT, source, work / "source")
    hop_flags = module("tcp_hop", ROOT / "scripts/ip6-hop.py").native_flags(ROOT, source)
    groups = json.loads((ROOT / "config/source-groups.json").read_text())
    selected = {"lwipcore_SRCS", "lwipcore4_SRCS", "lwipcore6_SRCS", "lwipapi_SRCS", "lwipppp_SRCS",
                "lwipmqtt_SRCS", "lwipsmtp_SRCS", "lwipiperf_SRCS", "lwipsntp_SRCS"}
    upstream = [name for group, names in groups.items() if group in selected for name in names]
    upstream += ["src/netif/ethernet.c", "src/apps/http/http_client.c", "src/apps/http/altcp_proxyconnect.c"]
    if original_smtp:
        if case != "smtp":
            raise ValueError("Original SMTP comparison applies only to smtp")
        staged.pop("src/apps/smtp/smtp.c", None)
    paths = [staged.get(name, source / name) for name in upstream]
    paths += [ROOT / "src/Host/bridge.c", ROOT / "tests/NativeOracle/sys_arch.c"]
    paths += [HERE / name for name in ("native-main.c", "native-peer.c", "native-mqtt.c", "native-smtp.c", "native-lwiperf.c", "native-http.c", "native-proxy.c")]
    inputs = set(verified + paths + [policy_path, work / "profile.h"])
    inputs.update((source / "src/include").rglob("*.h"))
    inputs.update(path for path in (ROOT / "src/port").rglob("*") if path.is_file())
    inputs.update((work / "source").rglob("*.c"))
    inputs.update(path for path in HERE.iterdir() if path.is_file())
    frozen = [{"path": str(path), "sha256": sha(path)} for path in sorted(inputs)]
    (output / "inputs.json").write_text(json.dumps({"profile": profile, "case": case, "pin": pin, "inputs": frozen}, indent=2) + "\n")
    base = ["cc", "-std=c17", "-D_POSIX_C_SOURCE=200809L", "-O1", "-g", "-ffunction-sections", "-fdata-sections", "-pthread", "-include", "errno.h",
            "-I" + str(work), "-I" + str(HERE), "-I" + str(ROOT / "src/port"), "-I" + str(source / "src/include")]
    commands = []
    for index, path in enumerate(paths):
        flags = ["-DDOTCC_TCPIP_WORKER=1"] if path.name == "tcpip.c" else []
        if path.name == "ip6.c":
            flags += hop_flags
        if path.name == "sys_arch.c":
            flags += ["-Dsys_now=fixture_real_sys_now"]
        commands.append(base + flags + ["-c", str(path), "-o", str(work / f"{index}.o")])
    (output / "compile-commands.json").write_text(json.dumps(commands, indent=2) + "\n")
    report = {"case": case, "profile": profile, "status": "failed", "source": str(source),
              "original_smtp": original_smtp,
              "scope": "Finite original TCP application controls; not product owning-service acceptance",
              "adaptations": [entry for entry in adaptations if entry["path"] in upstream and not (original_smtp and entry["path"] == "src/apps/smtp/smtp.c")]}
    try:
        def compile_one(item):
            index, command = item
            result = subprocess.run(command, capture_output=True, text=True, timeout=120)
            (output / f"compile-{index}.log").write_text(result.stdout + result.stderr)
            return result.returncode
        with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:
            statuses = list(pool.map(compile_one, enumerate(commands)))
        if any(statuses):
            report["compile_exits"] = statuses
            raise RuntimeError("Native compile failed; exact per-command logs preserved")
        executable = work / "applications-tcp"
        link = ["cc", "-pthread", "-Wl,--gc-sections", *[str(work / f"{i}.o") for i in range(len(paths))], "-o", str(executable)]
        (output / "link-command.json").write_text(json.dumps(link, indent=2) + "\n")
        linked = subprocess.run(link, capture_output=True, text=True, timeout=60)
        (output / "link.log").write_text(linked.stdout + linked.stderr)
        linked.check_returncode()
        controls = [[str(executable), "--assert-control"], [str(executable), case]]
        (output / "execution-commands.json").write_text(json.dumps(controls, indent=2) + "\n")
        control = subprocess.run(controls[0], capture_output=True, text=True, timeout=10)
        (output / "assert-control.log").write_text(control.stdout + control.stderr)
        if control.returncode != 92 or "FAIL" not in control.stderr:
            raise AssertionError("Native assertion control must fail before initialization without protocol input")
        result = subprocess.run(controls[1], capture_output=True, text=True, timeout=90)
        (output / "stdout.jsonl").write_text(result.stdout)
        (output / "stderr.log").write_text(result.stderr)
        report["process_exit"] = result.returncode
        if result.returncode:
            raise RuntimeError(f"Native {case} failed with exit {result.returncode}")
        observation = json.loads(result.stdout)
        if observation.get("case") != case or observation.get("status") != "passed":
            raise AssertionError("No matching completed native application observation")
        report.update(observation)
        report["assert_control_exit"] = control.returncode
        report["compiler"] = subprocess.check_output(["cc", "--version"], text=True).splitlines()[0]
    except BaseException as error:
        report["error"] = str(error)
        raise
    finally:
        changed = [entry["path"] for entry in frozen if sha(Path(entry["path"])) != entry["sha256"]]
        if changed:
            report.update(status="failed", changed_inputs=changed)
        (output / "report.json").write_text(json.dumps(report, indent=2) + "\n")
        if changed:
            raise RuntimeError("Frozen input changed during native qualification")
    print(json.dumps(report))


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--profile", default="nosys")
    parser.add_argument("--case", required=True, choices=CASES)
    parser.add_argument("--output", required=True)
    parser.add_argument("--original-smtp", action="store_true")
    args = parser.parse_args()
    run(args.profile, args.case, args.output, args.original_smtp)
