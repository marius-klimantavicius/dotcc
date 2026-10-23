"""Prove C/header/flag equivalence without rewriting a native control receipt.

Managed provider bodies and DotCC code are deliberately outside this proof:
their new executions remain required. Only the native assertion oracle is reused.
"""
import argparse
import hashlib
import json
from pathlib import Path
import tarfile

PROFILE_KEYS = ("abi", "compiler_definitions", "undefined_features", "forced_include", "header_api", "header_source",
                "identity", "macro", "include_directories", "required_legacy", "native_version_prefix")
CAPABILITY_KEYS = ("abi", "advertised_mask", "required_mask", "initialization_options_mask", "ocsp_verification_flags",
                   "verification_flags", "excluded_unselected_flags")


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def load(path):
    return json.loads(Path(path).read_text())


def receipt_path(root, campaign):
    return root / "artifacts/campaign" / Path(campaign).name / "receipt.json"


def c_flags(command):
    flags = []
    index = 0
    while index < len(command):
        flag = command[index]
        if flag in ("-I", "-isystem", "-include", "-imacros", "-D", "-U"):
            flags.extend(command[index:index + 2]); index += 2
        else:
            if flag.startswith(("-D", "-U", "-std=", "-I")):
                flags.append(flag)
            index += 1
    return flags


def normalize(value, campaign):
    prefix = str(Path(campaign).resolve())
    return "$CAMPAIGN" + value[len(prefix):] if value == prefix or value.startswith(prefix + "/") else value


def semantics(profile):
    return {**{key: profile[key] for key in PROFILE_KEYS},
            "capability_contract": {key: profile["capability_contract"][key] for key in CAPABILITY_KEYS}}


def validate_profile(root, campaign, native_path, equivalence_path=None, *, deep=True):
    root, campaign, native_path = Path(root), Path(campaign).resolve(), Path(native_path).resolve()
    native = load(native_path)
    target = load(receipt_path(root, campaign))
    first = next(c["command"] for c in target["commands"] if "--emit=obj" in c["command"])
    if Path(native["campaign"]).resolve() == campaign:
        if native["declaration_flags"] != c_flags(first) or semantics(native["provider_profile"]) != semantics(target["provider_configuration"]):
            raise RuntimeError("Native C declarations/features differ from the exact product profile")
        return {"native_campaign": native["campaign"], "comparison": "identical campaign/profile"}
    if not equivalence_path:
        raise RuntimeError("Different campaign paths require a checked --native-equivalence receipt")
    proof = load(equivalence_path)
    if (proof["status"] != "passed" or Path(proof["target_campaign"]).resolve() != campaign
        or Path(proof["native_campaign"]).resolve() != Path(native["campaign"]).resolve()
        or proof["native_report_sha256"] != sha(native_path)
        or proof["target_campaign_receipt_sha256"] != sha(receipt_path(root, campaign))
        or proof["native_campaign_receipt_sha256"] != sha(receipt_path(root, native["campaign"]))):
        raise RuntimeError("Native equivalence receipt identity differs from actual inputs")
    if deep:
        for path, expected in proof["inputs_sha256"].items():
            if sha(path) != expected:
                raise RuntimeError("Native C/header equivalence input changed: " + path)
    return {"native_campaign": native["campaign"], "comparison": "checked C/header/effective-flag equivalence",
            "equivalence_receipt": str(Path(equivalence_path).resolve()), "equivalence_receipt_sha256": sha(equivalence_path)}


