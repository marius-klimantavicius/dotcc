"""Inspect current inputs; inventory success never means complete translation."""
from campaigns.inputs import acquire
from campaigns.identity import write_json
from campaigns.recipes import json_file


def run(ctx, suite=None):
    source = acquire(next(ctx.recipe.sources(ctx.root)), ctx.policy, ctx.options.fetch)
    policy = json_file(ctx.root / "config/translation.json")
    paths = sorted(path.relative_to(source).as_posix() for path in source.rglob("*.c"))
    units = policy["diagnostic_units"]
    if len(units) != len(set(units)) or any(name not in paths for name in units):
        raise RuntimeError("Missing or duplicate real diagnostic translation units")
    manifest = json_file(ctx.root / policy["backend_manifest"])
    backend = [name for context in manifest["contexts"] for name in context["units"]]
    if len(backend) != len(set(backend)) or len(backend) != manifest["backend_unit_count"]:
        raise RuntimeError("Duplicate or incomplete frozen backend source selection")
    # Generated parser/catalog C may be absent in an unconfigured reference;
    # preparation materializes and verifies actual translation paths.
    pin = json_file(ctx.root / "config/sources.json")
    if manifest["revision"] != pin["product"]["revision"]:
        raise RuntimeError("Frozen backend manifest revision differs from selected product")
    frontend = json_file(ctx.root / "src/Initialization/frontend-closure.json")
    if frontend["revision"] != manifest["revision"] or frontend["profile"] != manifest["profile"]:
        raise RuntimeError("Frontend/module inputs differ from the selected backend source/profile")
    frontend_units = [unit["source"] for unit in frontend["units"]]
    module_units = frontend["bootstrap_modules"]["plpgsql"]["sources"]
    if len(frontend_units) != len(set(frontend_units)) or len(module_units) != 6:
        raise RuntimeError("Missing or duplicate initializer/module inventory")
    report = dict(status="passed", scope="source inventory; managed behavioral qualification is separate",
                  revision=pin["product"]["revision"], source=str(source),
                  source_files=paths, source_c_count=len(paths), diagnostic_units=units,
                  frozen_backend_units=len(backend), complete_backend_link_closure=True,
                  generated_backend_inputs=[name for name in backend if name not in paths],
                  frontend_units=frontend_units, required_module_units=module_units,
                  complete_product_manifest=policy["complete_product_manifest"],
                  notices=pin["product"]["notices"],
                  embedding_guard_files=sorted(path.relative_to(source).as_posix()
                      for path in source.rglob("*.c")
                      if "__PGLITE__" in path.read_text(errors="replace")))
    path = ctx.artifacts / "source-inventory.json"
    write_json(path, report)
    ctx.receipt["pglite_inventory"] = str(path)
    ctx.receipt["scope"] = report["scope"]
