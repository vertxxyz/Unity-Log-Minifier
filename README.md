# Unity Log Minifier
Console app that shortens `.log` files by collapsing repeats.

## Requirements
- .NET 10.

## Capabilities
### Collapses and annotates the following
#### First Pass
- Repeated sections of text separated by multiple newlines.
- Repeated single-line text.

#### Second Pass
- Collapses larger groups of repeated lines. Repeats are first come, first served, which may lead to undesirable results. 

### Future considerations
- Looping the second pass until no changes are made to the output.

## Usage
1. Drag `*.log` files onto the executable or pass them as arguments.
1. Files are output to `*_minified.log` and opened.

| Argument             | Description                     |
|----------------------|---------------------------------|
| `--dont-open-file`   | Don't open files after running. |
| `--skip-first-pass`  | Don't run the first pass.       |
| `--skip-second-pass` | Don't run the second pass.      |