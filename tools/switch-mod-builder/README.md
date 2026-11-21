# Switch clothing parameter mod builder (experimental)

A small, dependency-free Python CLI for editing **decoded JSON** clothing parameters in VS Code. It is intentionally separate from the existing .NET solution.

## Important limitations

- This tool **does not decode or encode binary** `.bgyml` / BYML files.
- First use a converter compatible with the exact game and version to decode the file to JSON; then re-encode the edited JSON using that converter.
- Some converters wrap values in type metadata or use different field layouts. The tool supports only plain JSON objects with one clothing parameter containing `VariationColor` and `SortID` or `ModelName`.
- Re-encoding may require preserving original types, field order, and other format metadata. Confirm compatibility before using on a console.
- Field names and effects are **hypotheses**, not verified gameplay behavior. Changing `SortID` might not change UI order; `ModelName` may require matching model and skeleton assets.
- Keep original files backed up. Test offline, one change at a time. Do not commit game assets or copyrighted dumps.

## Use in Visual Studio Code

Open the repository folder, then open a terminal:

```powershell
cd tools/switch-mod-builder
python -m unittest -v
python mod_builder.py clothes.decoded.json clothes.sort10.json --sort-id 10
python mod_builder.py clothes.decoded.json clothes.red.json --rgba 1 0 0 1
python mod_builder.py clothes.decoded.json clothes.model.json --model-name "Work/Model/Player/Clothes/Example/output/Example.fmdb"
```

These are **examples only**; `clothes.decoded.json` must be supplied by you after decoding your own original file. The tool never overwrites an existing output file or modifies the input.

## Workflow

1. Make a backup of the original `.bgyml` file.
2. Decode it with a converter verified for your game's BGYML format.
3. Inspect the decoded JSON and run the script.
4. Compare the output with the original JSON.
5. Re-encode with the same compatible converter; validate the result.
6. Install only at the correct game-specific mod path and test offline.

The `atmosphere` root shown on an SD card is **not** the correct destination for a loose `.bgyml` file; a supported game-specific mod layout is required.
