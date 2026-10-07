"""Assemble the pinned LLVM libc++ source provider for DuckDB.

This module stages source inputs only. A successful assembly is not evidence
that the provider or its consumers have translated or passed execution tests.
"""
import hashlib
import json
from pathlib import Path


# The core profile already supplies allocation, managed EH/atomic/thread ABI,
# strings' exception classes, locale, streams' base classes and synchronization.
# Keep explicit template definitions with those implementations in the primary
# TU. Other implementations retain their upstream TU boundaries by default.
PRIMARY_EXTRA_SOURCES = ("src/string.cpp", "src/vector.cpp", "src/ios.instantiations.cpp")
SEPARATE_SOURCES = (
    "src/bind.cpp", "src/charconv.cpp", "src/ryu/d2fixed.cpp",
    "src/ryu/d2s.cpp", "src/ryu/f2s.cpp", "src/fstream.cpp",
    "src/iostream.cpp", "src/ostream.cpp", "src/random.cpp",
    "src/random_shuffle.cpp", "src/regex.cpp", "src/shared_mutex.cpp",
    "src/strstream.cpp", "src/valarray.cpp",
)


def _sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def prepare_provider(root: Path, acquisition: Path, output: Path,
                     primary_consumers=(), standard="c++20", unity_provider=False):
    """Return verified source/config provenance and ordered translation units.

    ``root`` is the edgcpp directory; ``output`` is a new staging directory.
    ``primary_consumers`` can include a targeted consumer in the primary TU if
    required by EDG template ownership, without modifying its source text.
    """
    root, acquisition, output = map(lambda p: Path(p).resolve(), (root, acquisition, output))
    if standard not in ("c++17", "c++20"):
        raise ValueError("libc++ provider standard must be c++17 or c++20")
    adapter = root / "backend/runtime/libcxx"
    pin = json.loads((adapter / "source.json").read_text())
    acquired = json.loads((acquisition / "acquisition.json").read_text())
    if acquired["source"]["sha256"] != pin["sha256"]:
        raise ValueError("libc++ acquisition does not match the pinned archive")
    upstream = acquisition / pin["archiveRoot"]
    hashes = acquired["fileHashes"]
    verified = {}
    for name, expected in hashes.items():
        if name.startswith(("config/", pin["archiveRoot"] + "/include/", pin["archiveRoot"] + "/src/")):
            if _sha(acquisition / name) != expected:
                raise ValueError("libc++ acquisition file changed: " + name)
            verified[name] = expected
    for prefix in ("config/", pin["archiveRoot"] + "/include/", pin["archiveRoot"] + "/src/"):
        if not any(name.startswith(prefix) for name in verified):
            raise ValueError("libc++ acquisition lacks recorded files under " + prefix)
    # Config is a managed ABI contract, not an arbitrary historical acquisition.
    for path in (adapter / "config").rglob("*"):
        relative = path.relative_to(adapter / "config")
        acquired_path = acquisition / "config" / relative
        if path.is_file() and (not acquired_path.is_file() or _sha(path) != _sha(acquired_path)):
            raise ValueError("libc++ acquisition has an outdated managed configuration: " + str(relative))

    profile = json.loads((adapter / "core-source-profile.json").read_text())
    primary, records = [], []
    for entry in profile["orderedSources"]:
        path = (adapter if entry["base"] == "adapter" else upstream) / entry["path"]
        if _sha(path) != entry["sha256"]:
            raise ValueError("libc++ core source changed: " + str(path))
        primary.append(path)
        records.append({"path": str(path), "sha256": entry["sha256"], "unit": "primary"})

    def implementation(name, unit):
        key = pin["archiveRoot"] + "/" + name
        if key not in verified:
            raise ValueError("Unrecorded libc++ implementation: " + name)
        path = upstream / name
        records.append({"path": str(path), "sha256": verified[key], "unit": unit})
        return path

    primary.extend(implementation(name, "primary") for name in PRIMARY_EXTRA_SOURCES)
    separate = [(name, implementation(name, name)) for name in SEPARATE_SOURCES]
    consumers = [Path(path).resolve() for path in primary_consumers]
    output.mkdir(parents=True, exist_ok=False)

    # Every TU belongs to one managed ABI. Native ABI tags and forced-inlining
    # annotations must not vary between libc++ implementation and application
    # declarations; the CLR chooses inlining after translation. Load __config
    # before the implementation's _LIBCPP_BUILDING_LIBRARY preamble as well.
    uniform_header = Path(__file__).resolve().with_name("libcxx-preinclude.h")

    adaptations = []
    adapted_sources = {}
    charconv_header = None
    if standard == "c++17":
        adapted = output / "adapted"
        adapted.mkdir()
        original = upstream / "src/algorithm.cpp"
        content = original.read_text()
        replacements = {
            "std::__bit_log2(static_cast<size_t>(last - first))":
                "static_cast<size_t>(numeric_limits<size_t>::digits - 1 - std::__countl_zero(static_cast<size_t>(last - first)))",
            "ranges::less": "less<>",
        }
        for old, new in replacements.items():
            expected = 1 if old.startswith("std::__bit_log2") else 3
            if content.count(old) != expected:
                raise ValueError("Pinned algorithm.cpp adaptation no longer matches")
            content = content.replace(old, new)
        copy = adapted / "algorithm.cpp"
        copy.write_text(content)
        adapted_sources[original] = copy
        adaptations.append({"source": str(original), "path": str(copy), "sha256": _sha(copy),
            "kind": "c++17-private-sort-implementation",
            "changes": replacements,
            "contract": "Only upstream explicit arithmetic element instantiations; same < comparisons, count-leading-zero expression and branchless policy"})
        original = upstream / "src/include/to_chars_floating_point.h"
        content = original.read_text()
        replacements = {"std::bit_cast<_Uint_type>(_Value)": "__builtin_bit_cast(_Uint_type, _Value)",
                        "std::bit_cast<_Floating>(_Uint_value)": "__builtin_bit_cast(_Floating, _Uint_value)"}
        if sum(content.count(old) for old in replacements) != 5:
            raise ValueError("Pinned floating charconv bit_cast adaptation no longer matches")
        for old, new in replacements.items():
            content = content.replace(old, new)
        charconv_header = adapted / "to_chars_floating_point.h"
        charconv_header.write_text(content)
        adaptations.append({"source": str(original), "path": str(charconv_header), "sha256": _sha(charconv_header),
            "kind": "c++17-private-bit-cast", "changes": replacements,
            "contract": "Same compiler intrinsic called by LLVM18 std::bit_cast; all five uses cast equal-size arithmetic types"})

    def include(path):
        # These objects already have constant initializers in C++17: null
        # pointers, constexpr category unions, locale storage/IDs and opaque
        # synchronization records. Preserve their definitions and use libc++'s own
        # compatibility spelling, scoped to the implementation that needs it.
        if standard == "c++17" and path in [upstream / "src" / name for name in
                                            ("new_handler.cpp", "ios.cpp", "locale.cpp", "memory.cpp",
                                             "system_error.cpp", "future.cpp", "call_once.cpp", "random_shuffle.cpp")]:
            adaptations.append({"source": str(path), "kind": "scoped-constinit-compatibility",
                                "replacement": "_LIBCPP_CONSTINIT"})
            return ("#define constinit _LIBCPP_CONSTINIT\n#include " + json.dumps(str(path)) +
                    "\n#undef constinit\n")
        prefix = ""
        if path == upstream / "src/charconv.cpp" and charconv_header:
            prefix = "#include " + json.dumps(str(charconv_header)) + "\n"
        suffix = ""
        if path == upstream / "src/iostream.cpp":
            # Upstream ends its TU here. Its private stringizing helper would
            # otherwise rewrite subsequent regex/stream str() declarations.
            suffix = "#undef str\n#undef _str\n#undef _LIBCPP_ABI_NAMESPACE_STR\n"
        return prefix + "#include " + json.dumps(str(adapted_sources.get(path, path))) + "\n" + suffix

    primary_path = output / "provider.cpp"
    if unity_provider:
        ryu = [path for name, path in separate if name.startswith("src/ryu/")]
        rest = [path for name, path in separate if not name.startswith("src/ryu/")]
        primary = [primary[0], *ryu, *primary[1:], *rest]
        separate = []
        for entry in records:
            entry["unit"] = "primary"
    primary_path.write_text("// Pinned libc++ implementation and managed ABI.\n" +
        "#include " + json.dumps(str(uniform_header)) + "\n" +
        "".join(map(include, primary)) +
        "#undef _LIBCPP_BUILDING_LIBRARY\n#undef _LIBCPP_REMOVE_TRANSITIVE_INCLUDES\n" +
        "".join(map(include, consumers)))
    units = [primary_path]
    for name, path in separate:
        wrapper = output / name.removeprefix("src/").replace("/", "-")
        wrapper.write_text("// Preserve the upstream implementation TU boundary.\n" +
                           "#include " + json.dumps(str(uniform_header)) + "\n" +
                           include(adapter / "core-source-preamble.h") + include(path))
        units.append(wrapper)
    manifest = {
        "schema": 1, "kind": "translated-llvm-libcxx", "release": pin["release"],
        "assembly": "unity-provider" if unity_provider else "separate-implementation-units",
        "archiveSha256": pin["sha256"], "standard": standard, "adaptations": adaptations,
        "exceptions": True, "rtti": True, "threads": "managed-external-thread-api",
        "posix": True, "libc": "musl", "defines": [],
        "preincludeHeaders": [str(uniform_header)],
        "preincludeHeaderHashes": {str(uniform_header): _sha(uniform_header)},
        "includeDirectories": [str(acquisition / "config"), str(upstream / "include"), str(upstream / "src"), str(adapter / "include")],
        "translationUnits": [str(path) for path in units],
        "generatedInputs": [{"path": str(path), "sha256": _sha(path)} for path in units],
        "sources": records, "acquisitionHashes": verified,
        "adapterHeaderHashes": {str(path): _sha(path) for path in sorted((adapter / "include").rglob("*")) if path.is_file()},
        "primaryConsumers": [{"path": str(path), "sha256": _sha(path)} for path in consumers],
        "scope": "DuckDB source dependency provider; not the entire libc++ implementation",
        "validation": "Source assembly only; translation and runtime acceptance are recorded separately",
    }
    (output / "provider.json").write_text(json.dumps(manifest, indent=2) + "\n")
    return manifest
