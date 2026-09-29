"""libuv all-backend campaign; incomplete publication is rejected explicitly."""
from campaigns.model import Consumer, Recipe, Source, Suite, Translation, Unit
from campaigns.recipes import flags, json_file, link_flags
import platform
import sys


def sources(root):
    pin = json_file(root / "config/sources.json")["product"]
    yield Source("product", pin["url"], root / "ref" / pin["archive"],
                 root / "ref" / pin["directory"], pin["directory"],
                 pin.get("sha256"), tuple(pin["required"]))
    metadata = json_file(root / "config/toolchains.json")
    machine = {"amd64": "x86_64", "arm64": "aarch64"}.get(platform.machine().lower(), platform.machine().lower())
    system = "windows" if sys.platform == "win32" else "macos" if sys.platform == "darwin" else sys.platform
    host = machine + "-" + system
    if host in metadata["hosts"]:
        toolchain = metadata["hosts"][host]
        yield Source("zig", toolchain["url"], root / "ref" / toolchain["archive"],
                     root / "ref" / toolchain["directory"], toolchain["directory"],
                     toolchain["sha256"], (toolchain["executable"], "LICENSE", "lib/include/stddef.h"), group="sdk")


def prepare(ctx):
    source = ctx.sources["product"]
    if ctx.options.action != "probe":
        raise RuntimeError("All-platform aggregation/native bindings are incomplete; refusing to publish a host-only product. See docs/blockers.md. Use probe for diagnostic emission.")
    policy = json_file(ctx.root / "config/translation.json")
    return Translation([Unit(source / path) for path in policy["diagnostic_units"]],
                       ["-std=c11", *flags(policy["diagnostic_defines"],
                         [source / "include", source / "src"])],
                       link_flags("LinuxGlibcX64", "Managed.Libuv.Translated"), objects=True)


def native_baseline(ctx, suite):
    from campaigns.recipes import helper
    helper(ctx.root / "scripts/native-baseline.py").run(ctx)


def source_inventory(ctx, suite):
    ctx.script("inventory.py", "--check", label="source-inventory")


def native_upstream(ctx, suite):
    from campaigns.recipes import helper
    helper(ctx.root / "scripts/native-upstream.py").run(ctx)


def dispatch_foundation(ctx, suite):
    from campaigns.recipes import helper
    helper(ctx.root / "scripts/dispatch-foundation.py").run(ctx)


def foreign_headers(ctx, suite):
    from campaigns.recipes import helper
    helper(ctx.root / "scripts/foreign-headers.py").run(ctx)


def incomplete(ctx, suite):
    raise RuntimeError(f"Required suite {suite.name!r} is not implemented; no qualification claimed. See docs/blockers.md.")


REQUIRED = ("source-coverage", "abi", "dispatch", "consumer", "upstream", "lifecycle",
            "process-signal", "native-calls", "interop", "capabilities", "delivery")


recipe = Recipe("libuv", "TranslatedLibuv", ("native-runtime",),
                ("src/Managed.Libuv.Native/Managed.Libuv.Native.csproj",
                 "samples/ManagedConsumer/ManagedConsumer.csproj"), sources, prepare,
                consumer=Consumer(property="TranslatedProject"),
                suites=(Suite("consumer", ("consumer",), matrix=True),
                        Suite("inventory", (), run=source_inventory,
                              scope="source inventory freshness; not emitted source coverage"),
                        Suite("native-baseline", (), run=native_baseline,
                              scope="pinned native first workflow and native-only ABI measurements"),
                        Suite("native-upstream", (), run=native_upstream, timeout=1200,
                              scope="complete native pinned upstream cases; not translated qualification"),
                        Suite("dispatch-foundation", (), run=dispatch_foundation, timeout=1200,
                              scope="authored native identity/binding JIT/AOT; not translated backend dispatch"),
                        Suite("foreign-headers", (), run=foreign_headers, timeout=1800,
                              scope="real Windows/Darwin target preprocessing, syntax, layout and AST; not runtime qualification"),
                        *(Suite(name, (), run=incomplete, scope="required gate, currently unimplemented")
                          for name in REQUIRED if name != "consumer")),
                default_suites=("consumer",),
                verify_suites=("inventory", "native-baseline", *REQUIRED))
