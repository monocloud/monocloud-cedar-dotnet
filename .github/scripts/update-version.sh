#!/bin/bash
set -e

# 1. Run the actual changeset versioning
pnpm changeset version

# 2. Sync to props file
PACKAGE_VERSION=$(jq -r '.version' package.json)
echo "Syncing Directory.Build.props to $PACKAGE_VERSION"

sed -i "s|<Version>.*</Version>|<Version>$PACKAGE_VERSION</Version>|g" Directory.Packages.props

# 3. Explicitly stage the file so the Action commits it
git add Directory.Packages.props
