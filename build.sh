#!/bin/bash

if ! [ -x "$(command -v mono)" ]; then
  echo >&2 "Could not find 'mono' on the path."
  exit 1
fi

echo ""
echo "Installing KRE 1.0 beta 3..."
echo ""

. tools/kvm.sh
kvm install 1.0.0-beta3
if [ $? -ne 0 ]; then
  echo >&2 "KRE 1.0 beta 3 installation has failed."
  exit 1
fi

echo ""
echo "Restoring packages..."
echo ""

kpm restore
if [ $? -ne 0 ]; then
  echo >&2 "Package restore has failed."
  exit 1
fi

echo ""
echo "Building packages..."
echo ""

pushd src/xunit.runner.aspnet
kpm pack
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
