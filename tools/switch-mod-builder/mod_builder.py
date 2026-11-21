"""Edit decoded clothing parameter JSON without touching the original BGYML binary.

Use a compatible BYML/BGYML converter separately to decode and re-encode game data.
"""
from __future__ import annotations

import argparse
import copy
import json
from pathlib import Path
from typing import Any

EDITABLE = {"SortID", "ModelName", "Gender", "BodyHideType", "IsDefault",
            "IsAbleCpuBot", "IsInDevelopment", "IsNotReward", "VariationColor"}


def load_document(path: Path) -> dict[str, Any]:
    if path.suffix.lower() != ".json":
        raise ValueError("Input must be decoded JSON; raw .bgyml is not supported")
    value = json.loads(path.read_text(encoding="utf-8"))
    if not isinstance(value, dict):
        raise ValueError("Expected a JSON object at the root")
    return value


def find_parameter_object(node: Any) -> dict[str, Any]:
    """Find a single object containing the clothing fields; reject ambiguous matches."""
    matches: list[dict[str, Any]] = []

    def visit(value: Any) -> None:
        if isinstance(value, dict):
            if "VariationColor" in value and ("SortID" in value or "ModelName" in value):
                matches.append(value)
            for child in value.values():
                visit(child)
        elif isinstance(value, list):
            for child in value:
                visit(child)

    visit(node)
    if len(matches) != 1:
        raise ValueError(f"Expected exactly one clothing parameter object; found {len(matches)}")
    return matches[0]


def modify(data: dict[str, Any], *, sort_id: int | None = None,
           model_name: str | None = None, color: tuple[float, ...] | None = None) -> dict[str, Any]:
    result = copy.deepcopy(data)
    target = find_parameter_object(result)
    if sort_id is not None:
        if isinstance(target.get("SortID"), bool) or not isinstance(target.get("SortID"), int):
            raise ValueError("SortID is not a plain integer in this decoded document")
        if not 0 <= sort_id <= 2_147_483_647:
            raise ValueError("SortID must be between 0 and 2147483647")
        target["SortID"] = sort_id
    if model_name is not None:
        if not isinstance(target.get("ModelName"), str):
            raise ValueError("ModelName is not a string")
        if not model_name.strip():
            raise ValueError("ModelName cannot be empty")
        target["ModelName"] = model_name
    if color is not None:
        channels = target.get("VariationColor")
        if not isinstance(channels, dict) or not all(key in channels for key in "RGBA"):
            raise ValueError("VariationColor must contain R, G, B, A")
        if any(not 0 <= n <= 1 for n in color):
            raise ValueError("RGBA channels must be between 0 and 1")
        for key, value in zip("RGBA", color):
            if not isinstance(channels[key], (int, float)) or isinstance(channels[key], bool):
                raise ValueError(f"VariationColor.{key} is not numeric")
            channels[key] = value
    return result


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("input", type=Path, help="Decoded JSON from a compatible BYML converter")
    parser.add_argument("output", type=Path, help="New JSON file (must not exist)")
    parser.add_argument("--sort-id", type=int)
    parser.add_argument("--model-name")
    parser.add_argument("--rgba", nargs=4, type=float, metavar=("R", "G", "B", "A"))
    args = parser.parse_args()
    if args.input.resolve() == args.output.resolve():
        parser.error("Input and output must differ")
    if args.output.exists():
        parser.error("Output already exists; refusing to overwrite")
    if not any((args.sort_id is not None, args.model_name is not None, args.rgba is not None)):
        parser.error("Supply at least one edit flag")
    try:
        result = modify(load_document(args.input), sort_id=args.sort_id,
                        model_name=args.model_name,
                        color=tuple(args.rgba) if args.rgba is not None else None)
        args.output.parent.mkdir(parents=True, exist_ok=True)
        with args.output.open("x", encoding="utf-8") as stream:
            json.dump(result, stream, ensure_ascii=False, indent=2)
            stream.write("\n")
    except (ValueError, OSError, json.JSONDecodeError) as exc:
        parser.exit(2, f"Error: {exc}\n")
    print(f"Wrote {args.output}. Re-encode with your game-compatible converter before testing.")


if __name__ == "__main__":
    main()
