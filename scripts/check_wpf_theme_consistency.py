#!/usr/bin/env python3
"""Lightweight static consistency checks for WPF theme wiring."""

from __future__ import annotations

import re
import sys
import xml.etree.ElementTree as et
from pathlib import Path


REPO_ROOT = Path(__file__).resolve().parents[1]
APP_XAML = REPO_ROOT / "src/DocuDesk.Desktop/App.xaml"
MAIN_WINDOW_XAML = REPO_ROOT / "src/DocuDesk.Desktop/MainWindow.xaml"


def extract_merged_dictionary_sources(app_tree: et.ElementTree) -> list[str]:
    root = app_tree.getroot()
    return [
        elem.attrib["Source"]
        for elem in root.findall(".//{http://schemas.microsoft.com/winfx/2006/xaml/presentation}ResourceDictionary")
        if "Source" in elem.attrib
    ]


def extract_style_keys(theme_file: Path) -> set[str]:
    root = et.parse(theme_file).getroot()
    x_ns = "{http://schemas.microsoft.com/winfx/2006/xaml}"
    return {
        elem.attrib[f"{x_ns}Key"]
        for elem in root.findall(".//{http://schemas.microsoft.com/winfx/2006/xaml/presentation}Style")
        if f"{x_ns}Key" in elem.attrib
    }


def extract_staticresource_keys(window_xaml_text: str) -> set[str]:
    return set(re.findall(r"\{StaticResource\s+([^}]+)\}", window_xaml_text))


def main() -> int:
    errors: list[str] = []

    app_tree = et.parse(APP_XAML)
    main_window_text = MAIN_WINDOW_XAML.read_text(encoding="utf-8")

    merged_sources = extract_merged_dictionary_sources(app_tree)
    expected = {
        "Resources/DocuDesk.DesignTokens.xaml",
        "Resources/DocuDesk.ModernTheme.xaml",
    }

    missing_expected = expected.difference(merged_sources)
    if missing_expected:
        errors.append(f"App.xaml is missing expected merged dictionaries: {sorted(missing_expected)}")

    for source in merged_sources:
        theme_file = APP_XAML.parent / source
        if not theme_file.exists():
            errors.append(f"Merged dictionary source does not exist: {source}")

    theme_file = APP_XAML.parent / "Resources/DocuDesk.ModernTheme.xaml"
    style_keys = extract_style_keys(theme_file) if theme_file.exists() else set()
    referenced_keys = extract_staticresource_keys(main_window_text)

    style_like_references = {
        key
        for key in referenced_keys
        if key.endswith("Style") and key not in {"x:Static"}
    }
    missing_styles = sorted(style_like_references.difference(style_keys))
    if missing_styles:
        errors.append(f"MainWindow references missing style keys: {missing_styles}")

    if 'x:Name="DocumentViewer"' not in main_window_text:
        errors.append('MainWindow is missing required viewer name: x:Name="DocumentViewer"')

    if errors:
        print("❌ WPF theme consistency check failed:")
        for error in errors:
            print(f" - {error}")
        return 1

    print("✅ WPF theme consistency check passed.")
    print(f"Merged dictionaries: {merged_sources}")
    print(f"Style keys in theme: {sorted(style_keys)}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
