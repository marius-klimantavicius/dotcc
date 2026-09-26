"""picotls source selection and campaign recipe."""
from campaigns.model import Consumer, Recipe, Source, Suite, Translation, Unit
from campaigns.recipes import flags, json_file, lines, link_flags


def sources(root):
    pins = json_file(root / "config/inputs.json")
    core = pins["picotls"]
    for name, pin in pins.items():
        destination = root / "ref" / core["directory"]
        if name != "picotls":
            destination /= "deps/" + name
        yield Source(name, pin["url"], root / "ref" / (pin["directory"] + ".tar.gz"),
                     destination, pin["directory"], pin["sha256"],
                     ("include/picotls.h",) if name == "picotls" else ("picotest.h",),
                     "product" if name == "picotls" else "tests")


def prepare(ctx):
    root, reference = ctx.root, ctx.sources["picotls"]
    core = lines(root / "config/core-sources.txt")
    hosts = lines(root / "config/host-sources.txt")
    wrappers = json_file(root / "config/core-wrappers.json")
    if not set(wrappers).issubset(core) or not set(wrappers.values()).issubset(hosts):
        raise RuntimeError("Core wrappers must name selected upstream and authored units")
    if len(set(wrappers.values())) != len(wrappers):
        raise RuntimeError("Duplicate core wrapper")
    units = [Unit(reference / name) for name in core if name not in wrappers]
    units += [Unit(root / name) for name in hosts]
    return Translation(units, ["-std=c17", *flags(lines(root / "config/core-defines.txt"),
                       [reference / "include", reference])], link_flags("PicoTls", "Managed.Security"))


def script_arguments(ctx, suite, script, args):
    if script == "test-campaign.py":
        if ctx.options.form == "all":
            args.append("--all")
        elif ctx.options.form == "raw":
            args.append("--raw")
        if ctx.options.mode in ("aot", "all"):
            args.append("--aot")
        args += ["--runtime", ctx.options.rid]
    return args


recipe = Recipe("picotls", "TranslatedPicotls", ("default",),
                ("src/BclProvider/BclProvider.csproj", "samples/ManagedConsumer/ManagedConsumer.csproj"),
                sources, prepare,
                script_arguments=script_arguments, suites=(Suite("consumer", ("consumer",), matrix=True),
                        Suite("qualification", ("script", "test-campaign.py"), 14400,
                              platforms=("linux",), scope="ABI, vectors, TLS and peer matrix"),
                        Suite("oracle", ("script", "oracle.sh"), 1800, platforms=("linux",)),
                        Suite("audit", ("script", "audit-product.py"), 1800)),
                consumer=Consumer(project="tests/CopiedConsumer/CopiedConsumer.csproj", property="PicotlsProject", build_solution=True),
                default_suites=("consumer",), verify_suites=("oracle", "qualification", "audit"))
