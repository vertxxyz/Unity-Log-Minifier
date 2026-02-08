# Unity Log Minifier
Console app that shortens `.log` files by collapsing repeats.

## Requirements
- .NET 10.

## Usage
1. Drag `*.log` files onto the executable.
1. Files are output to `*_minified.log` and opened.

## Capabilities
### Collapses and annotates the following
- Repeated sections of text separated by multiple newlines.
- Repeated single lines text.

### Under consideration
- Collapsing larger groups of multiple-newline-separated text.
- Collapsing multiple-line sections of text without multiple-newline separation.