def prove(root, campaign, native_path):
    root, campaign, native_path = Path(root).resolve(), Path(campaign).resolve(), Path(native_path).resolve()
    native = load(native_path); original = Path(native["campaign"]).resolve()
    old_path, new_path = receipt_path(root, original), receipt_path(root, campaign)
    old, new = load(old_path), load(new_path)
    if native["status"] != "passed" or old["status"] != "passed" or new["status"] != "passed":
        raise RuntimeError("Native control and both complete product receipts must pass")
    if semantics(native["provider_profile"]) != semantics(old["provider_configuration"]) or semantics(old["provider_configuration"]) != semantics(new["provider_configuration"]):
        raise RuntimeError("Actual provider C ABI/features/identity changed")
    inputs = {str(native_path): sha(native_path), str(old_path): sha(old_path), str(new_path): sha(new_path),
              str(Path(__file__).resolve()): sha(__file__)}
    comparisons = []

    def tree(directory, predicate):
        result = {}
        for path in sorted(directory.rglob("*")):
            if path.is_symlink():
                raise RuntimeError("Symlink in frozen C/header profile: " + str(path))
            if path.is_file() and predicate(path):
                result[path.relative_to(directory).as_posix()] = sha(path)
                inputs[str(path)] = result[path.relative_to(directory).as_posix()]
        return result

    for folder, predicate in (("source", lambda p: p.suffix in (".c", ".h", ".pem", ".der", ".crl")),
                              ("include", lambda p: True)):
        before, after = tree(original / folder, predicate), tree(campaign / folder, predicate)
        if before != after:
            changed = [p for p in set(before) | set(after) if before.get(p) != after.get(p)]
            raise RuntimeError("Staged C/header/fixture content differs: " + repr(sorted(changed)[:12]))
        comparisons.append({"tree": folder, "identical_files": len(before), "sha256_by_relative_path": before})
    def commands_by_unit(record, directory):
        mapped = {}
        observed = set()
        for observation in record["commands"]:
            command = observation["command"]
            if "--emit=obj" not in command:
                continue
            sources = [str(Path(arg).resolve()) for arg in command if arg.endswith(".c") and not arg.startswith("-")]
            if len(sources) != 1:
                raise RuntimeError("Emission command must identify one actual C source argument")
            unit = sources[0]; key = normalize(unit, directory)
            if key in mapped:
                raise RuntimeError("Duplicate actual emitted TU: " + unit)
            mapped[key] = (unit, command); observed.add(unit)
        if observed != {str(Path(unit).resolve()) for unit in record["units"]} or len(mapped) != 175:
            raise RuntimeError("Actual command sources must cover all 175 manifest TUs exactly")
        return mapped
    old_commands = commands_by_unit(old, original)
    new_commands = commands_by_unit(new, campaign)
    if old_commands.keys() != new_commands.keys():
        raise RuntimeError("Actual emitted TU sets differ")
    effective = []
    for key in sorted(old_commands):
        old_unit, before = old_commands[key]
        new_unit, after = new_commands[key]
        left = [normalize(flag, original) for flag in c_flags(before)]
        right = [normalize(flag, campaign) for flag in c_flags(after)]
        if left != right:
            raise RuntimeError("TU identity/effective C flags changed: " + old_unit)
        if native["declaration_flags"] != c_flags(before):
            raise RuntimeError("This native oracle did not execute a matching per-TU declaration profile: " + old_unit)
        for unit, record in ((old_unit, old), (new_unit, new)):
            path = Path(unit)
            expected = record["input_hashes"][path.relative_to(root).as_posix()]
            if sha(path) != expected:
                raise RuntimeError("Recorded original TU content changed: " + unit)
            inputs[unit] = expected
        effective.append({"unit": normalize(old_unit, original), "flags": left})
    authored = {}
    for form in ("raw", "processed"):
        left = {p: h for p, h in old["authored_inputs_at_configuration"][form].items() if Path(p).suffix in (".c", ".h")}
        right = {p: h for p, h in new["authored_inputs_at_configuration"][form].items() if Path(p).suffix in (".c", ".h")}
        if left != right:
            raise RuntimeError("Historical authored native boundary/header hashes differ")
        for relative, expected in left.items():
            path = root / relative
            if sha(path) != expected:
                raise RuntimeError("Authored C/header changed since native execution: " + relative)
            inputs[str(path)] = expected
        authored[form] = left
    pins = {}
    for name in ("s2n-tls", "openssl-legacy"):
        pin = old["requested_sources"][name]
        if pin != new["requested_sources"][name] or sha(pin["archive"]) != pin["sha256"]:
            raise RuntimeError("Pinned native C/header archive identity changed: " + name)
        inputs[pin["archive"]] = pin["sha256"]; pins[name] = pin
    # Prove unchanged external OpenSSL headers against the actual pinned archive,
    # rather than trusting their shared include-directory pathname.
    include = Path(pins["openssl-legacy"]["directory"]) / "include"
    headers = tree(include, lambda p: p.suffix == ".h")
    with tarfile.open(pins["openssl-legacy"]["archive"], "r:gz") as archive:
        archived = {m.name.split("/include/", 1)[1]: m for m in archive if m.isfile() and "/include/" in m.name and m.name.endswith(".h")}
        for relative, expected in headers.items():
            if relative not in archived or hashlib.sha256(archive.extractfile(archived[relative]).read()).hexdigest() != expected:
                raise RuntimeError("OpenSSL header differs from pinned archive: " + relative)
    backend = root / "build/native/openssl-legacy/libcrypto.a"
    if sha(backend) != native["native_backend_sha256"]:
        raise RuntimeError("Actual native control backend changed")
    inputs[str(backend)] = native["native_backend_sha256"]
    required = [r["path"] for r in load(root / "config/upstream-tests.json")["cases"]
                if r["status"].startswith("required") and r["path"].endswith(".c")]
    actual = [r["case"] for r in native["upstream"] if r["status"] == "passed"]
    if set(actual) != set(required) or len(actual) != len(required):
        raise RuntimeError("Native reference does not qualify the full required C inventory")
    for row in native["upstream"]:
        path = root / "ref/s2n-tls" / row["case"]
        if sha(path) != row["original_sha256"] or sha(row["compiled_source"]) != row["compiled_sha256"]:
            raise RuntimeError("Original native assertion source changed: " + row["case"])
        inputs[str(path)] = row["original_sha256"]; inputs[row["compiled_source"]] = row["compiled_sha256"]
    return {"status": "passed", "scope": "Native assertion-oracle C/header/effective-flag equivalence only; new emitted/managed executions remain required",
            "native_campaign": str(original), "target_campaign": str(campaign), "native_report": str(native_path),
            "native_report_sha256": sha(native_path), "native_campaign_receipt_sha256": sha(old_path),
            "target_campaign_receipt_sha256": sha(new_path), "units": 175, "native_required_c_cases": len(required),
            "proof_generator_sha256": sha(__file__),
            "staged_trees": comparisons, "effective_tu_flags": effective, "historical_authored_c_headers": authored,
            "provider_c_semantics": semantics(new["provider_configuration"]), "pinned_sources": pins,
            "pinned_openssl_headers": len(headers), "inputs_sha256": inputs,
            "non_c_metadata_differences": [key for key in old["provider_configuration"]
                if old["provider_configuration"][key] != new["provider_configuration"][key]]}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--campaign-run", type=Path, required=True)
    parser.add_argument("--native-report", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args(); root = Path(__file__).resolve().parents[2]
    args.output.parent.mkdir(parents=True, exist_ok=True)
    try:
        result = prove(root, args.campaign_run, args.native_report)
    except Exception as error:
        args.output.write_text(json.dumps({"status": "failed", "error": str(error)}, indent=2) + "\n")
        raise
    args.output.write_text(json.dumps(result, indent=2) + "\n")
    print("Native C/header/flag equivalence PASS:", args.output)


if __name__ == "__main__":
    main()
