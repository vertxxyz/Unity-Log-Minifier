# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.1.0]
### Added
- Added second pass that collapses all repeated lines.
- Added the ability to skip specific reduction passes.
### Fixed
- Fixed a bug where some newlines were eaten in non-repeated sections.
- Fixed a bug where the last section of a file wasn't output.

## [1.0.0]
- Initial release.