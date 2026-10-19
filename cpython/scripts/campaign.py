"""CPython's exact-pin embedded GIL profile; ordinary framework execution."""
from campaigns.model import Consumer, Recipe, Source, Suite
from campaigns.recipes import json_file, helper


def sources(root):
    pin = json_file(root / "config/source.json")
    yield Source("product", pin["url"], root / "ref" / pin["archive"],
                 root / "ref" / pin.get("cache_directory", pin["directory"]), pin["directory"], pin.get("sha256"),
                 tuple(pin["required"]), archive_paths=tuple(pin.get("archive_paths", ())))


def prepare(ctx):
    return helper(ctx.root / "scripts/prepare.py").prepare(ctx)


def validate_options(options):
    # The CLI validates 1..16 (default 4). Private object writers share read
    # leases; native builds and managed builds keep their separate job bounds.
    if options.rid != "linux-x64":
        raise ValueError("CPython embedded-gil currently selects linux-x64 only")


def abi(ctx, suite):
    return helper(ctx.root / 'tests/Abi/campaign_hooks.py').run_suite(ctx, suite)


def assets(ctx, suite):
    helper(ctx.root / "scripts/native.py").ensure(ctx)
    module = helper(ctx.root / "scripts/prepare.py")
    for form in ("raw", "processed"):
        manifest = module.stage_assets(ctx, ctx.work / form, form)
        import json
        (ctx.artifacts / ("assets-" + form + ".json")).write_text(json.dumps({**manifest, "status":"staged-unpublished"}, indent=2) + "\n")
    ctx.receipt["scope"] = "asset staging and manifests only; no product publication or runtime qualification"


def inventory(ctx, suite):
    return helper(ctx.root / "scripts/inventory.py").freeze(ctx)


def native(ctx, suite):
    return helper(ctx.root / "scripts/native.py").run_suite(ctx, suite)


def upstream(ctx, suite):
    helper_profile(ctx, suite)
    return helper(ctx.root / 'tests/Upstream/campaign_hooks.py').run_suite(ctx, suite)


def helper_profile(ctx, suite):
    return helper(ctx.root / 'scripts/helper_profile.py').run_suite(ctx, suite)


def specialist(ctx, suite):
    hooks = helper(ctx.root / 'tests/Delivery/campaign_hooks.py')
    action = {'behavior': hooks.run_behavior, 'threads': hooks.run_behavior,
              'lifecycle': hooks.run_behavior, 'delivery': hooks.run_delivery,
              'audit': hooks.run_audit, 'boundary': hooks.run_boundary}[suite.name]
    return action(ctx, suite)


recipe = Recipe(
    "cpython", "TranslatedCPython", ("embedded-gil", "upstream-tests-static"),
    ("src/Managed.CPython/Managed.CPython.csproj", "samples/ManagedConsumer/ManagedConsumer.csproj"),
    sources, prepare, consumer=Consumer(property="CPythonProject", arguments=("{work}/{label}/cpython/stdlib", "{work}/{label}-workflow")),
    suites=(Suite("native", (), run=native, platforms=("linux",), scope="same-pin native controls; no translated qualification"),
            Suite("assets", (), run=assets, scope="stdlib/notices staging only; no published product"),
            Suite("inventory", (), run=inventory, platforms=("linux",), scope="freeze manifests from same-pin resolved build inputs"),
            Suite("consumer", ("consumer",), matrix=True),
            Suite("abi", (), run=abi, matrix=True, scope="required actual translated layout/refcount/slot/varargs/vectorcall gate; existing same-pin native controls"),
            Suite("helper-profile", (), run=helper_profile, matrix=True, verify_profiles=("upstream-tests-static",),
                  scope="required existing test-profile source roles/emission/compiled static registrations; no interpreter execution"),
            Suite("upstream", (), run=upstream, matrix=True, verify_profiles=("upstream-tests-static",),
                  timeout=900, scope="frozen required cases inside separate actual translated static test profile"),
            *(Suite(name, (), run=specialist, matrix=True, profiles=("embedded-gil",),
                    scope="required actual-product behavior/delivery/audit; no native fallback")
              for name in ("behavior", "threads", "lifecycle", "delivery", "audit", "boundary"))),
    default_suites=("consumer",), verify_suites=("native", "abi", "consumer", "behavior", "threads", "lifecycle", "helper-profile", "upstream", "delivery", "audit", "boundary"),
    validate_options=validate_options,
)
