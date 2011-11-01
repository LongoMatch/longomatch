#!/bin/sh

# Creates a debian package for longomatch and uploads it to the ppa
# USAGE: $./deb-pkg.sh  longomatch 0.16.2 karmic 1

if [ $# -ne 4 ]
then
  echo "Usage: `basename $0` PKG_NAME PKG_VERSION DISTRIBUTION DEB_VERSION"
  exit 1
fi

BUILD_DIR=`pwd`"/build"
PKG_NAME=$1  				# longomatch
PKG_VERSION=$2  			# x.y.z
DIST=$3 				# karmic
DEB_VERSION=$4 				# w
DEB_RELEASE=$DIST$DEB_VERSION 		# karmicw
RELEASE=$PKG_NAME-$PKG_VERSION 		# longomatch-x.y.z
TARBALL=$RELEASE.tar.gz 		# longomatch-x.y.z.tar.gz
ORIG=$RELEASE~$DEB_RELEASE.orig.tar.gz 	# longomatch-x.y.z~karmicw.orig.tar.gz
DEST=$RELEASE~$DEB_RELEASE  		# longomatch-x.y.z~karmicw

mkdir -p $BUILD_DIR
echo "Copy $TARBALL to $BUILD_DIR/$ORIG"
cp $TARBALL $BUILD_DIR/$ORIG
echo `pwd $BUILD_DIR`
cd $BUILD_DIR
echo "Extract $ORIG"
tar xvzf $ORIG
echo "Move $RELEASE to $DEST"
mv $RELEASE $DEST
echo "Copy debian folder to $DEST"
cp -R ../debian $DEST/
cd $DEST
rm debian/changelog
export DEBEMAIL=ylatuya@gmail.com
echo "Create changelog dch --create --empty -v $PKG_VERSION~$DEB_RELEASE  --package $PKG_NAME --distribution $DIST"
dch --create -v $PKG_VERSION~$DEB_RELEASE  --package $PKG_NAME --distribution $DIST
dpkg-buildpackage -S
cd $BUILD_DIR
dput ppa:ylatuya/longomatch-dev $PKG_NAME\_$PKG_VERSION~$DEB_RELEASE\_source.changes


