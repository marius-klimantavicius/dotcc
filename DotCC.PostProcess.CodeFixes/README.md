# DotCC post-processing for Rider

This development-only Roslyn package provides two suggestions and code fixes:

- **DCCPP001**: inline proven dotcc `Cond.B` calls, including CBool conversions.
- **DCCPP002**: remove standalone empty blocks, retaining required bodies and trivia.

Use Rider's quick-fix menu (Alt+Enter) to preview and apply an edit in place.
Fix All supports document, project and solution scopes for each rule. No source
is changed by analysis or a build. Generated C# is intentionally analyzed.

Both passes share the standalone postprocessor's safety checks. Unknown helpers,
observable caller-argument text and directives are retained. Documents with
compiler errors are skipped until their bindings are valid.

The package contains only analyzer/code-fix assemblies under
`analyzers/dotnet/cs`; it adds no application runtime dependency. It targets
.NET Standard 2.0 and the Roslyn 4.14 API. Enable Roslyn analyzers in Rider's
Editor | Inspection Settings | Roslyn Analyzers settings if necessary.

The repository guide `docs/postprocess.md` describes local builds, SQLite wiring,
the standalone comparison workflow and diagnostic severity configuration.
