#!/bin/bash

if ! [ -x "$(command -v mono)" ]; then
  echo >&2 "Could not find 'mono' on the path."
  exit 1
fi

echo ""
echo "Installing .NET Execution Environment..."
echo ""

. tools/dnvm.sh
dnvm install latest
if [ $? -ne 0 ]; then
  echo >&2 ".NET Execution Environment installation has failed."
  exit 1
fi

echo ""
echo "Restoring packages..."
echo ""

dnu restore
if [ $? -ne 0 ]; then
  echo >&2 "Package restore has failed."
  exit 1
fi

echo ""
echo "Building packages..."
echo ""

pushd src/xunit.runner.aspnet
dnu pack
if [ $? -ne 0 ]; then
  popd
  echo >&2 "Build packages has failed."
  exit 1
fi

echo ""
echo "Running tests..."
echo ""
cd ../../test/test.xunit.runner.aspnet
k test -parallel none
if [ $? -ne 0 ]; then
  popd
  echo >&2 "Running tests has failed."
  exit 1
fi

popd
